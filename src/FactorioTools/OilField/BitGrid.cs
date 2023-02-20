using System.Collections;

namespace Knapcode.FactorioTools.OilField;

internal class BitGrid : ICollection<Location>
{
    private readonly int _width;
    private readonly int _height;
    private readonly CountedBitArray _bitArray;

    public BitGrid(int width, int height)
    {
        _width = width;
        _height = height;
        _bitArray = new CountedBitArray(width * height);
    }

    public int Width => _width;
    public int Height => _height;

    public int Count => _bitArray.TrueCount;
    public bool IsReadOnly => false;

    public void Clear()
    {
        _bitArray.SetAll(false);
    }

    public bool Add(Location id)
    {
        var existing = this[id];
        this[id] = true;
        return !existing;
    }

    public bool Contains(Location id)
    {
        return this[id];
    }

    void ICollection<Location>.Add(Location id)
    {
        this[id] = true;
    }

    public void CopyTo(Location[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(Location id)
    {
        var existing = this[id];
        this[id] = false;
        return existing;
    }

    public IEnumerator<Location> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool this[Location id]
    {
        get => _bitArray[_width * id.Y + id.X];
        set => _bitArray[_width * id.Y + id.X] = value;
    }
}
