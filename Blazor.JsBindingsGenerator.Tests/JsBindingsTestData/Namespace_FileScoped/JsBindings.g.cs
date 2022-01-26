// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task<System.Int32> ShowAsync(this IJSRuntime js, System.String s, System.Object obj, CancellationToken token)
        {
            return await js.InvokeAsync<System.Int32>("show", token, s, obj);
        }
    }
}
