#if USE_HASHSETS
global using LocationSet = System.Collections.Generic.HashSet<Knapcode.FactorioTools.OilField.Location>;
#else
global using LocationSet = Knapcode.FactorioTools.OilField.LocationIntSet;
#endif

using System.Collections.Generic;
using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools;

/// <summary>
/// Workaround for https://github.com/yanghuan/CSharp.lua/issues/443
/// </summary>
internal static class SetHandling
{
#if USE_HASHSETS
    public static HashSet<T> ToSet<T>(this IEnumerable<T> items, Context context)
    {
        return new HashSet<T>(items);
    }

    public static IEnumerable<T> EnumerateItems<T>(this HashSet<T> set)
    {
        return set;
    }
#else
    public static LocationSet ToSet(this IEnumerable<Location> items, Context context)
    {
        var set = context.GetLocationSet();
        foreach (var item in items)
        {
            set.Add(item);
        }

        return set;
    }
#endif
}
