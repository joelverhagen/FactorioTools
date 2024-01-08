using System;
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

    public ILocationSet GetLocationSet(ILocationSet other)
    {
        if (other is LocationSet enumerateSet)
        {
            return new LocationSet(enumerateSet);
        }

        return new LocationBitSet((LocationBitSet)other);
    }

    public ILocationSet GetLocationSet()
    {
        return GetLocationSet(allowEnumerate: false);
    }

    public ILocationSet GetLocationSet(bool allowEnumerate)
    {
        return allowEnumerate ? new LocationSet(Grid.Width, Grid.Height) : new LocationBitSet(Grid.Width, Grid.Height);
    }

    public ILocationSet GetLocationSet(Location location)
    {
        return GetLocationSet(location, allowEnumerate: false);
    }

    public ILocationSet GetLocationSet(Location location, bool allowEnumerate)
    {
        var set = GetLocationSet(allowEnumerate);
        set.Add(location);
        return set;
    }

    public ILocationSet GetLocationSet(int capacity)
    {
        return GetLocationSet(capacity, allowEnumerate: false);
    }

    public ILocationSet GetLocationSet(int capacity, bool allowEnumerate)
    {
        return allowEnumerate ? new LocationSet(Grid.Width, Grid.Height, capacity) : new LocationBitSet(Grid.Width, Grid.Height);
    }

    public ILocationSet GetLocationSet(Location location, int capacity)
    {
        return GetLocationSet(location, capacity, allowEnumerate: false);
    }

    public ILocationSet GetLocationSet(Location location, int capacity, bool allowEnumerate)
    {
        var set = GetLocationSet(capacity, allowEnumerate);
        set.Add(location);
        return set;
    }

    public ILocationSet GetLocationSet(IEnumerable<Location> locations)
    {
        return GetLocationSet(locations, allowEnumerate: false);
    }

    public ILocationSet GetLocationSet(IEnumerable<Location> locations, bool allowEnumerate)
    {
        var set = GetLocationSet(allowEnumerate);
        foreach (var location in locations)
        {
            set.Add(location);
        }

        return set;
    }

    public ILocationSet GetReadOnlyLocationSet(IEnumerable<Location> locations)
    {
        return GetReadOnlyLocationSet(locations, allowEnumerate: false);
    }

    public ILocationSet GetReadOnlyLocationSet(IEnumerable<Location> locations, bool allowEnumerate)
    {
        Location firstLocation = default;
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
                set = GetLocationSet(allowEnumerate);
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
                set = new SingleLocationSet(firstLocation);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        return set;
    }

    public SingleLocationSet GetSingleLocationSet(Location location)
    {
        return new SingleLocationSet(location);
    }
}
