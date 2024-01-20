﻿using System;
using System.Collections.Generic;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public class Context
{
    public required OilFieldOptions Options { get; set; }
    public required Blueprint InputBlueprint { get; set; }
    public required float DeltaX { get; set; }
    public required float DeltaY { get; set; }
    public required SquareGrid Grid { get; set; }
    public required ITableList<Location> Centers { get; set; }
    public required ILocationDictionary<ITableList<TerminalLocation>> CenterToTerminals { get; set; }
    public required ILocationDictionary<Direction> CenterToOriginalDirection { get; set; }
    public required ILocationDictionary<ITableList<TerminalLocation>> LocationToTerminals { get; set; }
    public required int[] LocationToAdjacentCount { get; set; }

    public required SharedInstances SharedInstances { get; set; }

    public ILocationDictionary<T> GetLocationDictionary<T>()
    {
#if USE_HASHSETS
        return new LocationHashDictionary<T>();
#else
        return new LocationIntDictionary<T>(Grid.Width);
#endif
    }

    public ILocationDictionary<T> GetLocationDictionary<T>(int capacity)
    {
#if USE_HASHSETS
        return new LocationHashDictionary<T>(capacity);
#else
        return new LocationIntDictionary<T>(Grid.Width, capacity);
#endif
    }

    public ILocationSet GetLocationSet(ILocationSet other)
    {
#if USE_HASHSETS
        if (other is LocationHashSet enumerateSet)
        {
            return new LocationHashSet(enumerateSet);
        }
#else
        if (other is LocationIntSet enumerateSet)
        {
            return new LocationIntSet(enumerateSet);
        }
#endif

        if (other is SingleLocationSet singleSet)
        {
            return new SingleLocationSet(singleSet);
        }

        return new LocationBitSet((LocationBitSet)other);
    }

    public ILocationSet GetLocationSet()
    {
        return GetLocationSet(allowEnumerate: false);
    }

    public ILocationSet GetLocationSet(bool allowEnumerate)
    {
        return allowEnumerate
#if USE_HASHSETS
            ? new LocationHashSet()
#else
            ? new LocationIntSet(Grid.Width)
#endif
            : new LocationBitSet(Grid.Width, Grid.Height);
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
        return allowEnumerate
#if USE_HASHSETS
            ? new LocationHashSet(capacity)
#else
            ? new LocationIntSet(Grid.Width, capacity)
#endif
            : new LocationBitSet(Grid.Width, Grid.Height);
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

    public SingleLocationSet GetSingleLocationSet(Location location)
    {
        return new SingleLocationSet(location);
    }
}
