namespace Gendarme.Analyzers.Extensions;

internal static class EnumerableExtensions
{
    public static HashSet<TSource> ToHashSet<TSource>(
        this IEnumerable<TSource> source,
        IEqualityComparer<TSource>? comparer = null)
    {
        return new HashSet<TSource>(source, comparer ?? EqualityComparer<TSource>.Default);
    }
}
