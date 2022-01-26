using System.Text;
using System.Threading.Tasks;
using JsBindingsGenerator;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Blazor.JsBindingsGenerator.Tests;

public class BlazorJsBindingsSourceGeneratorTests
{
    [Fact]
    public async Task Attributes_Generated()
    {
        const string generated = @"// Auto-generated
#nullable enable

using System;

namespace JsBindingsGenerator;

[AttributeUsage(AttributeTargets.Class)]
internal class JsBindingContextAttribute : Attribute
{
    public JsBindingContextAttribute(string jsContext) {}
}

[AttributeUsage(AttributeTargets.Class)]
internal class JsBindAttribute : Attribute
{
    public Type? Params { get; init; }

    public Type Returns { get; init; } = typeof(void);

    public bool ResetContext { get; init; }

    public JsBindAttribute(string member) {}
}
";

        await new CSharpSourceGeneratorVerifier<BlazorJsBindingsSourceGenerator>
        {
            //TestCode = code,
            TestState =
            {
                //Sources = { code },
                GeneratedSources =
                {
                    (
                        typeof(BlazorJsBindingsSourceGenerator),
                        "Attributes.g.cs",
                        SourceText.From(generated, Encoding.UTF8)
                    ),
                },
            },
        }.RunAsync();
    }
}
