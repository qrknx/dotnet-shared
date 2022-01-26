using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
    private const string Namespace = nameof(JsBindingsGenerator);
    private const string JsBindingContextAttribute = "JsBindingContextAttribute";
    private const string JsBindAttribute = "JsBindAttribute";
    private const string Params = "Params";
    private const string Returns = "Returns";
    private const string ResetContext = "ResetContext";

    private const string AttributesToUse = $@"// Auto-generated
#nullable enable

using System;

namespace {Namespace};

[AttributeUsage(AttributeTargets.Class)]
internal class {JsBindingContextAttribute} : Attribute
{{
    public {JsBindingContextAttribute}(string jsContext) {{}}
}}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class {JsBindAttribute} : Attribute
{{
    public Type? {Params} {{ get; init; }}

    public Type {Returns} {{ get; init; }} = typeof(void);

    public bool {ResetContext} {{ get; init; }}

    public {JsBindAttribute}(string member) {{}}
}}
";

    private class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        public readonly List<ClassForGeneration> ClassesForGeneration = new(capacity: 1);

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (IsClassForGeneration(context,
                                     out SyntaxList<AttributeListSyntax> attributeLists,
                                     out TypeName className,
                                     out bool isPublic))
            {
                SemanticModel semantics = context.SemanticModel;

                ClassForGeneration classForGeneration = new()
                {
                    Name = className,
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
                                                 out TypeName className,
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

                    className = new TypeName
                    {
                        Id = identifier,
                        ContainingNodePath = containingNodePath,
                    };

                    isPublic = modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
                    
                    return true;
                }
            }

            (attributeLists, className, isPublic) = (default, default, default);

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
                            ReturnTypeName = TypeName.Void,
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
                                            TypeName typeName = GetTypeName(element.Type, semantics);

                                            if (!typeName.IsValid)
                                            {
                                                return;
                                            }

                                            signature.Params.Add(new Param
                                            {
                                                Name = element.Identifier.ValueText,
                                                TypeName = typeName,
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
                                         && GetTypeName(type, semantics) is { IsValid: true } returnTypeName:

                                    signature.ReturnTypeName = returnTypeName;
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

        private static TypeName GetTypeName(TypeSyntax syntax, SemanticModel semantics)
        {
            TypeInfo typeInfo = semantics.GetTypeInfo(syntax);
            ITypeSymbol? type = typeInfo.Type;

            bool isNullable = syntax is NullableTypeSyntax;

            string id;

            // todo
            if (type == null)
            {
                id = "";
            }
            else
            {
                id = $"{type.Name}";
            }

            return new()
            {
                Id = id,
                ContainingNodePath = type?.ContainingNamespace.Name ?? "",
                IsNullable = isNullable,
            };
        }
    }

#pragma warning disable CS8618, CS0649

    private struct ClassForGeneration
    {
        public TypeName Name;
        public bool IsPublic;
        public List<Signature> Signatures;
        public string JsContext = "";
    }

    private struct Signature
    {
        public List<Param> Params;
        public string JsMember;
        public TypeName ReturnTypeName;
        public bool ResetJsContext;
    }

    private struct Param
    {
        public TypeName TypeName;
        public string Name;
    }

    private struct TypeName : IEquatable<TypeName>
    {
        public static readonly TypeName Void = new()
        {
            ContainingNodePath = "",
            Id = "void",
        };

        public string ContainingNodePath;
        public string Id;
        public bool IsNullable;

        public bool IsValid => Id != "";

        public string FullName
        {
            get
            {
                string nullability = IsNullable ? "?" : "";

                return ContainingNodePath != ""
                    ? $"{ContainingNodePath}.{Id}{nullability}"
                    : $"{Id}{nullability}";
            }
        }

        public bool Equals(TypeName other) => ContainingNodePath == other.ContainingNodePath && Id == other.Id;

        public override bool Equals(object? obj) => obj is TypeName other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(ContainingNodePath, Id);
    }

#pragma warning restore
}
