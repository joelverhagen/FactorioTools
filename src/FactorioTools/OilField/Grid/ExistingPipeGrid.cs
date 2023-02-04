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

    public override IEnumerable<Location> GetNeighbors(Location id)
    {
        foreach (var adjacent in GetAdjacent(id))
        {
            if (IsEntityType<Pipe>(adjacent))
            {
                yield return adjacent;
            }
        }
    }
}
