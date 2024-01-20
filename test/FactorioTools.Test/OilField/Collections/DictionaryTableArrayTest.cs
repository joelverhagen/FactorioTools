namespace Knapcode.FactorioTools.OilField;

public class DictionaryTableArrayTest
{
    [Fact]
    public void RemoveRange_Beginning()
    {
        var target = new DictionaryTableList<int>();
        target.AddCollection(Enumerable.Range(0, 8).ToList());

        target.RemoveRange(0, 5);

        Assert.Equal(new[] { 5, 6, 7 }, target.ToArray());
    }

    [Fact]
    public void RemoveRange_Middle()
    {
        var target = new DictionaryTableList<int>();
        target.AddCollection(Enumerable.Range(0, 8).ToList());

        target.RemoveRange(2, 3);

        Assert.Equal(new[] { 0, 1, 5, 6, 7 }, target.ToArray());
    }

    [Fact]
    public void RemoveRange_End()
    {
        var target = new DictionaryTableList<int>();
        target.AddCollection(Enumerable.Range(0, 8).ToList());

        target.RemoveRange(4, 4);

        Assert.Equal(new[] { 0, 1, 2, 3 }, target.ToArray());
    }

    [Fact]
    public void RemoveRange_All()
    {
        var target = new DictionaryTableList<int>();
        target.AddCollection(Enumerable.Range(0, 8).ToList());

        target.RemoveRange(0, 8);

        Assert.Empty(target.ToArray());
    }

    [Fact]
    public void RemoveAt_Beginning()
    {
        var target = new DictionaryTableList<int>();
        target.AddCollection(Enumerable.Range(0, 8).ToList());

        target.RemoveAt(0);

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7 }, target.ToArray());
    }

    [Fact]
    public void RemoveAt_Middle()
    {
        var target = new DictionaryTableList<int>();
        target.AddCollection(Enumerable.Range(0, 8).ToList());

        target.RemoveAt(3);

        Assert.Equal(new[] { 0, 1, 2, 4, 5, 6, 7 }, target.ToArray());
    }

    [Fact]
    public void RemoveAt_End()
    {
        var target = new DictionaryTableList<int>();
        target.AddCollection(Enumerable.Range(0, 8).ToList());

        target.RemoveAt(7);

        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 6 }, target.ToArray());
    }
}
