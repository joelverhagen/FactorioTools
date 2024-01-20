using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public interface IReadOnlyTableArray<T>
{
    T this[int index] { get; }
    int Count { get; }

    bool Contains(T item);
    IReadOnlyCollection<T> EnumerateItems();
}
