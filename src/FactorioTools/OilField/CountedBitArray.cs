using System.Collections;

namespace Knapcode.FactorioTools.OilField;

internal class CountedBitArray
{
    private readonly BitArray _array;

    public CountedBitArray(CountedBitArray bits)
    {
        _array = new BitArray(bits._array);
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

    public bool All(bool val)
    {
        return val ? TrueCount == _array.Count : TrueCount == 0;
    }

    public bool Any(bool val)
    {
        return val ? TrueCount > 0 : TrueCount < _array.Count;
    }

    internal CountedBitArray And(CountedBitArray value)
    {
        _array.And(value._array);
        TrueCount = CountTrue();
        return this;
    }

    internal CountedBitArray Not()
    {
        _array.Not();
        TrueCount = Count - TrueCount;
        return this;
    }

    internal CountedBitArray Or(CountedBitArray value)
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
