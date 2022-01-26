// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task Show1Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("show1", token);
        }

        public static async Task Show2Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("show2Async", token);
        }

        public static async Task Show3Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("Show3", token);
        }

        public static async Task Show4Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("Show4Async", token);
        }
    }
}
