// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task<byte[]> M1Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<byte[]>("M1Async", token);
        }

        public static async Task M2Async(this IJSRuntime js, object[] os, CancellationToken token)
        {
            await js.InvokeVoidAsync("M2Async", token, (object)os);
        }

        public static async Task M3Async(this IJSRuntime js, byte[] bs, object[] os, CancellationToken token)
        {
            await js.InvokeVoidAsync("M3Async", token, bs, os);
        }
    }
}
