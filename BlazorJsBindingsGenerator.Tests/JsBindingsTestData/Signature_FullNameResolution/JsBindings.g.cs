// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task VisibleNamespaceAsync(this IJSRuntime js, global::System.Collections.IList list, int i, CancellationToken token)
        {
            await js.InvokeVoidAsync("VisibleNamespaceAsync", token, list, i);
        }

        public static async Task WithAliasAsync(this IJSRuntime js, short s, global::System.IDisposable d, CancellationToken token)
        {
            await js.InvokeVoidAsync("WithAliasAsync", token, s, d);
        }

        public static async Task FullyQualifiedAsync(this IJSRuntime js, global::CustomNamespace.Nested.CustomStruct s, CancellationToken token)
        {
            await js.InvokeVoidAsync("FullyQualifiedAsync", token, s);
        }
    }
}
