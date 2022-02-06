using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using BlazorJsBindingsGenerator.Compatibility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorJsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
    private static bool IsForGeneration(SyntaxNode node, CancellationToken token)
        => node is ClassDeclarationSyntax
            {
                TypeParameterList: null,
                Modifiers: var modifiers,
            } && modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
              && modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

    private static ClassForGeneration? TryGetClassForGeneration(GeneratorSyntaxContext context, CancellationToken token)
    {
        if (context.Node is ClassDeclarationSyntax
            {
                Parent: BaseNamespaceDeclarationSyntax parentSyntax,
                Identifier.Value: string identifier,
                AttributeLists: var attributeLists,
                Modifiers: var modifiers,
            })
        {
            ClassForGeneration classForGeneration = InitializeClassForGeneration(identifier,
                                                                                 modifiers,
                                                                                 attributeLists,
                                                                                 parentSyntax);

            string prefix = "";

            ExtractGenerationInfo(attributeLists, ref classForGeneration, ref prefix, context.SemanticModel);

            return classForGeneration;
        }

        return null;
    }

    private static bool ShouldGenerate(in ClassForGeneration? cfg) => cfg is { Signatures.Count: > 0 };

    private static ClassForGeneration InitializeClassForGeneration(string identifier,
                                                                   SyntaxTokenList modifiers,
                                                                   SyntaxList<AttributeListSyntax> attributeLists,
                                                                   BaseNamespaceDeclarationSyntax parentSyntax)
    {
        Stack<string> fullIdParts = new();

        fullIdParts.Push(identifier);

        BaseNamespaceDeclarationSyntax? currentParentSyntax = parentSyntax;

        do
        {
            fullIdParts.Push(currentParentSyntax.Name.ToString());
            currentParentSyntax = currentParentSyntax.Parent as BaseNamespaceDeclarationSyntax;
        } while (currentParentSyntax != null);

        return new ClassForGeneration
        {
            Type = new TypeView
            {
                FullId = Compatible.StringJoin('.', fullIdParts),
                ShortId = identifier,
                ContainingParentsPath = Compatible.StringJoin('.', fullIdParts.SkipLast(1)),
                IsPublic = modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)),
            },
            Signatures = new List<Signature>(capacity: attributeLists.Sum(list => list.Attributes.Count)),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExtractGenerationInfo(SyntaxList<AttributeListSyntax> attributeLists,
                                              ref ClassForGeneration classForGeneration,
                                              ref string prefix,
                                              SemanticModel semantics)
    {
        foreach (AttributeListSyntax attributeList in attributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                ExtractGenerationInfo(attribute, ref classForGeneration, ref prefix, semantics);
            }
        }
    }

    private static void ExtractGenerationInfo(AttributeSyntax attribute,
                                              ref ClassForGeneration classForGeneration,
                                              ref string prefix,
                                              SemanticModel semantics)
    {
        if (semantics.GetSymbolInfo(attribute).Symbol is
            {
                ContainingNamespace.Name: Namespace,
                ContainingType.Name: var name,
            })
        {
            switch (name)
            {
                case JsBindingContextAttribute:
                    ParseContext(attribute, ref prefix, semantics);
                    break;

                case JsBindAttribute when TryParseSignature(attribute, prefix, semantics) is {} signature:
                    classForGeneration.Signatures.Add(signature);
                    break;

                case JsBindAttribute:
                    return;
            }
        }
    }

    private static void ParseContext(AttributeSyntax attribute,
                                     ref string prefix,
                                     SemanticModel semantics)
    {
        foreach (AttributeArgumentSyntax arg in attribute.ArgumentList?.Arguments
                                                ?? Enumerable.Empty<AttributeArgumentSyntax>())
        {
            switch (arg.NameEquals?.Name.Identifier.ValueText)
            {
                case Prefix when semantics.TryGetConstValue(arg.Expression, out string? value):
                    prefix = value;
                    break;
            }
        }
    }

    private static Signature? TryParseSignature(AttributeSyntax attribute, string prefix, SemanticModel semantics)
    {
        if (attribute.ArgumentList is { Arguments: { Count: >= 1 } args }
            && semantics.TryGetConstValue(args[0].Expression, out string? jsMember))
        {
            Signature signature = new()
            {
                JsMember = jsMember,
                Prefix = prefix,
                ReturnType = TypeView.Void,
            };

            for (int i = 1; i < args.Count; ++i)
            {
                if (!TryParseNamedArgument(args[i], ref signature, semantics))
                {
                    return null;
                }
            }

            signature.Params ??= new List<Param>(capacity: 0);

            return signature;
        }

        return null;
    }

    private static bool TryParseNamedArgument(AttributeArgumentSyntax arg,
                                              ref Signature signature,
                                              SemanticModel semantics)
    {
        switch (arg.NameEquals?.Name.Identifier.ValueText)
        {
            case Params when arg.Expression is TypeOfExpressionSyntax
            {
                Type: TupleTypeSyntax
                {
                    Elements: var elements,
                },
            }:
                signature.Params = new List<Param>(capacity: elements.Count);

                foreach (TupleElementSyntax element in elements)
                {
                    // Tuple (int i, int) generates (int i) param list.
                    if (!element.Identifier.IsKind(SyntaxKind.None))
                    {
                        if (TryGetTypeView(element.Type, semantics, out TypeView type))
                        {
                            signature.Params.Add(new Param
                            {
                                Name = element.Identifier.ValueText,
                                Type = type,
                            });
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return true;

            case Params:
                return false;

            case Returns when TryGetReturnType(arg, semantics, ref signature):
                return true;

            case Returns:
                return false;

            case ReturnsNullable when TryGetReturnType(arg, semantics, ref signature):
                signature.ReturnType.IsNullable = true;
                return true;

            case ReturnsNullable:
                return false;

            case ResetPrefix when semantics.TryGetConstValue(arg.Expression, out bool resetPrefix):
                if (resetPrefix)
                {
                    signature.Prefix = "";
                }
                return true;

            case ResetPrefix:
                return false;
        }

        return true;
    }

    private static bool TryGetReturnType(AttributeArgumentSyntax syntax,
                                         SemanticModel semantics,
                                         ref Signature signature)
    {
        return syntax.Expression is TypeOfExpressionSyntax
               {
                   Type: var type,
               }
               && TryGetTypeView(type, semantics, out signature.ReturnType);
    }

    private static bool TryGetTypeView(TypeSyntax syntax, SemanticModel semantics, out TypeView typeView)
    {
        bool isNullable;

        if (syntax is NullableTypeSyntax { ElementType: var nonNullableType })
        {
            isNullable = true;
        }
        else
        {
            isNullable = false;
            nonNullableType = syntax;
        }

        TypeInfo typeInfo = semantics.GetTypeInfo(nonNullableType);
        ITypeSymbol? type = typeInfo.Type;

        if (type != null)
        {
            bool isArray = nonNullableType is ArrayTypeSyntax;
            bool isPublic = true;
            ImmutableArray<SymbolDisplayPart> parts = type.ToDisplayParts(SymbolDisplayFormat.FullyQualifiedFormat);
            StringBuilder sb = new(capacity: parts.Sum(p => p.ToString().Length) + parts.Length);

            foreach (SymbolDisplayPart part in parts)
            {
                isPublic &= part.Symbol is not ITypeSymbol or ITypeSymbol
                {
                    DeclaredAccessibility: Accessibility.Public,
                };

                sb.Append(part.ToString());

                if (IsAnnotated(part))
                {
                    sb.Append('?');
                }
            }

            typeView = new TypeView
            {
                FullId = sb.ToString(),
                // Not implemented.
                ShortId = "",
                // Not implemented.
                ContainingParentsPath = "",
                IsPublic = isPublic,
                IsNullable = isNullable,
                IsArray = isArray,
            };

            return true;
        }

        typeView = default;
        return false;

        static bool IsAnnotated(in SymbolDisplayPart part) => part.Symbol is ITypeSymbol
        {
            NullableAnnotation: NullableAnnotation.Annotated,
        };
    }
}
