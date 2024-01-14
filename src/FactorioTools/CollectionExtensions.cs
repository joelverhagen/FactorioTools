using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools;

public static class CollectionExtensions
{
    public static TSource? MaxBy<TSource, TKey>(this IReadOnlyCollection<TSource> source, Func<TSource, TKey> keySelector)
    {
        TKey? maxKey = default;
        TSource? max = default;
        var hasItem = false;
        var comparer = Comparer<TKey>.Default;
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (hasItem)
            {
                if (comparer.Compare(key, maxKey) > 0)
                {
                    maxKey = key;
                    max = item;
                }
            }
            else
            {
                maxKey = key;
                max = item;
                hasItem = true;
            }
        }

        return max;
    }

    public static TResult? Max<TSource, TResult>(this IReadOnlyCollection<TSource> source, Func<TSource, TResult> selector)
    {
        TResult? max = default;
        var hasItem = false;
        var comparer = Comparer<TResult>.Default;
        foreach (var item in source)
        {
            var cmp = selector(item);
            if (hasItem)
            {
                if (comparer.Compare(cmp, max) > 0)
                {
                    max = cmp;
                }
            }
            else
            {
                max = cmp;
                hasItem = true;
            }
        }

        return max;
    }

    public static TSource? MinBy<TSource, TKey>(this IReadOnlyCollection<TSource> source, Func<TSource, TKey> keySelector)
    {
        TKey? minKey = default;
        TSource? min = default;
        var hasItem = false;
        var comparer = Comparer<TKey>.Default;
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (hasItem)
            {
                if (comparer.Compare(key, minKey) < 0)
                {
                    minKey = key;
                    min = item;
                }
            }
            else
            {
                minKey = key;
                min = item;
                hasItem = true;
            }
        }

        return min;
    }

    public static TResult? Min<TSource, TResult>(this IReadOnlyList<TSource> source, Func<TSource, TResult> selector)
    {
        TResult? min = default;
        var hasItem = false;
        var comparer = Comparer<TResult>.Default;
        foreach (var item in source)
        {
            var cmp = selector(item);
            if (hasItem)
            {
                if (comparer.Compare(cmp, min) < 0)
                {
                    min = cmp;
                }
            }
            else
            {
                min = cmp;
                hasItem = true;
            }
        }

        return min;
    }

    public static List<TSource> ToList<TSource>(this IReadOnlyCollection<TSource> source)
    {
        var output = new List<TSource>(source.Count);
        output.AddRange(source);
        return output;
    }

    public static TSource Single<TSource>(this IReadOnlyCollection<TSource> source)
    {
        TSource? single = default;
        var hasItem = false;
        foreach (var item in source)
        {
            if (hasItem)
            {
                throw new FactorioToolsException("Only one item should exist in the source.");
            }
            else
            {
                single = item;
                hasItem = true;
            }
        }

        if (hasItem)
        {
            return single!;
        }

        throw new FactorioToolsException("An item should exist in the source.");
    }

    public static TSource Single<TSource>(this IReadOnlyCollection<TSource> source, Func<TSource, bool> predicate)
    {
        TSource? single = default;
        var hasItem = false;
        foreach (var item in source)
        {
            if (predicate(item))
            {
                if (hasItem)
                {
                    throw new FactorioToolsException("Only one item should have matched the predicate.");
                }
                else
                {
                    single = item;
                    hasItem = true;
                }
            }
        }

        if (hasItem)
        {
            return single!;
        }

        throw new FactorioToolsException("An item should have matched the predicate.");
    }

    public static TSource First<TSource>(this IReadOnlyCollection<TSource> source)
    {
        foreach (var item in source)
        {
            return item;
        }

        throw new FactorioToolsException("An item should have matched the predicate.");
    }

    public static TSource First<TSource>(this IReadOnlyCollection<TSource> source, Func<TSource, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                return item;
            }
        }

        throw new FactorioToolsException("An item should have matched the predicate.");
    }

    public static TSource? FirstOrDefault<TSource>(this IReadOnlyList<TSource> source, Func<TSource, bool> predicate)
    {
        for (int i = 0; i < source.Count; i++)
        {
            var item = source[i];
            if (predicate(item))
            {
                return item;
            }
        }

        return default;
    }

    public static double Average<TSource>(this IReadOnlyCollection<TSource> source, Func<TSource, int> selector)
    {
        if (source.Count == 0)
        {
            return 0;
        }

        double sum = 0;
        var count = 0;
        foreach (var item in source)
        {
            sum += selector(item);
            count++;
        }

        return sum / count;
    }

    public static bool SequenceEqual<TSource>(this IReadOnlyList<TSource> first, IReadOnlyList<TSource> second)
    {
        if (first.Count != second.Count)
        {
            return false;
        }

        var comparer = EqualityComparer<TSource>.Default;

        for (var i = 0; i < first.Count; i++)
        {
            if (!comparer.Equals(first[i], second[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static int Sum<TSource>(this IReadOnlyList<TSource> source, Func<TSource, int> selector)
    {
        var sum = 0;
        for (var i = 0; i < source.Count; i++)
        {
            sum += selector(source[i]);
        }

        return sum;
    }
}