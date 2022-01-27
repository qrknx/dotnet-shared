using JsBindingsGenerator;

namespace A;

[JsBind("SingleParamAsync", Params = typeof((int i, int)))]
[JsBind("ManyParamsAsync", Params = typeof((string s, int i, object obj)))]
public static partial class B {}
