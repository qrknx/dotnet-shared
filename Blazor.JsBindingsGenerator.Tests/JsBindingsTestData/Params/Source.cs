using JsBindingsGenerator;

namespace A;

[JsBind("SingleParamAsync", Params = typeof((int i, int)))]
[JsBind("M1Async", Params = typeof((string s, int i, object obj)))]
public static partial class B {}
