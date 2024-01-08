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
