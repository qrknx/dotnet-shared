// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        internal static async Task M1Async(this IJSRuntime js, global::A.CustomStruct s, CancellationToken token)
        {
            await js.InvokeVoidAsync("M1Async", token, s);
        }

        internal static async Task<global::A.CustomStruct> M2Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<global::A.CustomStruct>("M2Async", token);
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
