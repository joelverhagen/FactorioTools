using PumpjackPipeOptimizer.Data;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

internal static class InitializeContext
{
    public static Context Execute(Options options, BlueprintRoot inputBlueprint)
    {
        var centers = GetPumpjackCenters(options, inputBlueprint);
        var grid = InitializeGrid(centers);
        var centerToTerminals = GetCenterToTerminals(centers, grid);

        var context = new Context
        {
            Options = options,
            InputBlueprint = inputBlueprint,
            Grid = grid,
            Centers = centers,
            CenterToTerminals = centerToTerminals,
        };
        return context;
    }

    private static HashSet<Location> GetPumpjackCenters(Options options, BlueprintRoot root)
    {
        var pumpjacks = root
            .Blueprint
            .Entities
            .Where(e => e.Name == EntityNames.Vanilla.Pumpjack)
            .ToList();

        // Translate the blueprint by the minimum X and Y. Leave three spaces on both lesser (left for X, top for Y) sides to cover:
        //   - The side of the pumpjack. It is a 3x3 entity and the position of the entity is the center.
        //   - A spot for a pipe, if needed.
        //   - A spot for an electric pole, if needed.
        var deltaX = 0 - pumpjacks.Min(x => x.Position.X) + 1 + 1 + options.ElectricPoleWidth;
        var deltaY = 0 - pumpjacks.Min(x => x.Position.Y) + 1 + 1 + options.ElectricPoleHeight;
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

    private static SquareGrid InitializeGrid(IReadOnlySet<Location> pumpjackCenters)
    {
        // Make a grid to contain game state. Similar to the above, we add extra spots for the pumpjacks, pipes, and
        // electric poles.
        var width = pumpjackCenters.Max(p => p.X) + 4;
        var height = pumpjackCenters.Max(p => p.Y) + 4;

        SquareGrid grid = new PipeGrid(width, height);

        // Fill the grid with the pumpjacks
        foreach (var center in pumpjackCenters)
        {
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    GridEntity entity = x != 0 || y != 0 ? new PumpjackSide() : new PumpjackCenter();
                    grid.AddEntity(new Location(center.X + x, center.Y + y), entity);
                }
            }
        }

        return grid;
    }

    private static Dictionary<Location, HashSet<Location>> GetCenterToTerminals(IReadOnlySet<Location> centers, SquareGrid grid)
    {
        var centerToTerminals = new Dictionary<Location, HashSet<Location>>();
        foreach (var center in centers)
        {
            // . . . x .
            // . j j j x
            // . j j j .
            // x j j j .
            // . x . . .
            var top = new Location(center.X + 1, center.Y - 2);
            var right = new Location(center.X + 2, center.Y - 1);
            var bottom = new Location(center.X - 1, center.Y + 2);
            var left = new Location(center.X - 2, center.Y + 1);
            var candidateTerminals = new HashSet<Location>();

            foreach (var terminal in new[] { top, right, bottom, left })
            {
                if (grid.IsInBounds(terminal) && grid.IsEmpty(terminal))
                {
                    candidateTerminals.Add(terminal);
                }
            }

            centerToTerminals.Add(center, candidateTerminals);
        }

        return centerToTerminals;
    }
}
