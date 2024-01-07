#if USE_HASHSETS
global using LocationSet = Knapcode.FactorioTools.OilField.LocationHashSet;
#else
global using LocationSet = Knapcode.FactorioTools.OilField.LocationIntSet;
#endif

#if USE_BITARRAY
global using CountedBitArray = Knapcode.FactorioTools.OilField.WrapperCountedBitArray;
#else
global using CountedBitArray = Knapcode.FactorioTools.OilField.CustomCountedBitArray;
#endif

using System.Collections.Generic;
using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools;

/// <summary>
/// Workaround for https://github.com/yanghuan/CSharp.lua/issues/443
/// </summary>
internal static class SetHandling
{
    public static LocationSet ToSet(this IEnumerable<Location> items, Context context)
    {
        var set = context.GetLocationSet();
        foreach (var item in items)
        {
            set.Add(item);
        }

        return set;
    }
}
