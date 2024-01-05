using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Knapcode.FactorioTools.OilField;

public class LocationIntSet
{
    private readonly HashSet<int> _set;

    public LocationIntSet(LocationIntSet existing)
    {
        Width = existing.Width;
        Height = existing.Height;
        _set = new HashSet<int>(existing._set);
    }

    public LocationIntSet(int width, int height)
    {
        Width = width;
        Height = height;
        _set = new HashSet<int>();
    }

    public LocationIntSet(int width, int height, int capacity)
    {
        Width = width;
        Height = height;
        _set = new HashSet<int>(capacity);
    }

    public int Width { get; }
    public int Height { get; }

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

    public void UnionWith(LocationIntSet other)
    {
        ValidateSameDimensions(other);
        _set.UnionWith(other._set);
    }

    public void ExceptWith(LocationIntSet other)
    {
        ValidateSameDimensions(other);
        _set.ExceptWith(other._set);
    }

    public bool SetEquals(LocationIntSet other)
    {
        ValidateSameDimensions(other);
        return _set.SetEquals(other._set);
    }

    public bool Overlaps(IEnumerable<Location> other)
    {
        return _set.Overlaps(other.Select(GetIndex));
    }

    private void ValidateSameDimensions(LocationIntSet other)
    {
        if (other.Width != Width || other.Height != Height)
        {
            throw new ArgumentException("The location set dimensions must be the same.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(Location location)
    {
        return location.Y * Width + location.X;
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
            yield return new Location(item % Width, item / Width);
        }
    }
}
