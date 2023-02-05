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

    public override List<Location> GetNeighbors(Location id)
    {
        var adjacent = GetAdjacent(id);
        for (var i = 0; i < adjacent.Count; i++)
        {
            if (!IsEntityType<Pipe>(adjacent[i]))
            {
                adjacent.RemoveAt(i);
                i--;
            }
        }

        return adjacent;
    }
}
