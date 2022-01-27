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

        public static async Task M2Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("M2Async", token);
        }
    }
}

namespace A
{
    public static partial class C
    {
        public static async Task M1Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("M1Async", token);
        }
    }
}

namespace A
{
    public static partial class D
    {
        public static async Task M1Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("M1Async", token);
        }
    }
}

namespace A1
{
    public static partial class D
    {
        public static async Task M1Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("M1Async", token);
        }

        public static async Task M2Async(this IJSRuntime js, CancellationToken token)
        {
            await js.InvokeVoidAsync("M2Async", token);
        }
    }
}
