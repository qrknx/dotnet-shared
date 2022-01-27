﻿using System.Runtime.CompilerServices;
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
            if (TryGetClassForGeneration(context) is {} classForGeneration)
            {
                ClassesForGeneration.Add(classForGeneration);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ClassForGeneration? TryGetClassForGeneration(GeneratorSyntaxContext context)
        {
            if (context.Node is not ClassDeclarationSyntax
                {
                    Parent: BaseNamespaceDeclarationSyntax parentSyntax,
                    TypeParameterList: null,
                    Identifier.Value: string identifier,
                    AttributeLists: var attributeLists,
                    Modifiers: var modifiers,
                }
                || !modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                || !modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                return null;
            }

            ClassForGeneration classForGeneration = InitializeClassForGeneration(identifier,
                                                                                 modifiers,
                                                                                 attributeLists,
                                                                                 parentSyntax);

            ExtractGenerationInfo(attributeLists, ref classForGeneration, context.SemanticModel);

            return classForGeneration.Signatures.Any()
                ? classForGeneration
                : null;
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
            SymbolInfo symbolInfo = semantics.GetSymbolInfo(attribute);

            if (symbolInfo.Symbol is
                {
                    ContainingNamespace.Name: Namespace,
                    ContainingType.Name: var name,
                })
            {
                switch (name)
                {
                    case JsBindingContextAttribute
                        when attribute.ArgumentList is { Arguments: { Count: 1 } args }
                             && semantics.GetConstantValue(args[0].Expression) is
                             {
                                 HasValue: true,
                                 Value: string value,
                             }:

                        classForGeneration.JsContext = value;
                        break;

                    case JsBindingContextAttribute:
                        return;

                    case JsBindAttribute
                        when attribute.ArgumentList is { Arguments: { Count: >= 1 } args }
                             && semantics.GetConstantValue(args[0].Expression) is
                             {
                                 HasValue: true,
                                 Value: string value,
                             }:

                        Signature signature = new()
                        {
                            JsMember = value,
                            Params = new List<Param>(capacity: 4),
                            ReturnType = TypeView.Void,
                        };

                        for (int i = 1; i < args.Count; ++i)
                        {
                            AttributeArgumentSyntax arg = args[i];

                            switch (arg.NameEquals?.Name.Identifier.ValueText)
                            {
                                case Params
                                    when arg.Expression is TypeOfExpressionSyntax
                                    {
                                        Type: TupleTypeSyntax
                                        {
                                            Elements: var elements,
                                        },
                                    }:

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
                                                return;
                                            }
                                        }
                                    }
                                    break;

                                case Params:
                                    return;

                                case Returns when arg.Expression is TypeOfExpressionSyntax
                                                  {
                                                      Type: var type,
                                                  }
                                                  && TryGetTypeView(type, semantics, out signature.ReturnType):
                                    break;

                                case Returns:
                                    return;

                                case ReturnsNullable when arg.Expression is TypeOfExpressionSyntax
                                                          {
                                                              Type: var type,
                                                          }
                                                          && TryGetTypeView(type, semantics, out signature.ReturnType):
                                    signature.ReturnType.IsNullable = true;
                                    break;

                                case ReturnsNullable:
                                    return;

                                case ResetContext
                                    when semantics.GetConstantValue(arg.Expression) is
                                    {
                                        HasValue: true,
                                        Value: bool resetContext,
                                    }:
                                    signature.ResetJsContext = resetContext;
                                    break;

                                case ResetContext:
                                    return;
                            }
                        }

                        classForGeneration.Signatures.Add(signature);
                        break;

                    case JsBindAttribute:
                        return;
                }
            }
        }

        private static bool TryGetTypeView(TypeSyntax syntax, SemanticModel semantics, out TypeView typeView)
        {
            bool isNullable;

            TypeSyntax internalType;

            if (syntax is NullableTypeSyntax nts)
            {
                isNullable = true;
                internalType = nts.ElementType;
            }
            else
            {
                isNullable = false;
                internalType = syntax;
            }

            TypeInfo typeInfo = semantics.GetTypeInfo(internalType);
            ITypeSymbol? type = typeInfo.Type;

            if (type != null)
            {
                bool isArray = internalType is ArrayTypeSyntax;

                typeView = new TypeView
                {
                    FullId = string.Join("", type.ToDisplayParts(SymbolDisplayFormat.FullyQualifiedFormat)
                                             .Select(p => IsAnnotated(p)
                                                         ? $"{p}?"
                                                         : p.ToString())),
                    // Not implemented.
                    ShortId = "",
                    // Not implemented.
                    ContainingParentsPath = "",
                    IsPublic = type.DeclaredAccessibility == Accessibility.Public || isArray,
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
