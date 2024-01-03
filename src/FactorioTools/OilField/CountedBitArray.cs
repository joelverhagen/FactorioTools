using System;
using System.Collections;

namespace Knapcode.FactorioTools.OilField;

public class CountedBitArray
{
#if USE_BITARRAY
    private readonly BitArray _array;
#else
    private readonly bool[] _array;
#endif

    public CountedBitArray(CountedBitArray bits)
    {
#if USE_BITARRAY
        _array = new BitArray(bits._array);
#else
        _array = new bool[bits.Count];
        Array.Copy(bits._array, _array, bits.Count);
#endif
        TrueCount = bits.TrueCount;
    }

    public CountedBitArray(int length)
    {
#if USE_BITARRAY
        _array = new BitArray(length);
#else
        _array = new bool[length];
#endif
    }

#if USE_BITARRAY
    public int Count => _array.Count;
#else
    public int Count => _array.Length;
#endif

    public bool this[int index]
    {
        get => _array[index];
        set
        {
            var current = _array[index];
            if (current != value)
            {
                TrueCount += value ? 1 : -1;
            }

            _array[index] = value;
        }
    }

    public int TrueCount { get; private set; }

    public bool All(bool value)
    {
        return value ? TrueCount == Count : TrueCount == 0;
    }

    public bool Any(bool value)
    {
        return value ? TrueCount > 0 : TrueCount < Count;
    }

    public void SetAll(bool value)
    {
#if USE_BITARRAY
        _array.SetAll(value);
#else
        Array.Fill(_array, value);
#endif
        TrueCount = value ? Count : 0;
    }

    public CountedBitArray And(CountedBitArray value)
    {
#if USE_BITARRAY
        _array.And(value._array);
#else
        if (value.Count != Count)
        {
            throw new ArgumentException("The two bit arrays must have the same number of elements.");
        }

        for (var i = 0; i < Count; i++)
        {
            _array[i] = _array[i] && value[i];
        }
#endif
        TrueCount = CountTrue();
        return this;
    }

    public CountedBitArray Not()
    {
#if USE_BITARRAY
        _array.Not();
#else
        for (var i = 0; i < Count; i++)
        {
            _array[i] = !_array[i];
        }
#endif
        TrueCount = Count - TrueCount;
        return this;
    }

    public CountedBitArray Or(CountedBitArray value)
    {
#if USE_BITARRAY
        _array.Or(value._array);
#else
        if (value.Count != Count)
        {
            throw new ArgumentException("The two bit arrays must have the same number of elements.");
        }

        for (var i = 0; i < Count; i++)
        {
            _array[i] = _array[i] || value[i];
        }
#endif
        TrueCount = CountTrue();
        return this;
    }

    private int CountTrue()
    {
        var count = 0;

        for (var i = 0; i < _array.Length; i++)
        {
            if (_array[i])
            {
                count++;
            }
        }

        return count;
    }
}
