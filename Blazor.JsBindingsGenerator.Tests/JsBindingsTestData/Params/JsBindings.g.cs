﻿// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task SingleParamAsync(this IJSRuntime js, System.Int32 i, CancellationToken token)
        {
            await js.InvokeVoidAsync("SingleParamAsync", token, i);
        }

        public static async Task M1Async(this IJSRuntime js, System.String s, System.Int32 i, System.Object obj, CancellationToken token)
        {
            await js.InvokeVoidAsync("M1Async", token, s, i, obj);
        }
    }
}
