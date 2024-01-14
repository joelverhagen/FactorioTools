using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class EmptyLocationSet : ILocationSet
{
    public static readonly EmptyLocationSet Instance = new EmptyLocationSet();

    public int Count => 0;

    public bool Add(Location location)
    {
        throw new NotSupportedException();
    }

    public void Clear()
    {
    }

    public bool Contains(Location location)
    {
        return false;
    }

    public void CopyTo(Span<Location> array)
    {
    }

    public IReadOnlyCollection<Location> EnumerateItems()
    {
        return Array.Empty<Location>();
    }

    public void ExceptWith(ILocationSet other)
    {
    }

    public bool Overlaps(IReadOnlyCollection<Location> other)
    {
        return false;
    }

    public bool Remove(Location location)
    {
        return false;
    }

    public bool SetEquals(ILocationSet other)
    {
        return other.Count == 0;
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
