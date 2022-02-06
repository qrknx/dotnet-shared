using BlazorJsBindingsGenerator;

namespace A;

[JsBindingContext("BlazorCallbacks")]
[JsBind("M1Async")]
[JsBind("M2Async", ResetContext = true)]
[JsBind("M3Async", ResetContext = false)]
[JsBind("SomePrefix.M4Async", ResetContext = true)]
[JsBind("SomePrefix.M5Async")]
public static partial class B {}
