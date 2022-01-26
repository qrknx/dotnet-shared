// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task VisibleNamespaceAsync(this IJSRuntime js, System.Collections.IList list, CancellationToken token)
        {
            await js.InvokeVoidAsync("VisibleNamespaceAsync", token, list);
        }

        public static async Task FullyQualifiedAsync(this IJSRuntime js, CustomNamespace.Nested.CustomStruct s, CancellationToken token)
        {
            await js.InvokeVoidAsync("FullyQualifiedAsync", token, s);
        }
    }
}
