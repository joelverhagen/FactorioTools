namespace Knapcode.FactorioTools.OilField.Grid;

internal class ExistingPipeGrid : SquareGrid
{
    private readonly HashSet<Location> _pipes;

    public ExistingPipeGrid(SquareGrid squareGrid, HashSet<Location> pipes) : base(squareGrid)
    {
        _pipes = pipes;
    }

    public override void GetNeighbors(Span<Location> neighbors, Location id)
    {
        var a = id.Translate((1, 0));
        neighbors[0] = _pipes.Contains(a) ? a : Location.Invalid;

        var b = id.Translate((0, -1));
        neighbors[1] = _pipes.Contains(b) ? b : Location.Invalid;

        var c = id.Translate((-1, 0));
        neighbors[2] = _pipes.Contains(c) ? c : Location.Invalid;

        var d = id.Translate((0, 1));
        neighbors[3] = _pipes.Contains(d) ? d : Location.Invalid;
    }
}
