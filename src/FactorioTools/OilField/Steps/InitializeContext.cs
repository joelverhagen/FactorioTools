using System;
using System.Collections.Generic;
using System.Linq;
using Knapcode.FactorioTools.Data;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

public static class InitializeContext
{
    public static Context Execute(OilFieldOptions options, Blueprint blueprint)
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

        return Execute(options, blueprint, marginX, marginY);
    }

    public static Context GetEmpty(OilFieldOptions options, int width, int height)
    {
        var blueprint = new Blueprint
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
        };

        return Execute(options, blueprint, width, height);
    }

    private static Context Execute(OilFieldOptions options, Blueprint blueprint, int marginX, int marginY)
    {
        var centerToOriginalDirection = GetPumpjackCenterToOriginalDirection(blueprint, marginX, marginY);
        var grid = InitializeGrid(centerToOriginalDirection.Keys, marginX, marginY);
        var centerToTerminals = GetCenterToTerminals(grid, centerToOriginalDirection.Keys);

        return new Context
        {
            Options = options,
            InputBlueprint = blueprint,
            Grid = grid,
            CenterToTerminals = centerToTerminals,
            CenterToOriginalDirection = centerToOriginalDirection,
            LocationToTerminals = GetLocationToTerminals(centerToTerminals),
            LocationToAdjacentCount = GetLocationToAdjacentCount(grid),
            SharedInstances = new SharedInstances(),
        };
    }

    private static int[,] GetLocationToAdjacentCount(SquareGrid grid)
    {
        var locationToHasAdjacentPumpjack = new int[grid.Width, grid.Height];

#if USE_STACKALLOC
        Span<Location> neighbors = stackalloc Location[4];
#else
        Span<Location> neighbors = new Location[4];
#endif

        foreach ((var entity, var location) in grid.EntityToLocation)
        {
            if (entity is not PumpjackSide)
            {
                continue;
            }

            grid.GetAdjacent(neighbors, location);
            for (var i = 0; i < neighbors.Length; i++)
            {
                if (!neighbors[i].IsValid)
                {
                    continue;
                }

                locationToHasAdjacentPumpjack[neighbors[i].X, neighbors[i].Y]++;
            }
        }

        return locationToHasAdjacentPumpjack;
    }

    private static Dictionary<Location, Direction> GetPumpjackCenterToOriginalDirection(Blueprint blueprint, int marginX, int marginY)
    {
        var pumpjacks = blueprint
            .Entities
            .Where(e => e.Name == EntityNames.Vanilla.Pumpjack)
            .ToList();

        const int maxPumpjacks = 150;
        if (pumpjacks.Count > maxPumpjacks)
        {
            throw new FactorioToolsException($"Having more than {maxPumpjacks} pumpjacks is not supported. There are {pumpjacks.Count} pumpjacks provided.");
        }

        var centerToOriginalDirection = new Dictionary<Location, Direction>();

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

                var location = new Location(ToInt(x), ToInt(y));
                centerToOriginalDirection.Add(location, entity.Direction.GetValueOrDefault(Direction.Up));
            }
        }

        return centerToOriginalDirection;
    }

    private static int ToInt(float x)
    {
        return (int)Math.Round(x, 0);
    }

    private static bool IsInteger(float value)
    {
        return Math.Abs(value % 1) > float.Epsilon * 100;
    }

    private static SquareGrid InitializeGrid(IReadOnlyCollection<Location> pumpjackCenters, int marginX, int marginY)
    {
        // Make a grid to contain game state. Similar to the above, we add extra spots for the pumpjacks, pipes, and
        // electric poles.
        var width = pumpjackCenters.Select(p => p.X).DefaultIfEmpty(-1).Max() + 1 + marginX;
        var height = pumpjackCenters.Select(p => p.Y).DefaultIfEmpty(-1).Max() + 1 + marginY;

        const int maxWidth = 1000;
        const int maxHeight = 1000;
        const int maxArea = 500 * 500;
        var area = width * height;
        if (width > maxWidth || height > maxHeight || area > maxArea)
        {
            throw new FactorioToolsException(
                $"The planning grid cannot be larger than {maxWidth} x {maxHeight} or an area larger than {maxArea}. " +
                $"The planning grid for the provided options is {width} x {height} with an area of {area}.");
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
