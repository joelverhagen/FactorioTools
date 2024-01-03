using System;

namespace Knapcode.FactorioTools.OilField;

public class ExistingPipeGrid : SquareGrid
{
    public ExistingPipeGrid(SquareGrid squareGrid, LocationSet pipes) : base(squareGrid, clone: false)
    {
        Pipes = pipes;
    }

    public LocationSet Pipes { get; }

    public override void GetNeighbors(Span<Location> neighbors, Location id)
    {
        var a = id.Translate(1, 0);
        neighbors[0] = Pipes.Contains(a) ? a : Location.Invalid;

        var b = id.Translate(0, -1);
        neighbors[1] = Pipes.Contains(b) ? b : Location.Invalid;

        var c = id.Translate(-1, 0);
        neighbors[2] = Pipes.Contains(c) ? c : Location.Invalid;

        var d = id.Translate(0, 1);
        neighbors[3] = Pipes.Contains(d) ? d : Location.Invalid;
    }
}
