using JsBindingsGenerator;

namespace A;

[JsBindingContext("BlazorCallbacks")]
[JsBind("show", Params = typeof((string s, object obj)), Returns = typeof(int), ResetContext = false)]
public static partial class B {}
