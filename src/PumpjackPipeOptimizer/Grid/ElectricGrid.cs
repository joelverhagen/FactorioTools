using PumpjackPipeOptimizer.Steps;

namespace PumpjackPipeOptimizer.Grid;

internal class ElectricGrid : SquareGrid
{
    private readonly double _wireReach;

    public ElectricGrid(ElectricGrid grid) : base(grid)
    {
        _wireReach = grid._wireReach;
    }

    public ElectricGrid(SquareGrid grid, double wireReach) : base(grid)
    {
        _wireReach = wireReach;
    }

    public override double GetNeighborCost(Location a, Location b)
    {
        return 1;
    }

    public override IEnumerable<Location> GetNeighbors(Location id)
    {
        var queue = new Queue<Location>();
        var discovered = new HashSet<Location>();
        queue.Enqueue(id);

        while (queue.Count > 0)
        {
            var location = queue.Dequeue();

            foreach (var neighbor in GetAdjacent(location))
            {
                if (discovered.Add(neighbor))
                {
                    if (AddElectricPoles.AreElectricPolesConnected(id, neighbor, _wireReach))
                    {
                        queue.Enqueue(neighbor);

                        if (IsEmpty(neighbor) || IsEntityType<ElectricPole>(neighbor))
                        {
                            yield return neighbor;
                        }
                    }
                }
            }
        }
    }
}
