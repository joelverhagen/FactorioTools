using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class DictionaryTableList<T> : ITableList<T>
{
    private readonly Dictionary<int, T> _dictionary;

    public DictionaryTableList()
    {
        _dictionary = new Dictionary<int, T>();
    }

    public DictionaryTableList(int capacity)
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

    public void AddCollection(IReadOnlyCollection<T> collection)
    {
        foreach (var item in collection)
        {
            _dictionary.Add(_dictionary.Count, item);
        }
    }

    public void AddRange(IReadOnlyTableList<T> collection)
    {
        var other = (DictionaryTableList<T>)collection;
        for (var i = 0; i < other.Count; i++)
        {
            _dictionary.Add(_dictionary.Count, other[i]);
        }
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool Contains(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        foreach (var value in _dictionary.Values)
        {
            if (comparer.Equals(item, value))
            {
                return true;
            }
        }

        return false;
    }

    public IReadOnlyCollection<T> EnumerateItems()
    {
        return _dictionary.Values;
    }

    public bool Remove(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        var index = -1;
        foreach (var (key, value) in _dictionary)
        {
            if (comparer.Equals(item, value))
            {
                index = key;
                break;
            }
        }

        if (index == -1)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        RemoveRange(index, 1);
    }

    public void RemoveRange(int index, int count)
    {
        var originalCount = _dictionary.Count;
        if (index + count > originalCount)
        {
            throw new IndexOutOfRangeException();
        }

        var startIndex = index + count;
        for (var i = startIndex; i < originalCount; i++)
        {
            _dictionary[i - count] = _dictionary[i];
            _dictionary.Remove(i);
        }

        for (var i = originalCount - count; i < startIndex; i++)
        {
            _dictionary.Remove(i);
        }
    }

    public void Reverse()
    {
        var count = _dictionary.Count;
        for (var i = 0; i < count / 2; i++)
        {
            var temp = _dictionary[i];
            var otherIndex = count - 1 - i;
            _dictionary[i] = _dictionary[otherIndex];
            _dictionary[otherIndex] = temp;
        }
    }

    public void Sort(Comparison<T> comparison)
    {
        var keys = new int[_dictionary.Count];
        var values = new T[_dictionary.Count];
        for (var i = 0; i < keys.Length; i++)
        {
            keys[i] = i;
            values[i] = _dictionary[i];
        }

        Array.Sort(keys, (a, b) => comparison(this[a], this[b]));

        for (var i = 0; i < keys.Length; i++)
        {
            _dictionary[i] = values[keys[i]];
        }
    }

    public void SortRange(int index, int count, IComparer<T> comparer)
    {
        var keys = new int[_dictionary.Count];
        var values = new T[_dictionary.Count];
        for (var i = 0; i < keys.Length; i++)
        {
            keys[i] = i;
            values[i] = _dictionary[i];
        }

        var keyComparer = Comparer<int>.Create((a, b) => comparer.Compare(this[a], this[b]));
        Array.Sort(keys, index, count, keyComparer);

        for (var i = 0; i < keys.Length; i++)
        {
            _dictionary[i] = values[keys[i]];
        }
    }
}
