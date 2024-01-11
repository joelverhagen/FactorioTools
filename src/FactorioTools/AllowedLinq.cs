using System;
using System.Collections.Generic;
using System.Linq;

namespace Knapcode.FactorioTools;

public static class AllowedLinq
{
    public static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        return Enumerable.MaxBy(source, keySelector);
    }

    public static TSource? MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        return Enumerable.MinBy(source, keySelector);
    }

    public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
    {
        return Enumerable.ToList(source);
    }

    public static TResult? Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        return Enumerable.Max(source, selector);
    }

    public static TResult? Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        return Enumerable.Min(source, selector);
    }

    public static TSource Single<TSource>(this IEnumerable<TSource> source)
    {
        return Enumerable.Single(source);
    }

    public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        return Enumerable.Single(source, predicate);
    }

    public static TSource First<TSource>(this IEnumerable<TSource> source)
    {
        return Enumerable.First(source);
    }

    public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        return Enumerable.First(source, predicate);
    }

    public static TSource? FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        return Enumerable.FirstOrDefault(source, predicate);
    }

    public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
    {
        return Enumerable.Average(source, selector);
    }

    public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
        return Enumerable.SequenceEqual(first, second);
    }

    public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
    {
        return Enumerable.Sum(source, selector);
    }
}