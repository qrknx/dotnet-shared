// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A0.A1.A2
{
    public static partial class B
    {
        public static async Task<System.Int32> M1Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<System.Int32>("M1Async", token);
        }
    }
}
