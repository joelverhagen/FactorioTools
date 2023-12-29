using System.Collections.Generic;
using System.Linq;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools;

internal static class ExtensionMethods
{
    public static LocationSet ToLocationSet(this IEnumerable<Location> locations)
    {
        return locations.ToHashSet();
    }
}
