#if USE_BITARRAY
using System.Collections;
using System.Linq;

namespace Knapcode.FactorioTools.OilField;

public class WrapperCountedBitArray
{
    private readonly BitArray _array;

    public WrapperCountedBitArray(WrapperCountedBitArray bits)
    {
        _array = new BitArray(bits._array);
        TrueCount = bits.TrueCount;
    }

    public WrapperCountedBitArray(int length)
    {
        _array = new BitArray(length);
    }

    public int Count => _array.Count;

    public bool this[int index]
    {
        get => _array[index];
        set => Set(index, value);
    }

    public bool Set(int index, bool value)
    {
        var current = _array[index];
        if (current != value)
        {
            TrueCount += value ? 1 : -1;
        }
        _array[index] = value;
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
        _array.SetAll(value);
        TrueCount = value ? Count : 0;
    }

    public WrapperCountedBitArray And(WrapperCountedBitArray value)
    {
        _array.And(value._array);
        TrueCount = CountTrue();
        return this;
    }

    public WrapperCountedBitArray Not()
    {
        _array.Not();
        TrueCount = Count - TrueCount;
        return this;
    }

    public WrapperCountedBitArray Or(WrapperCountedBitArray value)
    {
        _array.Or(value._array);
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
#endif