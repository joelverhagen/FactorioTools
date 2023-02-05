namespace Knapcode.FactorioTools.OilField.Grid;

internal class ExistingPipeGrid : SquareGrid
{
    public ExistingPipeGrid(SquareGrid squareGrid) : base(squareGrid)
    {
    }

    public ExistingPipeGrid(int width, int height) : base(width, height)
    {
    }

    public override double GetNeighborCost(Location a, Location b)
    {
        return 1;
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

            if (!IsEntityType<Pipe>(neighbors[i]))
            {
                neighbors[i] = Location.Invalid;
            }
        }
    }
}
