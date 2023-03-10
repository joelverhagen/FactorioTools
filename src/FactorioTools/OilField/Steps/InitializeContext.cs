using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

public static class InitializeContext
{
    public static Context Execute(OilFieldOptions options, BlueprintRoot root)
    {
        // Translate the blueprint by the minimum X and Y. Leave three spaces on both lesser (left for X, top for Y) sides to cover:
        //   - The side of the pumpjack. It is a 3x3 entity and the position of the entity is the center.
        //   - A spot for a pipe, if needed.
        //   - A spot for an electric pole, if needed.
        var marginX = 1 + 1 + options.ElectricPoleWidth;
        var marginY = 1 + 1 + options.ElectricPoleHeight;

        if (options.AddBeacons)
        {
            marginX += options.BeaconSupplyWidth + (options.BeaconWidth / 2);
            marginY += options.BeaconSupplyHeight + (options.BeaconHeight / 2);
        }

        return Execute(options, root, marginX, marginY);
    }

    public static Context GetEmpty(OilFieldOptions options, int width, int height)
    {
        var root = new BlueprintRoot
        {
            Blueprint = new Blueprint
            {
                Entities = Array.Empty<Entity>(),
                Icons = new[]
                {
                    new Icon
                    {
                        Index = 1,
                        Signal = new SignalID
                        {
                            Name = EntityNames.Vanilla.Pumpjack,
                            Type = SignalTypes.Vanilla.Item,
                        },
                    },
                },
                Item = ItemNames.Vanilla.Blueprint,
                Version = 1,
            },
        };

        return Execute(options, root, width, height);
    }

    private static Context Execute(OilFieldOptions options, BlueprintRoot root, int marginX, int marginY)
    {
        var centers = GetPumpjackCenters(root, marginX, marginY);
        var grid = InitializeGrid(centers, marginX, marginY);
        var centerToTerminals = GetCenterToTerminals(grid, centers);

#if USE_SHARED_INSTANCES
        centers.Clear();
#endif

        return new Context
        {
            Options = options,
            InputBlueprint = root,
            Grid = grid,
            CenterToTerminals = centerToTerminals,
            LocationToTerminals = GetLocationToTerminals(centerToTerminals),
            LocationToAdjacentCount = GetLocationToAdjacentCount(grid),

            SharedInstances = new SharedInstances
            {
#if USE_SHARED_INSTANCES
                LocationQueue = new Queue<Location>(),
                LocationArray = Array.Empty<Location>(),
                IntArrayX = Array.Empty<int>(),
                IntArrayY = Array.Empty<int>(),
                LocationToLocation = new Dictionary<Location, Location>(),
                LocationToDouble = new Dictionary<Location, double>(),
                LocationPriorityQueue = new PriorityQueue<Location, double>(),
                LocationListA = new List<Location>(),
                LocationListB = new List<Location>(),
                LocationSetA = centers,
                LocationSetB = new HashSet<Location>(),
#endif
            },
        };
    }

    private static int[,] GetLocationToAdjacentCount(SquareGrid grid)
    {
        var locationToHasAdjacentPumpjack = new int[grid.Width, grid.Height];
        Span<Location> adjacent = stackalloc Location[4];

        foreach ((var entity, var location) in grid.EntityToLocation)
        {
            if (entity is not PumpjackSide)
            {
                continue;
            }

            grid.GetAdjacent(adjacent, location);
            for (var i = 0; i < adjacent.Length; i++)
            {
                if (!adjacent[i].IsValid)
                {
                    continue;
                }

                locationToHasAdjacentPumpjack[adjacent[i].X, adjacent[i].Y]++;
            }
        }

        return locationToHasAdjacentPumpjack;
    }

    private static HashSet<Location> GetPumpjackCenters(BlueprintRoot root, int marginX, int marginY)
    {
        var pumpjacks = root
            .Blueprint
            .Entities
            .Where(e => e.Name == EntityNames.Vanilla.Pumpjack)
            .ToList();

        const int maxPumpjacks = 100;
        if (pumpjacks.Count > 100)
        {
            throw new FactorioToolsException($"Having more than {maxPumpjacks} pumpjacks is not supported. There are {pumpjacks.Count} pumpjacks provided.");
        }

        var centers = new HashSet<Location>();

        if (pumpjacks.Count > 0)
        {
            var deltaX = 0 - pumpjacks.Min(x => x.Position.X) + marginX;
            var deltaY = 0 - pumpjacks.Min(x => x.Position.Y) + marginY;
            foreach (var entity in pumpjacks)
            {
                var x = entity.Position.X + deltaX;
                var y = entity.Position.Y + deltaY;

                if (IsInteger(x))
                {
                    throw new FactorioToolsException($"Entity {entity.EntityNumber} (a '{entity.Name}') does not have an integer X value after translation.");
                }

                if (IsInteger(y))
                {
                    throw new FactorioToolsException($"Entity {entity.EntityNumber} (a '{entity.Name}') does not have an integer Y value after translation.");
                }

                centers.Add(new Location(ToInt(x), ToInt(y)));
            }
        }

        return centers;
    }

    private static int ToInt(float x)
    {
        return (int)Math.Round(x, 0);
    }

    private static bool IsInteger(float value)
    {
        return Math.Abs(value % 1) > float.Epsilon * 100;
    }

    private static SquareGrid InitializeGrid(IReadOnlySet<Location> pumpjackCenters, int marginX, int marginY)
    {
        // Make a grid to contain game state. Similar to the above, we add extra spots for the pumpjacks, pipes, and
        // electric poles.
        var width = pumpjackCenters.Select(p => p.X).DefaultIfEmpty(-1).Max() + 1 + marginX;
        var height = pumpjackCenters.Select(p => p.Y).DefaultIfEmpty(-1).Max() + 1 + marginY;

        const int maxWidth = 300;
        const int maxHeight = 300;
        if (width > maxWidth || height > maxHeight)
        {
            throw new FactorioToolsException($"The planning grid cannot be larger than {maxWidth} x {maxHeight}. The planning grid for the provided options is {width} x {height}.");
        }

        SquareGrid grid = new PipeGrid(width, height);

        // Fill the grid with the pumpjacks
        foreach (var center in pumpjackCenters)
        {
            AddPumpjack(grid, center);
        }

        return grid;
    }
}
