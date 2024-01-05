using System;
using System.Collections;
using System.Linq;

namespace Knapcode.FactorioTools.OilField;

public class CountedBitArray
{
#if USE_BITARRAY
    private readonly BitArray _array;
#else
    private readonly int[] _array;
#endif

    public CountedBitArray(CountedBitArray bits)
    {
#if USE_BITARRAY
        _array = new BitArray(bits._array);
#else
        Count = bits.Count;
        _array = new int[bits._array.Length];
        Array.Copy(bits._array, _array, bits._array.Length);
#endif
        TrueCount = bits.TrueCount;
    }

    public CountedBitArray(int length)
    {
#if USE_BITARRAY
        _array = new BitArray(length);
#else
        Count = length;
        var intLength = length / 32;
        if (length % 32 != 0)
        {
            intLength++;
        }
        _array = new int[intLength];
#endif
    }

#if USE_BITARRAY
    public int Count => _array.Count;
#else
    public int Count { get; }
#endif

    public bool this[int index]
    {
        get
        {
            var intIndex = index / 32;
            var bitIndex = index % 32;
            return (_array[intIndex] & (1 << bitIndex)) != 0;
        }
        set
        {
            var intIndex = index / 32;
            var bitIndex = index % 32;
            var currentInt = _array[intIndex];
            var mask = 1 << bitIndex;
            var current = (currentInt & mask) != 0;
            if (current != value)
            {
                if (value)
                {
                    _array[intIndex] = currentInt | mask;
                    TrueCount++;
                }
                else
                {
                    _array[intIndex] = currentInt & ~mask;
                    TrueCount--;
                }
            }
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
        Array.Fill(_array, value ? -1 : 0);
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

        for (var i = 0; i < _array.Length; i++)
        {
            _array[i] = _array[i] & value._array[i];
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
        for (var i = 0; i < _array.Length; i++)
        {
            _array[i] = ~_array[i];
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

        for (var i = 0; i < _array.Length; i++)
        {
            _array[i] = _array[i] | value._array[i];
        }
#endif
        TrueCount = CountTrue();
        return this;
    }

    private int CountTrue()
    {
        var count = 0;

        for (var i = 0; i < Count; i++)
        {
            if (this[i])
            {
                count++;
            }
        }

        return count;
    }

#if ENABLE_GRID_TOSTRING
    public override string ToString()
    {
        return string.Join("", Enumerable.Range(0, Count).Select(i => this[i] ? '1' : '0'));
    }
#endif
}
