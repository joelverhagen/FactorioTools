using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class InitializeContext
{
    public static Context Execute(Options options, BlueprintRoot root)
    {
        // Translate the blueprint by the minimum X and Y. Leave three spaces on both lesser (left for X, top for Y) sides to cover:
        //   - The side of the pumpjack. It is a 3x3 entity and the position of the entity is the center.
        //   - A spot for a pipe, if needed.
        //   - A spot for an electric pole, if needed.
        var marginX = 1 + 1 + options.ElectricPoleWidth;
        var marginY = 1 + 1 + options.ElectricPoleHeight;

        return Execute(options, root, marginX, marginY);
    }

    public static Context Execute(Options options, BlueprintRoot root, int marginX, int marginY)
    {
        var centers = GetPumpjackCenters(root, marginX, marginY);
        var grid = InitializeGrid(centers, marginX, marginY);
        var centerToTerminals = GetCenterToTerminals(centers, grid);

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

    private static Dictionary<Location, List<TerminalLocation>> GetLocationToTerminals(Dictionary<Location, List<TerminalLocation>> centerToTerminals)
    {
        var locationToTerminals = new Dictionary<Location, List<TerminalLocation>>();
        foreach (var terminals in centerToTerminals.Values)
        {
            foreach (var terminal in terminals)
            {
                if (!locationToTerminals.TryGetValue(terminal.Terminal, out var list))
                {
                    list = new List<TerminalLocation>(2);
                    locationToTerminals.Add(terminal.Terminal, list);
                }

                list.Add(terminal);
            }
        }

        return locationToTerminals;
    }

    private static HashSet<Location> GetPumpjackCenters(BlueprintRoot root, int marginX, int marginY)
    {
        var pumpjacks = root
            .Blueprint
            .Entities
            .Where(e => e.Name == EntityNames.Vanilla.Pumpjack)
            .ToList();

        var deltaX = 0 - pumpjacks.Min(x => x.Position.X) + marginX;
        var deltaY = 0 - pumpjacks.Min(x => x.Position.Y) + marginY;
        var centers = new HashSet<Location>();
        foreach (var entity in pumpjacks)
        {
            var x = entity.Position.X + deltaX;
            var y = entity.Position.Y + deltaY;

            if (IsInteger(x))
            {
                throw new InvalidDataException($"Entity {entity.EntityNumber} (a '{entity.Name}') does not have an integer X value after translation.");
            }

            if (IsInteger(y))
            {
                throw new InvalidDataException($"Entity {entity.EntityNumber} (a '{entity.Name}') does not have an integer Y value after translation.");
            }

            centers.Add(new Location(ToInt(x), ToInt(y)));
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
        var width = pumpjackCenters.Max(p => p.X) + 1 + marginX;
        var height = pumpjackCenters.Max(p => p.Y) + 1 + marginY;

        SquareGrid grid = new PipeGrid(width, height);

        // Fill the grid with the pumpjacks
        foreach (var center in pumpjackCenters)
        {
            var centerEntity = new PumpjackCenter();
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    GridEntity entity = x != 0 || y != 0 ? new PumpjackSide(centerEntity) : centerEntity;
                    grid.AddEntity(new Location(center.X + x, center.Y + y), entity);
                }
            }
        }

        return grid;
    }

    /// <summary>
    /// . . . + .
    /// . j j j +
    /// . j J j .
    /// + j j j .
    /// . + . . .
    /// </summary>
    public static readonly IReadOnlyList<(Direction Direction, (int DeltaX, int DeltaY))> TerminalOffsets = new List<(Direction Direction, (int DeltaX, int DeltaY))>
    {
        (Direction.Up, (1, -2)),
        (Direction.Right, (2, -1)),
        (Direction.Down, (-1, 2)),
        (Direction.Left, (-2, 1)),
    };

    private static Dictionary<Location, List<TerminalLocation>> GetCenterToTerminals(IReadOnlySet<Location> centers, SquareGrid grid)
    {
        var centerToTerminals = new Dictionary<Location, List<TerminalLocation>>();
        foreach (var center in centers)
        {
            var candidateTerminals = new List<TerminalLocation>();
            foreach ((var direction, var translation) in TerminalOffsets)
            {
                var location = center.Translate(translation);
                var terminal = new TerminalLocation(center, location, direction);
                if (grid.IsEmpty(location))
                {
                    candidateTerminals.Add(terminal);
                }
            }

            centerToTerminals.Add(center, candidateTerminals);
        }

        return centerToTerminals;
    }
}
