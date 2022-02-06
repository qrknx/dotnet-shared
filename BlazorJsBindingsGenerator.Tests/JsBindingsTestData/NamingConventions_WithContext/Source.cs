using BlazorJsBindingsGenerator;

namespace A;

[JsBindingContext(Prefix = "BlazorCallbacks")]
[JsBind("M1Async")]
[JsBind("M2Async", ResetPrefix = true)]
[JsBind("M3Async", ResetPrefix = false)]
[JsBind("SomePrefix.M4Async", ResetPrefix = true)]
[JsBind("SomePrefix.M5Async")]
[JsBindingContext(Prefix = "")]
[JsBind("M6Async")]
public static partial class B {}
