using System.Collections.Generic;
using JsBindingsGenerator;

namespace A;

internal struct CustomClass {}

[JsBind("M1Async", Params = typeof((CustomClass c, int)))]
[JsBind("M2Async", Returns = typeof(CustomClass))]
[JsBind("M3Async", Returns = typeof(CustomClass?[]))]
[JsBind("M4Async", Returns = typeof(CustomClass?))]
[JsBind("M5Async", Returns = typeof(List<CustomClass?>))]
[JsBind("M6Async", Returns = typeof((CustomClass?, int)))]
public static partial class B {}

[JsBind("M1Async", Returns = typeof(int))]
internal static partial class C {}
