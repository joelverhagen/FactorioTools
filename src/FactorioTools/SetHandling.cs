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
        this IEnumerable<TItem> items,
        Context context,
        Func<TItem, Location> keySelector,
        Func<TItem, TValue> valueSelector)
    {
        var dictionary = context.GetLocationDictionary<TValue>();
        foreach (var item in items)
        {
            dictionary.Add(keySelector(item), valueSelector(item));
        }

        return dictionary;
    }

    public static IEnumerable<Location> Distinct(this IEnumerable<Location> locations, Context context)
    {
        var set = context.GetLocationSet();
        foreach (var location in locations)
        {
            if (set.Add(location))
            {
                yield return location;
            }
        }
    }

    public static IEnumerable<Location> Except(this IEnumerable<Location> locations, IEnumerable<Location> other, Context context)
    {
        return locations.ExceptSet(other, context, allowEnumerate: true).EnumerateItems();
    }

    public static ILocationSet ExceptSet(this IEnumerable<Location> locations, IEnumerable<Location> other, Context context, bool allowEnumerate)
    {
        var set = context.GetLocationSet(locations, allowEnumerate);
        foreach (var item in other)
        {
            set.Remove(item);
        }

        return set;
    }

    public static ILocationSet ToSet(this IEnumerable<Location> locations, Context context)
    {
        return context.GetLocationSet(locations);
    }

    public static ILocationSet ToSet(this IEnumerable<Location> locations, Context context, bool allowEnumerate)
    {
        return context.GetLocationSet(locations, allowEnumerate);
    }

    public static ILocationSet ToReadOnlySet(this IEnumerable<Location> locations, Context context)
    {
        return context.GetReadOnlyLocationSet(locations);
    }

    public static ILocationSet ToReadOnlySet(this IEnumerable<Location> locations, Context context, bool allowEnumerate)
    {
        return context.GetReadOnlyLocationSet(locations, allowEnumerate);
    }
}
