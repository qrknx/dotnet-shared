using System.Collections;
using JsBindingsGenerator;

namespace A;

public struct CustomStruct {}

[JsBind("M1Async", Returns = typeof(byte[]))]
[JsBind("M2Async", ReturnsNullable = typeof(object?[]))]
[JsBind("M3Async", ReturnsNullable = typeof(IList?[]))]
[JsBind("M4Async", ReturnsNullable = typeof(CustomStruct?[]))]
[JsBind("M5Async", Params = typeof((object[] os, int)))]
[JsBind("M6Async", Params = typeof((byte[] bs, object[] os)))]
public static partial class B {}
