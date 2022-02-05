// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task M1Async(this IJSRuntime js, global::System.Collections.Generic.List<int> ls, CancellationToken token)
        {
            await js.InvokeVoidAsync("M1Async", token, ls);
        }

        public static async Task<int> M2Async(this IJSRuntime js, global::System.Collections.Generic.List<int?>? ls, CancellationToken token)
        {
            return await js.InvokeAsync<int>("M2Async", token, ls);
        }

        public static async Task M3Async(this IJSRuntime js, global::System.Collections.Generic.List<object?> ls, (object? o, object o1, int? i, int i1, global::System.Collections.Generic.List<object?> ls, global::System.Collections.IList ls1) ts, CancellationToken token)
        {
            await js.InvokeVoidAsync("M3Async", token, ls, ts);
        }

        public static async Task M4Async(this IJSRuntime js, global::System.Collections.Generic.List<global::System.Collections.Generic.KeyValuePair<global::A.CustomStruct?, string?>?> kvs, CancellationToken token)
        {
            await js.InvokeVoidAsync("M4Async", token, kvs);
        }
    }
}
