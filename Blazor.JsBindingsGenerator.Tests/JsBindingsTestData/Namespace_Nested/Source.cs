using JsBindingsGenerator;

namespace A0.A1
{
    namespace A2
    {
        [JsBind("show", Params = typeof((string s, object obj)), Returns = typeof(int))]
        public static partial class B {}
    }
}
