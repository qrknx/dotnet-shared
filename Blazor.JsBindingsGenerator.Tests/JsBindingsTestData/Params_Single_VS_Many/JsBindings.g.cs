// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task SingleParamAsync(this IJSRuntime js, global::System.Int32 i, CancellationToken token)
        {
            await js.InvokeVoidAsync("SingleParamAsync", token, i);
        }

        public static async Task ManyParamsAsync(this IJSRuntime js, global::System.String s, global::System.Int32 i, global::System.Object obj, CancellationToken token)
        {
            await js.InvokeVoidAsync("ManyParamsAsync", token, s, i, obj);
        }
    }
}
