using BlazorJsBindingsGenerator;

namespace A;

[JsBindingContext(JsPrefix = "BlazorCallbacks")]
[JsBind("M1Async")]
[JsBind("M2Async", ResetJsPrefix = true)]
[JsBind("M3Async", ResetJsPrefix = false)]
[JsBind("SomePrefix.M4Async", ResetJsPrefix = true)]
[JsBind("SomePrefix.M5Async")]
[JsBindingContext(JsPrefix = "")]
[JsBind("M6Async")]
public static partial class B {}
