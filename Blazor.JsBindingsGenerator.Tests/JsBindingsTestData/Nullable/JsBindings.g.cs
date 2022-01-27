// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task<global::System.Int32?> M1Async(this IJSRuntime js, global::System.Int32? i, global::System.String? s, global::System.Threading.Tasks.Task? t, global::A.CustomStruct? cs, CancellationToken token)
        {
            return await js.InvokeAsync<global::System.Int32?>("M1Async", token, i, s, t, cs);
        }
    }
}
