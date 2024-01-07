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
