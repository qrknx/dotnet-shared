using JsBindingsGenerator;

namespace A;

public struct CustomStruct {}

[JsBind("M1Async",
        Params = typeof((int? i, string? s, System.Threading.Tasks.Task? t, CustomStruct? cs)),
        Returns = typeof(int?))]
[JsBind("M2Async", ReturnsNullable = typeof(object))]
public static partial class B {}
