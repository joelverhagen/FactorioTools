using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Knapcode.FactorioTools.OilField;

public class LocationBitSet : ILocationSet
{
    private readonly int _width;
    private readonly CustomCountedBitArray _set;

    public LocationBitSet(LocationBitSet existing)
    {
        _width = existing._width;
        _set = new CustomCountedBitArray(existing._set);
    }

    public LocationBitSet(int width, int height)
    {
        _width = width;
        _set = new CustomCountedBitArray(width * height);
    }

    public int Count => _set.TrueCount;

    public bool Add(Location location)
    {
        return !_set.Set(GetIndex(location), true);
    }

    public void Clear()
    {
        _set.SetAll(false);
    }

    public bool Contains(Location location)
    {
        return _set[GetIndex(location)];
    }

    public void CopyTo(Span<Location> array)
    {
        throw new NotSupportedException();
    }

    public IReadOnlyCollection<Location> EnumerateItems()
    {
        throw new NotSupportedException();
    }

    public void ExceptWith(ILocationSet other)
    {
        var otherSet = new CustomCountedBitArray(ValidateSameDimensions(other)._set);
        otherSet.Not();
        _set.And(otherSet);
    }

    public bool Overlaps(IEnumerable<Location> other)
    {
        foreach (var item in other)
        {
            if (_set[GetIndex(item)])
            {
                return true;
            }
        }

        return false;
    }

    public bool Remove(Location location)
    {
        return _set.Set(GetIndex(location), false);
    }

    public bool SetEquals(ILocationSet other)
    {
        var otherSet = ValidateSameDimensions(other);
        return _set.Equals(otherSet);
    }

    public void UnionWith(IEnumerable<Location> other)
    {
        foreach (var item in other)
        {
            _set.Set(GetIndex(item), true);
        }
    }

    public void UnionWith(ILocationSet other)
    {
        var otherSet = ValidateSameDimensions(other);
        _set.Or(otherSet._set);
    }

    private LocationBitSet ValidateSameDimensions(ILocationSet other)
    {
        var otherSet = (LocationBitSet)other;
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
