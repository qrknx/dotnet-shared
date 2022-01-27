using System.Collections;
using JsBindingsGenerator;

namespace A;

public struct CustomStruct {}

[JsBind("M1Async", Returns = typeof(byte[]))]
[JsBind("M2Async", Returns = typeof(object?[]))]
[JsBind("M3Async", Returns = typeof(IList?[]))]
[JsBind("M4Async", Returns = typeof(CustomStruct?[]))]
[JsBind("M5Async", Params = typeof((object[] os, int)))]
[JsBind("M6Async", Params = typeof((byte[] bs, object[] os)))]
public static partial class B {}
