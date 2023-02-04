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

    public override List<Location> GetNeighbors(Location id)
    {
        var adjacent = GetAdjacent(id);
        for (var i = 0; i < adjacent.Count; i++)
        {
            if (!IsEmpty(adjacent[i]))
            {
                adjacent.RemoveAt(i);
                i--;
            }
        }

        return adjacent;
    }
}
