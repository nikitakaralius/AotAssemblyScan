namespace AotAssemblyScan.Extensions;

internal static class ReadOnlySpanExtensions
{
    internal static bool All<TSource>(
        this ReadOnlySpan<TSource> span,
        Func<TSource, bool> predicate)
    {
        foreach (var element in span)
        {
            if (!predicate(element))
                return false;
        }

        return true;
    }
}
