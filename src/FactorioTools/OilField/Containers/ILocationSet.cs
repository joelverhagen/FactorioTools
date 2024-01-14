using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField
{
    public interface ILocationSet
    {
        int Count { get; }

        bool Add(Location location);
        void Clear();
        bool Contains(Location location);
        void CopyTo(Span<Location> array);
        IReadOnlyCollection<Location> EnumerateItems();
        void ExceptWith(ILocationSet other);
        bool Overlaps(IReadOnlyCollection<Location> other);
        bool Remove(Location location);
        bool SetEquals(ILocationSet other);
        void UnionWith(IReadOnlyCollection<Location> other);
        void UnionWith(ILocationSet other);
    }
}