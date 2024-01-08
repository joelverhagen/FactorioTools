using System;
using System.Collections.Generic;
using System.Linq;
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

    public LocationIntSet(int width, int height)
    {
        _width = width;
        _set = new HashSet<int>();
    }

    public LocationIntSet(int width, int height, int capacity)
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

    public void UnionWith(IEnumerable<Location> other)
    {
        _set.UnionWith(other.Select(GetIndex));
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

    public bool Overlaps(IEnumerable<Location> other)
    {
        return _set.Overlaps(other.Select(GetIndex));
    }

    public void Clear()
    {
        _set.Clear();
    }

    public void CopyTo(Location[] array)
    {
        var index = 0;
        foreach (var location in EnumerateItems())
        {
            array[index] = location;
            index++;
        }
    }

    public IEnumerable<Location> EnumerateItems()
    {
        foreach (var item in _set)
        {
            yield return new Location(item % _width, item / _width);
        }
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
