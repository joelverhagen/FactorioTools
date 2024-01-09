using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Knapcode.FactorioTools.OilField;

public interface ILocationDictionary<T>
{
    int Count { get; }
    IReadOnlyCollection<Location> Keys { get; }
    IReadOnlyCollection<T> Values { get; }
    T this[Location index] { get; set; }

    void Add(Location key, T value);
    void Clear();
    bool ContainsKey(Location key);
    IReadOnlyCollection<KeyValuePair<Location, T>> EnumeratePairs();
    bool Remove(Location key);
    bool TryAdd(Location key, T value);
    bool TryGetValue(Location key, [MaybeNullWhen(false)] out T value);
}
