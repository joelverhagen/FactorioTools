using System;
using System.Collections.Generic;
using Knapcode.FactorioTools.Data;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

public static class InitializeContext
{
    public static Context Execute(OilFieldOptions options, Blueprint blueprint, IReadOnlyList<AvoidLocation> avoid)
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

        return Execute(options, blueprint, avoid, marginX, marginY);
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

        return Execute(options, blueprint, Array.Empty<AvoidLocation>(), width, height);
    }

    private static Context Execute(OilFieldOptions options, Blueprint blueprint, IReadOnlyList<AvoidLocation> avoid, int marginX, int marginY)
    {
        var (centerAndOriginalDirections, avoidLocations, deltaX, deltaY) = TranslateLocations(blueprint, avoid, marginX, marginY);

        var grid = InitializeGrid(centerAndOriginalDirections, avoidLocations, marginX, marginY);

        var centers = new List<Location>(centerAndOriginalDirections.Count);
        PopulateCenters(centerAndOriginalDirections, centers);
        centers.Sort((a, b) =>
        {
            var c = a.Y.CompareTo(b.Y);
            if (c != 0)
            {
                return c;
            }

            return a.X.CompareTo(b.X);
        });

#if USE_HASHSETS
        var centerToOriginalDirection = new LocationHashDictionary<Direction>(centerAndOriginalDirections.Count);
        var centerToTerminals = new LocationHashDictionary<List<TerminalLocation>>(centerAndOriginalDirections.Count);
        var locationToTerminals = new LocationHashDictionary<List<TerminalLocation>>();
#else
        var centerToOriginalDirection = new LocationIntDictionary<Direction>(grid.Width, centerAndOriginalDirections.Count);
        var centerToTerminals = new LocationIntDictionary<List<TerminalLocation>>(grid.Width, centerAndOriginalDirections.Count);
        var locationToTerminals = new LocationIntDictionary<List<TerminalLocation>>(grid.Width);
#endif

        PopulateCenterToOriginalDirection(centerAndOriginalDirections, centerToOriginalDirection);
        PopulateCenterToTerminals(centerToTerminals, grid, centers);
        PopulateLocationToTerminals(locationToTerminals, centerToTerminals);

        return new Context
        {
            Options = options,
            InputBlueprint = blueprint,
            DeltaX = deltaX,
            DeltaY = deltaY,
            Grid = grid,
            Centers = centers,
            CenterToTerminals = centerToTerminals,
            CenterToOriginalDirection = centerToOriginalDirection,
            LocationToTerminals = locationToTerminals,
            LocationToAdjacentCount = GetLocationToAdjacentCount(grid),
            SharedInstances = new SharedInstances(grid),
        };
    }

    private static void PopulateCenters(List<Tuple<Location, Direction>> centerAndOriginalDirections, List<Location> centers)
    {
        for (int i = 0; i < centerAndOriginalDirections.Count; i++)
        {
            centers.Add(centerAndOriginalDirections[i].Item1);
        }
    }

    private static void PopulateCenterToOriginalDirection(List<Tuple<Location, Direction>> centerAndOriginalDirections, ILocationDictionary<Direction> centerToOriginalDirection)
    {
        for (int i = 0; i < centerAndOriginalDirections.Count; i++)
        {
            var pair = centerAndOriginalDirections[i];
            centerToOriginalDirection.Add(pair.Item1, pair.Item2);
        }
    }

    private static int[] GetLocationToAdjacentCount(SquareGrid grid)
    {
        var locationToHasAdjacentPumpjack = new int[grid.Width * grid.Height];

#if USE_STACKALLOC && LOCATION_AS_STRUCT
        Span<Location> neighbors = stackalloc Location[4];
#else
        Span<Location> neighbors = new Location[4];
#endif

        foreach (var location in grid.EntityLocations.EnumerateItems())
        {
            var entity = grid[location];
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

                locationToHasAdjacentPumpjack[neighbors[i].Y * grid.Width + neighbors[i].X]++;
            }
        }

        return locationToHasAdjacentPumpjack;
    }

    private static Tuple<List<Tuple<Location, Direction>>, List<Location>, float, float> TranslateLocations(Blueprint blueprint, IReadOnlyList<AvoidLocation> avoid, int marginX, int marginY)
    {
        var pumpjacks = new List<Entity>();
        for (var i = 0; i < blueprint.Entities.Length; i++)
        {
            var entity = blueprint.Entities[i];
            if (entity.Name == EntityNames.Vanilla.Pumpjack)
            {
                pumpjacks.Add(entity);
            }
        }

        const int maxPumpjacks = 150;
        if (pumpjacks.Count > maxPumpjacks)
        {
            throw new FactorioToolsException($"Having more than {maxPumpjacks} pumpjacks is not supported. There are {pumpjacks.Count} pumpjacks provided.");
        }

        var centerAndOriginalDirections = new List<Tuple<Location, Direction>>(pumpjacks.Count);
        var avoidLocations = new List<Location>(avoid.Count);

        float deltaX = 0;
        float deltaY = 0;

        if (pumpjacks.Count > 0 || avoid.Count > 0)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;

            if (pumpjacks.Count > 0)
            {
                minX = pumpjacks.Min(p => p.Position.X);
                minY = pumpjacks.Min(p => p.Position.Y);
            }

            if (avoid.Count > 0)
            {
                minX = Math.Min(minX, avoid.Min(a => a.X));
                minY = Math.Min(minY, avoid.Min(a => a.Y));
            }

            deltaX = 0 - minX + marginX;
            deltaY = 0 - minY + marginY;

            for (int i = 0; i < pumpjacks.Count; i++)
            {
                var p = pumpjacks[i];
                var x = ToInt(p.Position.X + deltaX, $"Entity {p.EntityNumber} (a '{p.Name}') does not have an integer X value after translation.");
                var y = ToInt(p.Position.Y + deltaY, $"Entity {p.EntityNumber} (a '{p.Name}') does not have an integer Y value after translation.");
                var center = new Location(x, y);
                var originalDirection = p.Direction.GetValueOrDefault(Direction.Up);
                centerAndOriginalDirections.Add(Tuple.Create(center, originalDirection));
            }

            for (var i = 0; i < avoid.Count; i++)
            {
                var a = avoid[i];
                var x = ToInt(a.X + deltaX, $"Avoided location {i} does not have an integer X value after translation.");
                var y = ToInt(a.Y + deltaY, $"Avoided location {i} does not have an integer Y value after translation.");
                var avoidLocation = new Location(x, y);
                avoidLocations.Add(avoidLocation);
            }
        }

        return Tuple.Create(centerAndOriginalDirections, avoidLocations, deltaX, deltaY);
    }

    private static SquareGrid InitializeGrid(List<Tuple<Location, Direction>> centerAndOriginalDirections, List<Location> avoidLocations, int marginX, int marginY)
    {
        // Make a grid to contain game state. Similar to the above, we add extra spots for the pumpjacks, pipes, and
        // electric poles.
        int width = marginX;
        int height = marginY;
        if (centerAndOriginalDirections.Count > 0)
        {
            width += 1 + centerAndOriginalDirections.Max(p => p.Item1.X);
            height += 1 + centerAndOriginalDirections.Max(p => p.Item1.Y);
        }

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

        for (int i = 0; i < centerAndOriginalDirections.Count; i++)
        {
            AddPumpjack(grid, centerAndOriginalDirections[i].Item1);
        }

        for (var i = 0; i < avoidLocations.Count; i++)
        {
            var avoidLocation = avoidLocations[i];
            var entity = grid[avoidLocation];
            if (entity is not null && entity is not AvoidEntity)
            {
                throw new FactorioToolsException($"Avoided location {i} has another entity already placed there (perhaps it's part of a pumpjack spot).");
            }

            grid.AddEntity(avoidLocation, new AvoidEntity(grid.GetId()));
        }

        return grid;
    }
}
