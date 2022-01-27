using JsBindingsGenerator;

namespace A;

[JsBind("M1Async", Returns = typeof(byte[]))]
[JsBind("M2Async", Params = typeof((object[] os, int)))]
[JsBind("M3Async", Params = typeof((byte[] bs, object[] os)))]
public static partial class B {}
