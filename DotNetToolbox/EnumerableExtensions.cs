namespace DotNetToolbox;

public static class EnumerableExtensions
{
    public static List<TResult> ToList<TSource, TResult>(this IEnumerable<TSource> source,
                                                         Func<TSource, int, TResult> selector)
    {
        return source.Select(selector)
                     .ToList();
    }

    public static List<TResult> ToList<TSource, TResult>(this IEnumerable<TSource> source,
                                                         Func<TSource, TResult> selector)
    {
        return source.Select(selector)
                     .ToList();
    }

    public static TResult[] ToArray<TSource, TResult>(this IEnumerable<TSource> source,
                                                      Func<TSource, int, TResult> selector)
    {
        return source.Select(selector)
                     .ToArray();
    }

    public static TResult[] ToArray<TSource, TResult>(this IEnumerable<TSource> source,
                                                      Func<TSource, TResult> selector)
    {
        return source.Select(selector)
                     .ToArray();
    }
}
