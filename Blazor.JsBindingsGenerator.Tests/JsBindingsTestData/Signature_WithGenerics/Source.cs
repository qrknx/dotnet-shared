#nullable enable

using System.Collections;
using System.Collections.Generic;
using JsBindingsGenerator;

namespace A;

public struct CustomStruct {}

[JsBind("M1Async", Params = typeof((List<int> ls, int)))]
[JsBind("M2Async", Params = typeof((List<int?>? ls, int)), Returns = typeof(int))]
[JsBind("M3Async", Params = typeof((List<object?> ls, (object? o, object o1, int? i, int i1, List<object?> ls, IList ls1) ts)))]
[JsBind("M4Async", Params = typeof((List<KeyValuePair<CustomStruct?, string?>?> kvs, int)))]
public static partial class B {}
