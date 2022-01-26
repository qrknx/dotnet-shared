using JsBindingsGenerator;

namespace A;

internal struct CustomStruct {}

[JsBind("M1Async", Params = typeof((CustomStruct s, int)))]
[JsBind("M2Async", Returns = typeof(CustomStruct))]
public static partial class B {}

[JsBind("M1Async", Returns = typeof(int))]
internal static partial class C {}
