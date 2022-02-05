using BlazorJsBindingsGenerator;

namespace A;

[JsBind("M1Async")]
[JsBind("M2Async")]
public static partial class B {}

[JsBind("M1Async")]
public static partial class C {}
