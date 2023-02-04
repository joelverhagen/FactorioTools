namespace Knapcode.FactorioTools.OilField.Grid;

internal class PipeGrid : SquareGrid
{
    public PipeGrid(int width, int height) : base(width, height)
    {
    }

    public PipeGrid(SquareGrid squareGrid) : base(squareGrid)
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
            if (IsEmpty(adjacent))
            {
                yield return adjacent;
            }
        }
    }
}
