using System;
using System.Linq;

namespace Knapcode.FactorioTools.OilField;

public class CustomCountedBitArray
{
    private readonly int[] _array;

    public CustomCountedBitArray(CustomCountedBitArray bits)
    {
        Count = bits.Count;
        _array = new int[bits._array.Length];
        Array.Copy(bits._array, _array, bits._array.Length);
        TrueCount = bits.TrueCount;
    }

    public CustomCountedBitArray(int length)
    {
        Count = length;
        var intLength = length / 32;
        if (length % 32 != 0)
        {
            intLength++;
        }
        _array = new int[intLength];
    }

    public int Count { get; }

    public bool this[int index]
    {
        get
        {
            var intIndex = index / 32;
            var bitIndex = index % 32;
            return (_array[intIndex] & (1 << bitIndex)) != 0;
        }
        set => Set(index, value);
    }

    public bool Set(int index, bool value)
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
        return current;
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
        Array.Fill(_array, value ? -1 : 0);
        TrueCount = value ? Count : 0;
    }

    public CustomCountedBitArray And(CustomCountedBitArray value)
    {
        if (value.Count != Count)
        {
            throw new ArgumentException("The two bit arrays must have the same number of elements.");
        }

        for (var i = 0; i < _array.Length; i++)
        {
            _array[i] = _array[i] & value._array[i];
        }
        TrueCount = CountTrue();
        return this;
    }

    public CustomCountedBitArray Not()
    {
        for (var i = 0; i < _array.Length; i++)
        {
            _array[i] = ~_array[i];
        }
        TrueCount = Count - TrueCount;
        return this;
    }

    public CustomCountedBitArray Or(CustomCountedBitArray value)
    {
        if (value.Count != Count)
        {
            throw new ArgumentException("The two bit arrays must have the same number of elements.");
        }

        for (var i = 0; i < _array.Length; i++)
        {
            _array[i] = _array[i] | value._array[i];
        }
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
