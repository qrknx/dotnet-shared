namespace Blazor.JsBindingsGenerator.Compatibility;

internal static class Compatible
{
    public static string StringJoin(char c, IEnumerable<string> strings) => string.Join($"{c}", strings);

    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count)
    {
        List<T> list = source.ToList();

        int take = list.Count - count;
        int i = 0;

        while (i < take)
        {
            yield return list[i];
            ++i;
        }
    }
}
