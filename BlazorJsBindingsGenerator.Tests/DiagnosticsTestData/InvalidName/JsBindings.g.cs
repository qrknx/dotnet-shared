// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace N
{
    public static partial class C
    {
        public static async Task ValidAsync(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("ValidAsync", token);
        }
    }
}
