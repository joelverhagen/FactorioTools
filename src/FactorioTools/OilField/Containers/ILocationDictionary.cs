using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Knapcode.FactorioTools.OilField;

public interface ILocationDictionary<T>
{
    int Count { get; }
    IEnumerable<Location> Keys { get; }
    IEnumerable<T> Values { get; }
    T this[Location index] { get; set; }

    void Add(Location key, T value);
    void Clear();
    bool ContainsKey(Location key);
    IEnumerable<KeyValuePair<Location, T>> EnumeratePairs();
    bool Remove(Location key);
    bool TryAdd(Location key, T value);
    bool TryGetValue(Location key, [MaybeNullWhen(false)] out T value);
}
