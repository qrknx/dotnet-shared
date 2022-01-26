using JsBindingsGenerator;

namespace A;

[JsBind("show", Params = typeof((string s, object obj)), Returns = typeof(int))]
public static partial class B {}
