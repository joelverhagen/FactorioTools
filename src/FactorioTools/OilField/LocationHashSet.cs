using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class LocationHashSet : ILocationSet
{
    private readonly HashSet<Location> _set;

    public LocationHashSet(LocationHashSet existing)
    {
        _set = new HashSet<Location>(existing._set);
    }

    public LocationHashSet(int width, int height)
    {
        _set = new HashSet<Location>();
    }

    public LocationHashSet(int width, int height, int capacity)
    {
        _set = new HashSet<Location>(capacity);
    }

    public int Count => _set.Count;
    public int Height { get; }
    public int Width { get; }

    public bool Add(Location location)
    {
        return _set.Add(location);
    }

    public void Clear()
    {
        _set.Clear();
    }

    public bool Contains(Location location)
    {
        return _set.Contains(location);
    }

    public void CopyTo(Location[] array)
    {
        _set.CopyTo(array);
    }

    public IEnumerable<Location> EnumerateItems()
    {
        return _set;
    }

    public void ExceptWith(ILocationSet other)
    {
        var otherSet = (LocationHashSet)other;
        _set.ExceptWith(otherSet._set);
    }

    public bool Overlaps(IEnumerable<Location> other)
    {
        return _set.Overlaps(other);
    }

    public bool Remove(Location location)
    {
        return _set.Remove(location);
    }

    public bool SetEquals(ILocationSet other)
    {
        var otherSet = (LocationHashSet)other;
        return _set.SetEquals(otherSet._set);
    }

    public void UnionWith(IEnumerable<Location> other)
    {
        _set.UnionWith(other);
    }

    public void UnionWith(ILocationSet other)
    {
        var otherSet = (LocationHashSet)other;
        _set.UnionWith(otherSet._set);
    }
}
