#if USE_BITARRAY
global using CountedBitArray = Knapcode.FactorioTools.OilField.WrapperCountedBitArray;
#else
global using CountedBitArray = Knapcode.FactorioTools.OilField.CustomCountedBitArray;
#endif

using System;
using System.Collections.Generic;
using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools;

/// <summary>
/// Workaround for https://github.com/yanghuan/CSharp.lua/issues/443
/// </summary>
internal static class SetHandling
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

    public static List<Location> Distinct(this IReadOnlyCollection<Location> locations, Context context)
    {
        var set = context.GetLocationSet(locations.Count);
        var output = new List<Location>(locations.Count);
        foreach (var location in locations)
        {
            if (set.Add(location))
            {
                output.Add(location);
            }
        }
        return output;
    }

    public static ILocationSet ToSet(this IReadOnlyCollection<Location> locations, Context context, bool allowEnumerate)
    {
        return context.GetLocationSet(locations, allowEnumerate);
    }

    public static ILocationSet ToReadOnlySet(this IReadOnlyCollection<Location> locations, Context context)
    {
        return context.GetReadOnlyLocationSet(locations);
    }

    public static ILocationSet ToReadOnlySet(this IReadOnlyCollection<Location> locations, Context context, bool allowEnumerate)
    {
        return context.GetReadOnlyLocationSet(locations, allowEnumerate);
    }

#if ENABLE_VISUALIZER
    public static List<DelaunatorSharp.IPoint> ToDelaunatorPoints(this ILocationSet set)
    {
        var points = new List<DelaunatorSharp.IPoint>();
        foreach (var item in set.EnumerateItems())
        {
            points.Add(new DelaunatorSharp.Point(item.X, item.Y));
        }

        return points;
    }
    
    public static List<DelaunatorSharp.IPoint> ToDelaunatorPoints<T>(this ILocationDictionary<T> dictionary)
    {
        var points = new List<DelaunatorSharp.IPoint>();
        foreach (var item in dictionary.Keys)
        {
            points.Add(new DelaunatorSharp.Point(item.X, item.Y));
        }

        return points;
    }
#endif
}
