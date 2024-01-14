using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Knapcode.FactorioTools.OilField;

public class LocationIntSet : ILocationSet
{
    private readonly int _width;
    private readonly HashSet<int> _set;

    public LocationIntSet(LocationIntSet existing)
    {
        _width = existing._width;
        _set = new HashSet<int>(existing._set);
    }

    public LocationIntSet(int width)
    {
        _width = width;
        _set = new HashSet<int>();
    }

    public LocationIntSet(int width, int capacity)
    {
        _width = width;
        _set = new HashSet<int>(capacity);
    }

    public int Count => _set.Count;

    public bool Add(Location location)
    {
        return _set.Add(GetIndex(location));
    }

    public bool Remove(Location location)
    {
        return _set.Remove(GetIndex(location));
    }

    public bool Contains(Location location)
    {
        return _set.Contains(GetIndex(location));
    }

    public void UnionWith(IReadOnlyCollection<Location> other)
    {
        foreach (Location location in other)
        {
            _set.Add(GetIndex(location));
        }
    }

    public void UnionWith(ILocationSet other)
    {
        if (other.Count == 0)
        {
            return;
        }
        else if (other.Count == 1)
        {
            foreach (var item in other.EnumerateItems())
            {
                _set.Add(GetIndex(item));
            }
        }
        else
        {
            var otherSet = ValidateSameDimensions(other);
            _set.UnionWith(otherSet._set);
        }
    }

    public void ExceptWith(ILocationSet other)
    {
        var otherSet = ValidateSameDimensions(other);
        _set.ExceptWith(otherSet._set);
    }

    public bool SetEquals(ILocationSet other)
    {
        var otherSet = ValidateSameDimensions(other);
        return _set.SetEquals(otherSet._set);
    }

    public bool Overlaps(IReadOnlyCollection<Location> other)
    {
        if (Count == 0)
        {
            return false;
        }

        foreach (var location in other)
        {
            if (_set.Contains(GetIndex(location)))
            {
                return true;
            }
        }

        return false;
    }

    public void Clear()
    {
        _set.Clear();
    }

    public void CopyTo(Span<Location> array)
    {
        var index = 0;
        foreach (var location in EnumerateItems())
        {
            array[index] = location;
            index++;
        }
    }

    public IReadOnlyCollection<Location> EnumerateItems()
    {
        var items = new List<Location>(_set.Count);
        foreach (var item in _set)
        {
            items.Add(new Location(item % _width, item / _width));
        }
        return items;
    }

    private LocationIntSet ValidateSameDimensions(ILocationSet other)
    {
        var otherSet = (LocationIntSet)other;
        if (otherSet._width != _width)
        {
            throw new ArgumentException("The location set dimensions must be the same.");
        }

        return otherSet;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(Location location)
    {
        return location.Y * _width + location.X;
    }
}
