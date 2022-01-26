using System.Threading.Tasks;
using Xunit;

namespace Blazor.JsBindingsGenerator.Tests;

public class BlazorJsBindingsSourceGeneratorTests
{
    [Fact]
    public async Task AttributesForConsumer_Generated()
    {
        BlazorJsBindingsSourceGeneratorWrapper wrapper = new();

        await wrapper.RunAsync();
    }

    [Fact]
    public async Task Class_Generated()
    {
        BlazorJsBindingsSourceGeneratorWrapper wrapper = new()
        {
            Source = @"using JsBindingsGenerator;

namespace A;

[JsBindingContext(""BlazorCallbacks"")]
[JsBind(""show"", Params = typeof((string s, object obj)), Returns = typeof(int), ResetContext = false)]
public static partial class B {}
",
            GeneratedJsBindings = @"// Auto-generated
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
",
        };

        await wrapper.RunAsync();
    }
}
