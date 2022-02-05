using System;
using System.Collections;
using BlazorJsBindingsGenerator;
using Double = System.Int16;
using Int16 = System.Single;

namespace A;

using IEnumerable = IDisposable;

[JsBind("VisibleNamespaceAsync", Params = typeof((IList list, Int32 i)))]
[JsBind("WithAliasAsync", Params = typeof((Double s, IEnumerable d)))]
[JsBind("FullyQualifiedAsync", Params = typeof((CustomNamespace.Nested.CustomStruct s, int)))]
public static partial class B {}
