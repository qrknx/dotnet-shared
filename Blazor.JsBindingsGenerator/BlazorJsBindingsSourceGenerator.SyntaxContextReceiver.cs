using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
    private class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        public readonly List<ClassForGeneration> ClassesForGeneration = new(capacity: 1);

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (TryGetClassForGeneration(context) is {Signatures.Count: > 0} classForGeneration)
            {
                ClassesForGeneration.Add(classForGeneration);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ClassForGeneration? TryGetClassForGeneration(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax
                {
                    Parent: BaseNamespaceDeclarationSyntax parentSyntax,
                    TypeParameterList: null,
                    Identifier.Value: string identifier,
                    AttributeLists: var attributeLists,
                    Modifiers: var modifiers,
                } && modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                  && modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                ClassForGeneration classForGeneration = InitializeClassForGeneration(identifier,
                                                                                     modifiers,
                                                                                     attributeLists,
                                                                                     parentSyntax);

                ExtractGenerationInfo(attributeLists, ref classForGeneration, context.SemanticModel);

                return classForGeneration;
            }

            return null;
        }

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
                    FullId = string.Join('.', fullIdParts),
                    ShortId = identifier,
                    ContainingParentsPath = string.Join('.', fullIdParts.SkipLast(1)),
                    IsPublic = modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)),
                },
                Signatures = new List<Signature>(capacity: attributeLists.Sum(list => list.Attributes.Count)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExtractGenerationInfo(SyntaxList<AttributeListSyntax> attributeLists,
                                                  ref ClassForGeneration classForGeneration,
                                                  SemanticModel semantics)
        {
            foreach (AttributeListSyntax attributeList in attributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    ExtractGenerationInfo(attribute, ref classForGeneration, semantics);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExtractGenerationInfo(AttributeSyntax attribute,
                                                  ref ClassForGeneration classForGeneration,
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
                    case JsBindingContextAttribute
                        when attribute.ArgumentList is { Arguments: { Count: 1 } args }
                             && semantics.TryGetConstValue(args[0].Expression, out string? value):

                        classForGeneration.JsContext = value;
                        break;

                    case JsBindingContextAttribute:
                        return;

                    case JsBindAttribute when TryParseSignature(attribute, semantics) is {} signature:
                        classForGeneration.Signatures.Add(signature);
                        break;

                    case JsBindAttribute:
                        return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Signature? TryParseSignature(AttributeSyntax attribute, SemanticModel semantics)
        {
            if (attribute.ArgumentList is { Arguments: { Count: >= 1 } args }
                && semantics.TryGetConstValue(args[0].Expression, out string? jsMember))
            {
                Signature signature = new()
                {
                    JsMember = jsMember,
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

                case ResetContext when semantics.TryGetConstValue(arg.Expression, out bool resetContext):
                    signature.ResetJsContext = resetContext;
                    return true;

                case ResetContext:
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

            if (syntax is NullableTypeSyntax {ElementType: var nonNullableType })
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

#pragma warning disable CS8618, CS0649

    private struct ClassForGeneration
    {
        public TypeView Type;
        public List<Signature> Signatures;
        public string JsContext = "";
    }

    private struct Signature
    {
        public List<Param> Params;
        public string JsMember;
        public TypeView ReturnType;
        public bool ResetJsContext;

        public bool IsPublic => ReturnType.IsPublic && Params.TrueForAll(p => p.Type.IsPublic);
    }

    private struct Param
    {
        public TypeView Type;
        public string Name;
    }

    private struct TypeView : IEquatable<TypeView>
    {
        public static readonly TypeView Void = new()
        {
            FullId = "void",
            IsPublic = true,
        };

        public string FullId;
        public string ShortId;
        public string ContainingParentsPath;
        public bool IsNullable;
        public bool IsPublic;
        public bool IsArray;

        public string AnnotatedFullId => $"{FullId}{(IsNullable ? "?" : "")}";

        public bool Equals(TypeView other) => FullId == other.FullId
                                              && IsNullable == other.IsNullable
                                              && IsPublic == other.IsPublic;

        public override bool Equals(object? obj) => obj is TypeView other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(FullId, IsNullable, IsPublic);

        public override string ToString() => AnnotatedFullId;
    }

#pragma warning restore
}
