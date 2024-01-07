using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField
{
    public interface ILocationSet
    {
        int Count { get; }

        bool Add(Location location);
        void Clear();
        bool Contains(Location location);
        void CopyTo(Location[] array);
        IEnumerable<Location> EnumerateItems();
        void ExceptWith(ILocationSet other);
        bool Overlaps(IEnumerable<Location> other);
        bool Remove(Location location);
        bool SetEquals(ILocationSet other);
        void UnionWith(IEnumerable<Location> other);
        void UnionWith(ILocationSet other);
    }
}