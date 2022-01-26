using System;
using System.Text;
using System.Threading.Tasks;
using JsBindingsGenerator;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Blazor.JsBindingsGenerator.Tests;

public class BlazorJsBindingsSourceGeneratorTests
{
    private const string GeneratedAttributes = @"// Auto-generated
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

    private static readonly (Type, string, SourceText) AttributesGeneratedSource
        = GeneratedSource("Attributes.g.cs", GeneratedAttributes);

    [Fact]
    public async Task AttributesForConsumer_Generated()
    {
        CSharpSourceGeneratorVerifier<BlazorJsBindingsSourceGenerator> verifier = new()
        {
            TestState =
            {
                GeneratedSources =
                {
                    AttributesGeneratedSource,
                },
            },
        };

        await verifier.RunAsync();
    }

    [Fact]
    public async Task Class_Generated()
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
            return await js.InvokeAsync<System.Int32>(""BlazorCallbacks.show"", token, s, obj);
        }
    }
}
";

        CSharpSourceGeneratorVerifier<BlazorJsBindingsSourceGenerator> verifier = new()
        {
            TestState =
            {
                Sources = { source },
                GeneratedSources =
                {
                    AttributesGeneratedSource,
                    GeneratedSource("JsBindings.g.cs", generated),
                },
            },
            Packages = new[]
            {
                new PackageIdentity("Microsoft.JSInterop", "5.0.13"),
            },
        };

        await verifier.RunAsync();
    }

    private static (Type, string, SourceText) GeneratedSource(string fileName, string generated) =>
    (
        typeof(BlazorJsBindingsSourceGenerator),
        fileName,
        SourceText.From(generated, Encoding.UTF8)
    );
}
