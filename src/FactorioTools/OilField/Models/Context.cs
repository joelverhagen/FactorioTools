using System.Collections.Generic;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public class Context
{
    public required OilFieldOptions Options { get; set; }
    public required Blueprint InputBlueprint { get; set; }
    public required SquareGrid Grid { get; set; }
    public required Dictionary<Location, List<TerminalLocation>> CenterToTerminals { get; set; }
    public required Dictionary<Location, Direction> CenterToOriginalDirection { get; set; }
    public required Dictionary<Location, List<TerminalLocation>> LocationToTerminals { get; set; }
    public required int[,] LocationToAdjacentCount { get; set; }

    public required SharedInstances SharedInstances { get; set; }

    public LocationSet GetLocationSet()
    {
        return new LocationSet(Grid.Width, Grid.Height);
    }

    public LocationSet GetLocationSet(int capacity)
    {
        return new LocationSet(Grid.Width, Grid.Height, capacity);
    }

    public LocationSet GetLocationSet(Location location)
    {
        var set = GetLocationSet();
        set.Add(location);
        return set;
    }

    public LocationSet GetLocationSet(Location location, int capacity)
    {
        var set = GetLocationSet(capacity);
        set.Add(location);
        return set;
    }

    public LocationBitSet GetLocationBitSet()
    {
        return new LocationBitSet(Grid.Width, Grid.Height);
    }

    public LocationBitSet GetLocationBitSet(Location location)
    {
        var set = GetLocationBitSet();
        set.Add(location);
        return set;
    }
}
