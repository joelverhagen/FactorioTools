using PumpjackPipeOptimizer.Steps;

namespace PumpjackPipeOptimizer.Grid;

internal class ElectricGrid : SquareGrid
{
    private readonly Options _options;

    public ElectricGrid(ElectricGrid grid) : base(grid)
    {
        _options = grid._options;
    }

    public ElectricGrid(SquareGrid grid, Options options) : base(grid)
    {
        _options = options;
    }

    public override double GetNeighborCost(Location a, Location b)
    {
        return 1;
    }

    public override IEnumerable<Location> GetNeighbors(Location id)
    {
        var reachCeiling = (int)Math.Ceiling(_options.ElectricPoleWireReach);

        var neighbors = new HashSet<Location>();

        for (var x = -1 * reachCeiling; x <= reachCeiling; x++)
        {
            for (var y = -1 * reachCeiling; y <= reachCeiling; y++)
            {
                var candidate = id.Translate((x, y));

                if (!AddElectricPoles.AreElectricPolesConnected(id, candidate, _options))
                {
                    continue;
                }

                if (IsEmpty(candidate))
                {
                    (var fits, _) = AddElectricPoles.GetElectricPoleLocations(this, _options, candidate, populateSides: false);
                    if (fits && neighbors.Add(candidate))
                    {
                        yield return candidate;
                    }
                }
                else if (IsEntityType<ElectricPoleCenter>(candidate))
                {
                    if (neighbors.Add(candidate))
                    {
                        yield return candidate;
                    }
                }
                /*
                else if (IsEntityType<ElectricPoleSide>(candidate))
                {
                    var side = (ElectricPoleSide)LocationToEntity[candidate];
                    var center = EntityToLocation[side.Center];
                    if (neighbors.Add(center))
                    {
                        yield return center;
                    }
                }
                */
            }
        }
    }
}
