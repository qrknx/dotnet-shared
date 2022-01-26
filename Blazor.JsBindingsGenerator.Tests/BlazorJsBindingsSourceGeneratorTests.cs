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

        CSharpSourceGeneratorVerifier<BlazorJsBindingsSourceGenerator> verifier = new()
        {
            TestState =
            {
                GeneratedSources =
                {
                    (
                        typeof(BlazorJsBindingsSourceGenerator),
                        "Attributes.g.cs",
                        SourceText.From(generated, Encoding.UTF8)
                    ),
                },
            },
        };

        await verifier.RunAsync();
    }

    [Fact]
    public async Task Classes_Generated()
    {
        const string source = @"using JsBindingsGenerator;

namespace A;

[JsBindingContext(""BlazorCallbacks"")]
[JsBind(""show"", Params = typeof((string s, object obj)), Returns = typeof(int), ResetContext = false)]
public static partial class B {}
";

        const string generated = @"// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task<System.Int32> ShowAsync(this IJSRuntime js, System.String s, System.Object obj, CancellationToken token)
        {
            return await js.InvokeAsync<System.Int32>(""show"", token, s, obj);
        }
    }
}
";

        await new CSharpSourceGeneratorVerifier<BlazorJsBindingsSourceGenerator>
        {
            //TestCode = code,
            TestState =
            {
                Sources = { source },
                GeneratedSources =
                {
                    (
                        typeof(BlazorJsBindingsSourceGenerator),
                        "JsBindings.g.cs",
                        SourceText.From(generated, Encoding.UTF8)
                    ),
                },
            },
        }.RunAsync();
    }
}
