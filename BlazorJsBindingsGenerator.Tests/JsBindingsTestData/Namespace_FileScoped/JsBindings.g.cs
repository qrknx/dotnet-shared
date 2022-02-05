// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task M1Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("M1Async", token);
        }
    }
}
