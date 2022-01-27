// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task<int?> M1Async(this IJSRuntime js, int? i, string? s, global::System.Threading.Tasks.Task? t, global::A.CustomStruct? cs, CancellationToken token)
        {
            return await js.InvokeAsync<int?>("M1Async", token, i, s, t, cs);
        }

        public static async Task<object?> M2Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<object?>("M2Async", token);
        }
    }
}
