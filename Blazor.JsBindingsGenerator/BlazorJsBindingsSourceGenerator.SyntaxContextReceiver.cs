using System.Runtime.CompilerServices;
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
            if (IsClassForGeneration(context,
                                     out SyntaxList<AttributeListSyntax> attributeLists,
                                     out TypeView type,
                                     out bool isPublic))
            {
                SemanticModel semantics = context.SemanticModel;

                ClassForGeneration classForGeneration = new()
                {
                    Type = type,
                    IsPublic = isPublic,
                    Signatures = new(capacity: attributeLists.Sum(list => list.Attributes.Count)),
                };

                foreach (AttributeListSyntax attributeList in attributeLists)
                {
                    foreach (AttributeSyntax attribute in attributeList.Attributes)
                    {
                        ExtractGenerationInfo(attribute, ref classForGeneration, semantics);
                    }
                }

                if (classForGeneration.Signatures.Any())
                {
                    ClassesForGeneration.Add(classForGeneration);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsClassForGeneration(GeneratorSyntaxContext context,
                                                 out SyntaxList<AttributeListSyntax> attributeLists,
                                                 out TypeView type,
                                                 out bool isPublic)
        {
            if (context.Node is ClassDeclarationSyntax
                {
                    Parent: BaseNamespaceDeclarationSyntax parent,
                    Modifiers: var modifiers,
                } cds
                && modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                && modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                if (cds.Identifier.Value is string identifier)
                {
                    attributeLists = cds.AttributeLists;

                    string containingNodePath;

                    if (parent.Parent is not BaseNamespaceDeclarationSyntax)
                    {
                        containingNodePath = parent.Name.ToString();
                    }
                    else
                    {
                        Stack<string> nodes = new();
                        BaseNamespaceDeclarationSyntax? current = parent;

                        do
                        {
                            nodes.Push(current.Name.ToString());
                            current = current.Parent as BaseNamespaceDeclarationSyntax;
                        } while (current != null);

                        containingNodePath = string.Join('.', nodes);
                    }

                    type = new TypeView
                    {
                        Id = identifier,
                        ContainingParentsPath = containingNodePath,
                    };

                    isPublic = modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
                    
                    return true;
                }
            }

            (attributeLists, type, isPublic) = (default, default, default);

            return false;
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
                                        if (!element.Identifier.IsKind(SyntaxKind.None))
                                        {
                                            TypeView type = GetTypeName(element.Type, semantics);

                                            if (!type.IsValid)
                                            {
                                                return;
                                            }

                                            signature.Params.Add(new Param
                                            {
                                                Name = element.Identifier.ValueText,
                                                Type = type,
                                            });
                                        }
                                    }
                                    break;

                                case Params:
                                    return;

                                case Returns
                                    when arg.Expression is TypeOfExpressionSyntax
                                         {
                                             Type: var type,
                                         }
                                         && GetTypeName(type, semantics) is { IsValid: true } returnType:

                                    signature.ReturnType = returnType;
                                    break;

                                case Returns:
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

        private static TypeView GetTypeName(TypeSyntax syntax, SemanticModel semantics)
        {
            TypeInfo typeInfo = semantics.GetTypeInfo(syntax);
            ITypeSymbol? type = typeInfo.Type;

            // todo
            bool isNullable = syntax is NullableTypeSyntax;

            string id;
            string containingParentsPath;
            bool isPublic;
            
            if (type != null)
            {
                id = $"{type.Name}";
                containingParentsPath = type.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                isPublic = type.DeclaredAccessibility == Accessibility.Public;
            }
            else
            {
                id = "";
                containingParentsPath = "";
                isPublic = false;
            }

            return new()
            {
                Id = id,
                ContainingParentsPath = containingParentsPath,
                IsNullable = isNullable,
                IsPublic = isPublic,
            };
        }
    }

#pragma warning disable CS8618, CS0649

    private struct ClassForGeneration
    {
        public TypeView Type;
        public bool IsPublic;
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
            ContainingParentsPath = "",
            Id = "void",
            IsPublic = true,
        };

        public string ContainingParentsPath;
        public string Id;
        public bool IsNullable;
        public bool IsPublic;

        public bool IsValid => Id != "";

        public string FullName
        {
            get
            {
                string nullability = IsNullable ? "?" : "";

                return ContainingParentsPath != ""
                    ? $"{ContainingParentsPath}.{Id}{nullability}"
                    : $"{Id}{nullability}";
            }
        }

        public bool Equals(TypeView other) => ContainingParentsPath == other.ContainingParentsPath
                                              && Id == other.Id
                                              && IsNullable == other.IsNullable
                                              && IsPublic == other.IsPublic;

        public override bool Equals(object? obj) => obj is TypeView other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(ContainingParentsPath, Id, IsNullable, IsPublic);

        public override string ToString() => FullName;
    }

#pragma warning restore
}
