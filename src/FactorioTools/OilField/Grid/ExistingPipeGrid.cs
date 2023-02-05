namespace Knapcode.FactorioTools.OilField.Grid;

internal class ExistingPipeGrid : SquareGrid
{
    public ExistingPipeGrid(SquareGrid squareGrid) : base(squareGrid)
    {
    }

    public ExistingPipeGrid(int width, int height) : base(width, height)
    {
    }

    public override void GetNeighbors(Span<Location> neighbors, Location id)
    {
        var a = id.Translate((1, 0));
        neighbors[0] = IsInBounds(a) && IsEntityType<Pipe>(a) ? a : Location.Invalid;

        var b = id.Translate((0, -1));
        neighbors[1] = IsInBounds(b) && IsEntityType<Pipe>(b) ? b : Location.Invalid;

        var c = id.Translate((-1, 0));
        neighbors[2] = IsInBounds(c) && IsEntityType<Pipe>(c) ? c : Location.Invalid;

        var d = id.Translate((0, 1));
        neighbors[3] = IsInBounds(d) && IsEntityType<Pipe>(d) ? d : Location.Invalid;
    }
}
