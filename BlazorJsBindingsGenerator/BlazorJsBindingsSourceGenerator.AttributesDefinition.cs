namespace BlazorJsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
    private const string Namespace = nameof(BlazorJsBindingsGenerator);
    private const string JsBindingContextAttribute = "JsBindingContextAttribute";
    private const string Prefix = "Prefix";
    private const string JsBindAttribute = "JsBindAttribute";
    private const string Params = "Params";
    private const string Returns = "Returns";
    private const string ReturnsNullable = "ReturnsNullable";
    private const string ResetPrefix = "ResetPrefix";

    public const string AttributesToUse = $@"// Auto-generated
#nullable enable

using System;

namespace {Namespace};

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class {JsBindingContextAttribute} : Attribute
{{
    public string {Prefix} {{ get; init; }} = null!;
}}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class {JsBindAttribute} : Attribute
{{
    public Type? {Params} {{ get; init; }}

    public Type {Returns} {{ get; init; }} = typeof(void);

    public Type {ReturnsNullable} {{ get; init; }} = typeof(void);

    public bool {ResetPrefix} {{ get; init; }}

    public {JsBindAttribute}(string member) {{}}
}}
";
}
