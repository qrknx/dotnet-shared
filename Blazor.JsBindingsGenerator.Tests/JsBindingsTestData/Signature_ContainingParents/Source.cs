using System.Collections;
using JsBindingsGenerator;

namespace A;

[JsBind("VisibleNamespaceAsync", Params = typeof((IList list, int)))]
[JsBind("FullyQualifiedAsync", Params = typeof((CustomNamespace.Nested.CustomStruct s, int)))]
public static partial class B {}
