﻿// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace A
{
    public static partial class B
    {
        public static async Task<int?> M1Async(this IJSRuntime js, int? i, string? s, global::System.Threading.Tasks.Task? t, global::A.CustomStruct? cs, CancellationToken token)
        {
            return await js.InvokeAsync<int?>("M1Async", token, i, s, t, cs);
        }

        public static async Task<object?> M2Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<object?>("M2Async", token);
        }

        public static async Task<object?[]> M3Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<object?[]>("M3Async", token);
        }

        public static async Task<global::System.Collections.IList?[]> M4Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<global::System.Collections.IList?[]>("M4Async", token);
        }

        public static async Task<global::A.CustomStruct?[]> M5Async(this IJSRuntime js, CancellationToken token)
        {
            return await js.InvokeAsync<global::A.CustomStruct?[]>("M5Async", token);
        }

        public static async Task M6Async(this IJSRuntime js, object[]? os, CancellationToken token)
        {
            await js.InvokeVoidAsync("M6Async", token, (object?)os);
        }
    }
}
