using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField.Grid;

public class TerminalLocation
{
    public TerminalLocation(Location center, Location terminal, Direction direction)
    {
        Center = center;
        Terminal = terminal;
        Direction = direction;
    }

    public Location Center { get; set; }
    public Location Terminal { get; }
    public Direction Direction { get; }

#if ENABLE_VISUALIZER
    public override string ToString()
    {
        return $"Pump {Center:M} {Direction} terminal ({Terminal:M})";
    }
#endif
}
