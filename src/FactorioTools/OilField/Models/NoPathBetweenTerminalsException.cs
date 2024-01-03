using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools;

public class NoPathBetweenTerminalsException : FactorioToolsException
{
    public NoPathBetweenTerminalsException(Location a, Location b) : base(
        "One pumpjack terminal has no path to another terminal. This is likely due to the oil field having an area completed surrounded by pumpjacks.",
        badInput: true)
    {
        LocationA = a;
        LocationB = b;
    }

    public Location LocationA { get; }
    public Location LocationB { get; }
}
