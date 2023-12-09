namespace Knapcode.FactorioTools;

public class NoPathBetweenTerminalsException : FactorioToolsException
{
    public NoPathBetweenTerminalsException() : base(
        "One pumpjack terminal has no path to another terminal. This is likely due to the oil field having an area completed surrounded by pumpjacks.",
        badInput: true)
    {
    }
}
