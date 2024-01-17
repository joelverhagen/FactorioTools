using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class SingleLocationSet : ILocationSet
{
    private int _x;
    private int _y;
    private bool _hasItem;

    public SingleLocationSet(SingleLocationSet other)
    {
        _x = other._x;
        _y = other._y;
        _hasItem = other._hasItem;
    }

    public SingleLocationSet(Location location)
    {
        _x = location.X;
        _y = location.Y;
        _hasItem = true;
    }

    public SingleLocationSet()
    {
        _x = -1;
        _y = -1;
        _hasItem = false;
    }

    public int Count => _hasItem ? 1 : 0;

    public bool Add(Location location)
    {
        if (!_hasItem)
        {
            _x = location.X;
            _y = location.Y;
            _hasItem = true;
            return true;
        }
        else if (_x == location.X && _y == location.Y)
        {
            return false;
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public void Clear()
    {
        _hasItem = false;
    }

    public bool Contains(Location location)
    {
        return _hasItem && location.X == _x && location.Y == _y;
    }

    public void CopyTo(Span<Location> array)
    {
        if (!_hasItem)
        {
            return;
        }

        if (array.Length == 0)
        {
            throw new ArgumentException("The array is not big enough.", nameof(array));
        }

        array[0] = new Location(_x, _y);
    }

    public IReadOnlyCollection<Location> EnumerateItems()
    {
        return _hasItem ? new[] { new Location(_x, _y) } : Array.Empty<Location>();
    }

    public void ExceptWith(ILocationSet other)
    {
        throw new NotSupportedException();
    }

    public bool Overlaps(IReadOnlyCollection<Location> other)
    {
        throw new NotSupportedException();
    }

    public bool Remove(Location location)
    {
        if (!_hasItem)
        {
            return false;
        }
        else if (_x == location.X && _y == location.Y)
        {
            _x = -1;
            _y = -1;
            _hasItem = false;
            return true;
        }

        return false;
    }

    public bool SetEquals(ILocationSet other)
    {
        if (other.Count != 1)
        {
            return false;
        }

        return other.Contains(new Location(_x, _y));
    }

    public void UnionWith(IReadOnlyCollection<Location> other)
    {
        throw new NotSupportedException();
    }

    public void UnionWith(ILocationSet other)
    {
        throw new NotSupportedException();
    }
}
