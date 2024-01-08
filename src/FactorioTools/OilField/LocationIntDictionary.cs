using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Knapcode.FactorioTools.OilField;

public class LocationIntDictionary<T> : ILocationDictionary<T>
{
    private readonly int _width;
    private readonly Dictionary<int, T> _dictionary;

    public LocationIntDictionary(int width)
    {
        _width = width;
        _dictionary = new Dictionary<int, T>();
    }

    public LocationIntDictionary(int width, int capacity)
    {
        _width = width;
        _dictionary = new Dictionary<int, T>(capacity);
    }

    public T this[Location index]
    {
        get => _dictionary[GetIndex(index)];
        set => _dictionary[GetIndex(index)] = value;
    }

    public int Count => _dictionary.Count;

    public IEnumerable<Location> Keys
    {
        get
        {
            foreach (var item in _dictionary.Keys)
            {
                yield return new Location(item % _width, item / _width);
            }
        }
    }

    public IEnumerable<T> Values => _dictionary.Values;


    public void Add(Location key, T value)
    {
        _dictionary.Add(GetIndex(key), value);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool ContainsKey(Location key)
    {
        return _dictionary.ContainsKey(GetIndex(key));
    }

    public IEnumerable<KeyValuePair<Location, T>> EnumeratePairs()
    {
        foreach (var pair in _dictionary)
        {
            yield return new KeyValuePair<Location, T>(new Location(pair.Key % _width, pair.Key / _width), pair.Value);
        }
    }

    public bool Remove(Location key)
    {
        return _dictionary.Remove(GetIndex(key));
    }

    public bool TryAdd(Location key, T value)
    {
        return _dictionary.TryAdd(GetIndex(key), value);
    }

    public bool TryGetValue(Location key, [MaybeNullWhen(false)] out T value)
    {
        return _dictionary.TryGetValue(GetIndex(key), out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(Location location)
    {
        return location.Y * _width + location.X;
    }
}
