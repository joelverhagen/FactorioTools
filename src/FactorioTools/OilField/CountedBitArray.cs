using System.Collections;

namespace Knapcode.FactorioTools.OilField;

public class CountedBitArray
{
    private readonly BitArray _array;

    public CountedBitArray(CountedBitArray bits)
    {
        _array = new BitArray(bits._array);
        TrueCount = bits.TrueCount;
    }

    public CountedBitArray(int length)
    {
        _array = new BitArray(length);
    }

    public int Count => _array.Count;

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
        return value ? TrueCount == _array.Count : TrueCount == 0;
    }

    public bool Any(bool value)
    {
        return value ? TrueCount > 0 : TrueCount < _array.Count;
    }

    public void SetAll(bool value)
    {
        _array.SetAll(value);
        TrueCount = value ? _array.Count : 0;
    }

    public CountedBitArray And(CountedBitArray value)
    {
        _array.And(value._array);
        TrueCount = CountTrue();
        return this;
    }

    public CountedBitArray Not()
    {
        _array.Not();
        TrueCount = Count - TrueCount;
        return this;
    }

    public CountedBitArray Or(CountedBitArray value)
    {
        _array.Or(value._array);
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
