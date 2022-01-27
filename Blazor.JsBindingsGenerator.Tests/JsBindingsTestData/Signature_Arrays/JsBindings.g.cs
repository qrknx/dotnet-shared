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

        public static async Task<object?[]?> M2Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<object?[]?>("M2Async", token);
        }

        public static async Task<global::System.Collections.IList?[]?> M3Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<global::System.Collections.IList?[]?>("M3Async", token);
        }

        public static async Task<global::A.CustomStruct?[]?> M4Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<global::A.CustomStruct?[]?>("M4Async", token);
        }

        public static async Task M5Async(this IJSRuntime js, object[] os, CancellationToken token)
        {
            await js.InvokeVoidAsync("M5Async", token, (object)os);
        }

        public static async Task M6Async(this IJSRuntime js, byte[] bs, object[] os, CancellationToken token)
        {
            await js.InvokeVoidAsync("M6Async", token, bs, os);
        }
    }
}
