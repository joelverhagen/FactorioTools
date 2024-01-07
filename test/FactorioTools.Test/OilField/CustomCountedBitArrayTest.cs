using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knapcode.FactorioTools.OilField;

public class CustomCountedBitArrayTest
{
    [Fact]
    public void ClearsUpperBitsInSetAll()
    {
        var bits = new CustomCountedBitArray(100);
        bits.SetAll(true);
        for (var i = 0; i < bits.Count; i++)
        {
            bits[i] = false;
        }

        Assert.Equal(0, bits.GetInt(3));
    }

    [Fact]
    public void ClearsUpperBitsInNot()
    {
        var bits = new CustomCountedBitArray(100);
        bits.Not();
        for (var i = 0; i < bits.Count; i++)
        {
            bits[i] = false;
        }

        Assert.Equal(0, bits.GetInt(3));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(65)]
    [InlineData(100)]
    public void ClearsOnlyUpperUnusedBits(int count)
    {
        var a = new CustomCountedBitArray(count);
        for (var i = 0; i < a.Count; i++)
        {
            a[i] = true;
        }

        var b = new CustomCountedBitArray(count);
        b.Not();

        var lastIntIndex = count / 32;
        if (count % 32 == 0)
        {
            lastIntIndex--;
        }
        Assert.Equal(b.GetInt(lastIntIndex), a.GetInt(lastIntIndex));
    }

    [Fact]
    public void LogicalEquals_NotEqual()
    {
        var a = new CustomCountedBitArray(100);
        a[50] = true;
        var b = new CustomCountedBitArray(100);
        b.Set(49, true);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void LogicalEquals_DifferentCount()
    {
        var a = new CustomCountedBitArray(100);
        a[50] = true;
        var b = new CustomCountedBitArray(99);
        b.Set(50, true);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void LogicalEquals_Equal()
    {
        var a = new CustomCountedBitArray(100);
        a[50] = true;
        var b = new CustomCountedBitArray(100);
        b.Set(50, true);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void LogicalEquals_Same()
    {
        var a = new CustomCountedBitArray(100);
        Assert.True(a.Equals(a));
    }
}
