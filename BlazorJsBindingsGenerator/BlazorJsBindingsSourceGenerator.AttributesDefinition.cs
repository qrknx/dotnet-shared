namespace BlazorJsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
    private const string Namespace = nameof(BlazorJsBindingsGenerator);
    private const string AttributesVisibility = $"{Namespace}_ATTRIBUTES_ACCESS";
    private const string JsBindingContextAttribute = "JsBindingContextAttribute";
    private const string JsPrefix = "JsPrefix";
    private const string JsBindAttribute = "JsBindAttribute";
    private const string Params = "Params";
    private const string Returns = "Returns";
    private const string ReturnsNullable = "ReturnsNullable";
    private const string ResetJsPrefix = "ResetJsPrefix";

    public const string AttributesToUse = $@"// Auto-generated
#nullable enable

using System;
using System.Diagnostics;

namespace {Namespace};

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[Conditional(""{AttributesVisibility}"")]
internal class {JsBindingContextAttribute} : Attribute
{{
    public string {JsPrefix} {{ get; init; }} = null!;
}}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[Conditional(""{AttributesVisibility}"")]
internal class {JsBindAttribute} : Attribute
{{
    public Type? {Params} {{ get; init; }}

    public Type {Returns} {{ get; init; }} = typeof(void);

    public Type {ReturnsNullable} {{ get; init; }} = typeof(void);

    public bool {ResetJsPrefix} {{ get; init; }}

    public {JsBindAttribute}(string member) {{}}
}}
";
}
