using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsBindingsGenerator;

internal partial class BlazorJsBindingsSourceGenerator
{
    private const string Namespace = nameof(JsBindingsGenerator);
    private const string JsBindingContextAttribute = nameof(JsBindingContextAttribute);
    private const string JsBindAttribute = nameof(JsBindAttribute);
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

[AttributeUsage(AttributeTargets.Class)]
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
                SemanticModel semanticModel = context.SemanticModel;

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
                        ExtractGenerationInfo(attribute, ref classForGeneration, semanticModel);
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
            if (context.Node is ClassDeclarationSyntax { Modifiers: var modifiers } cds
                && modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                && modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            {
                SemanticModel semanticModel = context.SemanticModel;

                ISymbol? symbol = semanticModel.GetSymbolInfo(cds).Symbol;

                if (symbol is {ContainingType: null}
                    && cds.Identifier.Value is string identifier)
                {
                    attributeLists = cds.AttributeLists;

                    className = new TypeName
                    {
                        Id = identifier,
                        ContainingNodePath = symbol.MetadataName,
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
                                                  SemanticModel semanticModel)
        {
            if (semanticModel.GetSymbolInfo(attribute).Symbol is
                {
                    IsImplicitlyDeclared: true,
                    ContainingNamespace.Name: Namespace,
                    Name: var name,
                })
            {
                switch (name)
                {
                    case JsBindingContextAttribute
                        when attribute.ArgumentList is { Arguments: { Count: 1 } args }
                             && semanticModel.GetConstantValue(args[0].Expression) is
                             {
                                 HasValue: true,
                                 Value: string value,
                             }:

                        classForGeneration.JsContext = value;
                        break;

                    case JsBindAttribute
                        when attribute.ArgumentList is { Arguments: { Count: >= 1 } args }
                             && semanticModel.GetConstantValue(args[0].Expression) is
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
                            switch (args[i].NameColon?.Name.Identifier.ValueText)
                            {
                                case Params:
                                    break;

                                case Returns:
                                    break;

                                case ResetContext:
                                    break;
                            }
                        }
                        break;
                }
            }
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

        public string FullName => ContainingNodePath != ""
            ? $"{ContainingNodePath}.{Id}"
            : Id;

        public bool Equals(TypeName other) => ContainingNodePath == other.ContainingNodePath && Id == other.Id;

        public override bool Equals(object? obj) => obj is TypeName other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(ContainingNodePath, Id);
    }

#pragma warning restore
}
