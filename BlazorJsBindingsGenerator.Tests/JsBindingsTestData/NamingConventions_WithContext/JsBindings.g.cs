// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task M1Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("BlazorCallbacks.M1Async", token);
        }

        public static async Task M2Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("M2Async", token);
        }

        public static async Task M3Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("BlazorCallbacks.M3Async", token);
        }

        public static async Task SomePrefixM4Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("SomePrefix.M4Async", token);
        }

        public static async Task SomePrefixM5Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("BlazorCallbacks.SomePrefix.M5Async", token);
        }

        public static async Task M6Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("M6Async", token);
        }
    }
}
