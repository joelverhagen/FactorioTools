using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Knapcode.FactorioTools.OilField;

public class LocationHashDictionary<T> : ILocationDictionary<T>
{
    private readonly Dictionary<Location, T> _dictionary;

    public LocationHashDictionary()
    {
        _dictionary = new Dictionary<Location, T>();
    }

    public LocationHashDictionary(int capacity)
    {
        _dictionary = new Dictionary<Location, T>(capacity);
    }

    public T this[Location index]
    {
        get => _dictionary[index];
        set => _dictionary[index] = value;
    }

    public int Count => _dictionary.Count;
    public IEnumerable<Location> Keys => _dictionary.Keys;
    public IEnumerable<T> Values => _dictionary.Values;

    public void Add(Location key, T value)
    {
        _dictionary.Add(key, value);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool ContainsKey(Location key)
    {
        return _dictionary.ContainsKey(key);
    }

    public IEnumerable<KeyValuePair<Location, T>> EnumeratePairs()
    {
        return _dictionary;
    }

    public bool Remove(Location key)
    {
        return _dictionary.Remove(key);
    }

    public bool TryAdd(Location key, T value)
    {
        return _dictionary.TryAdd(key, value);
    }

    public bool TryGetValue(Location key, [MaybeNullWhen(false)] out T value)
    {
        return _dictionary.TryGetValue(key, out value);
    }
}
