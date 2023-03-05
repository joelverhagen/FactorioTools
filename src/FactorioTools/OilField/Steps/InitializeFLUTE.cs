using Knapcode.FluteSharp;

namespace Knapcode.FactorioTools.OilField.Steps;

public static class InitializeFLUTE
{
    public static FLUTE? FLUTE { get; private set; }
    private static readonly object FLUTELock = new object();

    public static void Execute(int lutD)
    {
        var baseType = typeof(Planner);
        var assembly = baseType.Assembly;
        var ns = baseType.Namespace;
        using var powvStream = assembly.GetManifestResourceStream($"{ns}.POWV{lutD}.dat")!;
        using var postStream = assembly.GetManifestResourceStream($"{ns}.POST{lutD}.dat")!;
        Execute(lutD, powvStream, postStream);
    }

    public static void Execute(int lutD, Stream powvStream, Stream postStream)
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
}
