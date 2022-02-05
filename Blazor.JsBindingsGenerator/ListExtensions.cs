namespace JsBindingsGenerator;

internal static class ListExtensions
{
    public static bool SequenceEqual<T>(this List<T> first, List<T> second)
        where T : IEquatable<T>
    {
        if (first.Count == second.Count)
        {
            for (int i = 0; i < first.Count; ++i)
            {
                if (!first[i].Equals(second[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }
}
