using System;
using System.Collections;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public static class Collection
{
    public static IItemList<T> List<T>()
    {
#if USE_ARRAY
        return new StandardList<T>();
#else
        return new DictionaryList<T>();
#endif
    }

    public static IItemList<T> List<T>(int capacity)
    {
#if USE_ARRAY
        return new StandardList<T>(capacity);
#else
        return new DictionaryList<T>(capacity);
#endif
    }
}

public interface IItemList<T>
{
    T this[int index] { get; set; }
    int Count { get; }

    void Add(T item);
    void AddRange(IReadOnlyCollection<T> collection);
    void Sort(Comparison<T> comparison);
}

public class StandardList<T> : IItemList<T>
{
    private readonly List<T> _list;

    public StandardList()
    {
        _list = new List<T>();
    }

    public StandardList(int capacity)
    {
        _list = new List<T>(capacity);
    }

    public T this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public int Count => _list.Count;

    public void Add(T item)
    {
        _list.Add(item);
    }

    public void AddRange(IReadOnlyCollection<T> collection)
    {
        _list.AddRange(collection);
    }

    public void Sort(Comparison<T> comparison)
    {
        _list.Sort(comparison);
    }
}

public class DictionaryList<T> : IItemList<T>
{
    private readonly Dictionary<int, T> _dictionary;

    public DictionaryList()
    {
        _dictionary = new Dictionary<int, T>();
    }

    public DictionaryList(int capacity)
    {
        _dictionary = new Dictionary<int, T>(capacity);
    }

    public T this[int index]
    {
        get
        {
            if (index >= _dictionary.Count)
            {
                throw new IndexOutOfRangeException();
            }

            return _dictionary[index];
        }
        set
        {
            if (index >= _dictionary.Count)
            {
                throw new IndexOutOfRangeException();
            }

            _dictionary[index] = value;
        }
    }

    public int Count => _dictionary.Count;

    public void Add(T item)
    {
        _dictionary.Add(_dictionary.Count, item);
    }

    public void AddRange(IReadOnlyCollection<T> collection)
    {
        foreach (var item in collection)
        {
            _dictionary.Add(_dictionary.Count, item);
        }
    }

    public void Sort(Comparison<T> comparison)
    {
        var keys = new int[Count];
        for (var i = 0; i < keys.Length; i++)
        {
            keys[i] = i;
        }

        Array.Sort(keys, (a, b) => comparison(this[a], this[b]));

        for (var i = 0; i < keys.Length; i++)
        {
            if (keys[i] != i)
            {
                var temp = _dictionary[i];
                _dictionary[i] = _dictionary[keys[i]];
                _dictionary[keys[i]] = temp;
                keys[i] = i;
            }
        }
    }
}
