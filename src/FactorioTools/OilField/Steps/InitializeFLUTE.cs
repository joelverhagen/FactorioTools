﻿using Knapcode.FluteSharp;

namespace Knapcode.FactorioTools.OilField;

public static class InitializeFLUTE
{
    public static FLUTE? FLUTE { get; private set; }
    private static readonly object FLUTELock = new object();

#if ALLOW_DYNAMIC_FLUTE_DEGREE
    public static void Execute(int lutD)
    {
        var baseType = typeof(Planner);
        var assembly = baseType.Assembly;
        var ns = baseType.Namespace;
        using var powvStream = assembly.GetManifestResourceStream($"{ns}.POWV{lutD}.dat")!;
        using var postStream = assembly.GetManifestResourceStream($"{ns}.POST{lutD}.dat")!;
        Execute(lutD, powvStream, postStream);
    }

    private static void Execute(int lutD, System.IO.Stream powvStream, System.IO.Stream postStream)
    {
        lock (FLUTELock)
        {
            if (FLUTE is not null)
            {
                return;
            }

            var lookUpTable = new LookUpTable(lutD, powvStream, postStream);
            var flute = new FLUTE(lookUpTable);
            FLUTE = flute;
        }
    }
#else
    public static void Execute(int lutD)
    {
        if (lutD != 6)
        {
            throw new System.ArgumentOutOfRangeException(nameof(lutD), "Only lookup table of degree 6 is supported.");
        }

        lock (FLUTELock)
        {
            if (FLUTE is not null)
            {
                return;
            }

            var lookUpTable = LookUpTable.Degree6;
            var flute = new FLUTE(lookUpTable);
            FLUTE = flute;
        }
    }
#endif
}
