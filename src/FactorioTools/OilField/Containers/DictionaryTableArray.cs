using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public static class TableArray
{
    public static DictionaryTableArray<T> New<T>(int length)
    {
        return new DictionaryTableArray<T>(length);
    }
}

public class DictionaryTableArray<T>
{
    private readonly Dictionary<int, T> _dictionary;
    private readonly EqualityComparer<T> _comparer;

    public DictionaryTableArray(int length)
    {
        _dictionary = new Dictionary<int, T>(length);
        _comparer = EqualityComparer<T>.Default;
        Length = length;
    }

    public int Length { get; private set; }

    public void Resize(int length)
    {
        if (length < Length)
        {
            _dictionary.Clear();
        }

        Length = length;
    }

    public T? this[int index]
    {
        get
        {
            if (index >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            if (!_dictionary.TryGetValue(index, out var value))
            {
                return default;
            }

            return value;
        }
        set
        {
            if (index >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            if (_comparer.Equals(value, default))
            {
                _dictionary.Remove(index);
            }
            else
            {
                _dictionary[index] = value!;
            }
        }
    }

    public IReadOnlyCollection<T> EnumerateItems()
    {
        return _dictionary.Values;
    }
}
