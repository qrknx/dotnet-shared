using System.Collections;
using BlazorJsBindingsGenerator;

namespace A;

public struct CustomStruct {}

[JsBind("M1Async",
        Params = typeof((int? i, string? s, System.Threading.Tasks.Task? t, CustomStruct? cs)),
        Returns = typeof(int?))]
[JsBind("M2Async", ReturnsNullable = typeof(object))]
[JsBind("M3Async", Returns = typeof(object?[]))]
[JsBind("M4Async", Returns = typeof(IList?[]))]
[JsBind("M5Async", Returns = typeof(CustomStruct?[]))]
public static partial class B {}
