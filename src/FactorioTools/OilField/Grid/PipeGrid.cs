namespace Knapcode.FactorioTools.OilField.Grid;

public class PipeGrid : SquareGrid
{
    public PipeGrid(SquareGrid existing) : base(existing, clone: true)
    {
    }

    public PipeGrid(int width, int height) : base(width, height)
    {
    }

    public override void GetNeighbors(Span<Location> neighbors, Location id)
    {
        var a = id.Translate(1, 0);
        neighbors[0] = IsInBounds(a) && IsEmpty(a) ? a : Location.Invalid;

        var b = id.Translate(0, -1);
        neighbors[1] = IsInBounds(b) && IsEmpty(b) ? b : Location.Invalid;

        var c = id.Translate(-1, 0);
        neighbors[2] = IsInBounds(c) && IsEmpty(c) ? c : Location.Invalid;

        var d = id.Translate(0, 1);
        neighbors[3] = IsInBounds(d) && IsEmpty(d) ? d : Location.Invalid;
    }
}
