// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        internal static async Task M1Async(this IJSRuntime js, global::A.CustomClass c, CancellationToken token)
        {
            await js.InvokeVoidAsync("M1Async", token, c);
        }

        internal static async Task<global::A.CustomClass> M2Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<global::A.CustomClass>("M2Async", token);
        }

        internal static async Task<global::A.CustomClass?[]> M3Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<global::A.CustomClass?[]>("M3Async", token);
        }

        internal static async Task<global::A.CustomClass?> M4Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<global::A.CustomClass?>("M4Async", token);
        }

        internal static async Task<global::System.Collections.Generic.List<global::A.CustomClass?>> M5Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<global::System.Collections.Generic.List<global::A.CustomClass?>>("M5Async", token);
        }

        internal static async Task<(global::A.CustomClass?, int)> M6Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<(global::A.CustomClass?, int)>("M6Async", token);
        }
    }
}

namespace A
{
    internal static partial class C
    {
        public static async Task<int> M1Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<int>("M1Async", token);
        }
    }
}
