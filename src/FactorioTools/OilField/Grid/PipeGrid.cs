namespace Knapcode.FactorioTools.OilField.Grid;

internal class PipeGrid : SquareGrid
{
    public PipeGrid(int width, int height) : base(width, height)
    {
    }

    public PipeGrid(SquareGrid squareGrid) : base(squareGrid)
    {
    }

    public override void GetNeighbors(Span<Location> neighbors, Location id)
    {
        GetAdjacent(neighbors, id);
        for (var i = 0; i < neighbors.Length; i++)
        {
            if (!neighbors[i].IsValid)
            {
                continue;
            }

            if (!IsEmpty(neighbors[i]))
            {
                neighbors[i] = Location.Invalid;
            }
        }
    }
}
