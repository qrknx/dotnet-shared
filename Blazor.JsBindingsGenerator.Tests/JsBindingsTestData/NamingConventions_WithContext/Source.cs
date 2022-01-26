using JsBindingsGenerator;

namespace A;

[JsBindingContext("BlazorCallbacks")]
[JsBind("M1Async")]
[JsBind("M2Async", ResetContext = true)]
[JsBind("M3Async", ResetContext = false)]
public static partial class B {}
