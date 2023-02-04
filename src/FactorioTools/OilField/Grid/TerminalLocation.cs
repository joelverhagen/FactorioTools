using Knapcode.FactorioTools.OilField.Data;

namespace Knapcode.FactorioTools.OilField.Grid;

internal class TerminalLocation
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
}
