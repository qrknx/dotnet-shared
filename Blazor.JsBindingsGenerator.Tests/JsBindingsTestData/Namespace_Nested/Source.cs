using JsBindingsGenerator;

namespace A0.A1
{
    namespace A2
    {
        [JsBindingContext("BlazorCallbacks")]
        [JsBind("show", Params = typeof((string s, object obj)), Returns = typeof(int), ResetContext = false)]
        public static partial class B {}
    }
}
