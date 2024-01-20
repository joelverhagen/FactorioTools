#if USE_BITARRAY
global using CountedBitArray = Knapcode.FactorioTools.OilField.WrapperCountedBitArray;
#else
global using CountedBitArray = Knapcode.FactorioTools.OilField.CustomCountedBitArray;
#endif

using System;
using System.Collections.Generic;
using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools;

internal static class CollectionExtensions
{
    public static ILocationDictionary<TValue> ToDictionary<TItem, TValue>(
        this IReadOnlyCollection<TItem> items,
        Context context,
        Func<TItem, Location> keySelector,
        Func<TItem, TValue> valueSelector)
    {
        var dictionary = context.GetLocationDictionary<TValue>(items.Count);
        foreach (var item in items)
        {
            dictionary.Add(keySelector(item), valueSelector(item));
        }

        return dictionary;
    }

    public static ILocationDictionary<TValue> ToDictionary<TItem, TValue>(
        this IReadOnlyTableList<TItem> items,
        Context context,
        Func<TItem, Location> keySelector,
        Func<TItem, TValue> valueSelector)
    {
        var dictionary = context.GetLocationDictionary<TValue>(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            dictionary.Add(keySelector(item), valueSelector(item));
        }

        return dictionary;
    }

    public static ITableList<Location> Distinct(this IReadOnlyCollection<Location> locations, Context context)
    {
        var set = context.GetLocationSet(locations.Count);
        var output = TableArray.New<Location>(locations.Count);
        foreach (var location in locations)
        {
            if (set.Add(location))
            {
                output.Add(location);
            }
        }
        return output;
    }

    public static ILocationSet ToSet(this IReadOnlyCollection<Location> locations, Context context)
    {
        return locations.ToSet(context, allowEnumerate: false);
    }

    public static ILocationSet ToSet(this IReadOnlyCollection<Location> locations, Context context, bool allowEnumerate)
    {
        var set = context.GetLocationSet(allowEnumerate);
        foreach (var location in locations)
        {
            set.Add(location);
        }

        return set;
    }

    public static ILocationSet ToReadOnlySet(this IReadOnlyCollection<Location> locations, Context context)
    {
        return locations.ToReadOnlySet(context, allowEnumerate: false);
    }

    public static ILocationSet ToReadOnlySet(this IReadOnlyCollection<Location> locations, Context context, bool allowEnumerate)
    {
        Location firstLocation = Location.Invalid;
        int itemCount = 0;
        ILocationSet? set = null;
        foreach (var location in locations)
        {
            if (itemCount == 0)
            {
                firstLocation = location;
            }
            else if (itemCount == 1)
            {
                set = context.GetLocationSet(allowEnumerate);
                set.Add(firstLocation);
                set.Add(location);
            }
            else
            {
                set!.Add(location);
            }

            itemCount++;
        }

        if (set is null)
        {
            if (itemCount == 0)
            {
                set = EmptyLocationSet.Instance;
            }
            else if (itemCount == 1)
            {
                set = context.GetSingleLocationSet(firstLocation);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        return set;
    }

#if ENABLE_VISUALIZER
    public static ITableArray<DelaunatorSharp.IPoint> ToDelaunatorPoints(this ILocationSet set)
    {
        var points = TableArray.New<DelaunatorSharp.IPoint>();
        foreach (var item in set.EnumerateItems())
        {
            points.Add(new DelaunatorSharp.Point(item.X, item.Y));
        }

        return points;
    }

    public static ITableArray<DelaunatorSharp.IPoint> ToDelaunatorPoints<T>(this ILocationDictionary<T> dictionary)
    {
        var points = TableArray.New<DelaunatorSharp.IPoint>();
        foreach (var item in dictionary.Keys)
        {
            points.Add(new DelaunatorSharp.Point(item.X, item.Y));
        }

        return points;
    }
#endif

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

    public static TResult? Min<TSource, TResult>(this IReadOnlyCollection<TSource> source, Func<TSource, TResult> selector)
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

    public static TSource[] ToArray<TSource>(this IReadOnlyTableList<TSource> source)
    {
        var output = new TSource[source.Count];
        for (var i = 0; i < source.Count; i++)
        {
            output[i] = source[i];
        }

        return output;
    }

    public static ITableList<TSource> ToTableArray<TSource>(this IReadOnlyCollection<TSource> source)
    {
        var output = TableArray.New<TSource>(source.Count);
        output.AddCollection(source);
        return output;
    }

    public static ITableList<TSource> ToTableArray<TSource>(this IReadOnlyTableList<TSource> source)
    {
        var output = TableArray.New<TSource>(source.Count);
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

    public static TSource? FirstOrDefault<TSource>(this IReadOnlyCollection<TSource> source, Func<TSource, bool> predicate)
    {
        foreach (var item in source)
        {
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

    public static bool SequenceEqual<TSource>(this IReadOnlyTableList<TSource> first, IReadOnlyTableList<TSource> second)
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

    public static int Sum<TSource>(this IReadOnlyCollection<TSource> source, Func<TSource, int> selector)
    {
        var sum = 0;
        foreach (var item in source)
        {
            sum += selector(item);
        }

        return sum;
    }
}