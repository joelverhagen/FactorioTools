#if USE_HASHSETS
global using LocationSet = System.Collections.Generic.HashSet<Knapcode.FactorioTools.OilField.Location>;
global using ElectricPoleCenterSet = System.Collections.Generic.HashSet<Knapcode.FactorioTools.OilField.ElectricPoleCenter>;
#else
global using LocationSet = System.Collections.Generic.Dictionary<Knapcode.FactorioTools.OilField.Location, bool>;
global using ElectricPoleCenterSet = System.Collections.Generic.Dictionary<Knapcode.FactorioTools.OilField.ElectricPoleCenter, bool>;
#endif

using System.Collections.Generic;

namespace Knapcode.FactorioTools;

/// <summary>
/// Workaround for https://github.com/yanghuan/CSharp.lua/issues/443
/// </summary>
internal static class SetHandling
{
#if USE_HASHSETS
    public static HashSet<T> ToSet<T>(this IEnumerable<T> items)
    {
        return new HashSet<T>(items);
    }

    public static IEnumerable<T> EnumerateItems<T>(this HashSet<T> set)
    {
        return set;
    }
#else
    public static IEnumerable<T> EnumerateItems<T>(this Dictionary<T, bool> set) where T : notnull
    {
        return set.Keys;
    }

    public static Dictionary<T, bool> ToSet<T>(this IEnumerable<T> items) where T : notnull
    {
        var dictionary = new Dictionary<T, bool>();
        foreach (var item in items)
        {
            dictionary.TryAdd(item, true);
        }

        return dictionary;
    }

    public static void CopyTo<T>(this Dictionary<T, bool> set, T[] array) where T : notnull
    {
        if (set.Count > array.Length)
        {
            throw new System.ArgumentException("The array is not large enough to hold the set.");
        }

        var index = 0;
        foreach (var item in set.Keys)
        {
            array[index] = item;
            index++;
        }
    }

    public static bool Contains<T>(this Dictionary<T, bool> set, T item) where T : notnull
    {
        return set.ContainsKey(item);
    }

    public static bool Add<T>(this Dictionary<T, bool> set, T item) where T : notnull
    {
        return set.TryAdd(item, true);
    }

    public static void ExceptWith<T>(this Dictionary<T, bool> set, IEnumerable<T> items) where T : notnull
    {
        foreach (var item in items)
        {
            set.Remove(item);
        }
    }

    public static void ExceptWith<T>(this Dictionary<T, bool> set, Dictionary<T, bool> items) where T : notnull
    {
        set.ExceptWith(items.Keys);
    }

    public static void IntersectWith<T>(this Dictionary<T, bool> set, IEnumerable<T> items) where T : notnull
    {
        var overlap = new Dictionary<T, bool>();

        foreach (var item in items)
        {
            if (set.ContainsKey(item))
            {
                overlap.Add(item);
            }
        }

        var missing = new List<T>();

        foreach (var item in set.EnumerateItems())
        {
            if (!overlap.ContainsKey(item))
            {
                missing.Add(item);
            }    
        }

        foreach (var item in missing)
        {
            set.Remove(item);
        }
    }

    public static void IntersectWith<T>(this Dictionary<T, bool> set, Dictionary<T, bool> items) where T : notnull
    {
        set.IntersectWith(items.Keys);
    }

    public static bool Overlaps<T>(this Dictionary<T, bool> set, IEnumerable<T> items) where T : notnull
    {
        foreach (var item in items)
        {
            if (set.ContainsKey(item))
            {
                return true;
            }
        }

        return false;
    }

    public static bool Overlaps<T>(this Dictionary<T, bool> set, Dictionary<T, bool> items) where T : notnull
    {
        return set.Overlaps(items.Keys);
    }

    public static bool SetEquals<T>(this Dictionary<T, bool> set, IEnumerable<T> items) where T : notnull
    {
        var count = 0;
        foreach (var item in items)
        {
            count++;

            if (!set.ContainsKey(item))
            {
                return false;
            }
        }

        return count == set.Count;
    }

    public static bool SetEquals<T>(this Dictionary<T, bool> set, Dictionary<T, bool> items) where T : notnull
    {
        return set.SetEquals(items.Keys);
    }

    public static void UnionWith<T>(this Dictionary<T, bool> set, IEnumerable<T> items) where T : notnull
    {
        foreach (var item in items)
        {
            set.TryAdd(item, true);
        }
    }

    public static void UnionWith<T>(this Dictionary<T, bool> set, Dictionary<T, bool> items) where T : notnull
    {
        set.UnionWith(items.Keys);
    }
#endif
}
