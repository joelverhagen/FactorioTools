using System;
using System.Collections.Generic;
using Knapcode.FactorioTools.Data;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

public static class InitializeContext
{
    public static Context Execute(OilFieldOptions options, Blueprint blueprint, IReadOnlyTableList<AvoidLocation> avoid)
    {
        return Execute(options, blueprint, avoid, minWidth: 0, minHeight: 0);
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

        return Execute(options, blueprint, TableArray.Empty<AvoidLocation>(), width, height);
    }

    public static Context Execute(OilFieldOptions options, Blueprint blueprint, IReadOnlyTableList<AvoidLocation> avoid, int minWidth, int minHeight)
    {
        var (centerAndOriginalDirections, avoidLocations, deltaX, deltaY, width, height) = TranslateLocations(options, blueprint, avoid, minWidth, minHeight);

        var grid = InitializeGrid(centerAndOriginalDirections, avoidLocations, width, height);

        var centers = TableArray.New<Location>(centerAndOriginalDirections.Count);
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
        var centerToTerminals = new LocationHashDictionary<ITableList<TerminalLocation>>(centerAndOriginalDirections.Count);
        var locationToTerminals = new LocationHashDictionary<ITableList<TerminalLocation>>();
#else
        var centerToOriginalDirection = new LocationIntDictionary<Direction>(grid.Width, centerAndOriginalDirections.Count);
        var centerToTerminals = new LocationIntDictionary<ITableArray<TerminalLocation>>(grid.Width, centerAndOriginalDirections.Count);
        var locationToTerminals = new LocationIntDictionary<ITableArray<TerminalLocation>>(grid.Width);
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

    private static void PopulateCenters(ITableList<Tuple<Location, Direction>> centerAndOriginalDirections, ITableList<Location> centers)
    {
        for (int i = 0; i < centerAndOriginalDirections.Count; i++)
        {
            centers.Add(centerAndOriginalDirections[i].Item1);
        }
    }

    private static void PopulateCenterToOriginalDirection(ITableList<Tuple<Location, Direction>> centerAndOriginalDirections, ILocationDictionary<Direction> centerToOriginalDirection)
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

    private record TranslatedLocations(
        ITableList<Tuple<Location, Direction>> CenterAndOriginalDirections,
        ITableList<Location> AvoidLocations,
        float DeltaX,
        float DeltaY,
        int Width,
        int Height);

    private static TranslatedLocations TranslateLocations(OilFieldOptions options, Blueprint blueprint, IReadOnlyTableList<AvoidLocation> avoid, int minWidth, int minHeight)
    {
        var pumpjacks = TableArray.New<Entity>(blueprint.Entities.Length);
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

        var centerAndOriginalDirections = TableArray.New<Tuple<Location, Direction>>(pumpjacks.Count);
        var avoidLocations = TableArray.New<Location>(avoid.Count);

        float deltaX = 0;
        float deltaY = 0;

        if (pumpjacks.Count == 0 && avoid.Count == 0)
        {
            return new TranslatedLocations(centerAndOriginalDirections, avoidLocations, deltaX, deltaY, minWidth, minHeight);
        }

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        if (pumpjacks.Count > 0)
        {
            var pumpjackOffsetX = PumpjackWidth / 2;
            var pumpjackOffsetY = PumpjackHeight / 2;

            minX = pumpjacks.EnumerateItems().Min(p => p.Position.X) - pumpjackOffsetX;
            minY = pumpjacks.EnumerateItems().Min(p => p.Position.Y) - pumpjackOffsetY;
            maxX = pumpjacks.EnumerateItems().Max(p => p.Position.X) + pumpjackOffsetX;
            maxY = pumpjacks.EnumerateItems().Max(p => p.Position.Y) + pumpjackOffsetY;

            if (options.AddBeacons)
            {
                // leave room around pumpjacks for beacons
                var beaconOffsetX = ((options.BeaconSupplyWidth - 1) / 2) + (options.BeaconWidth / 2);
                var beaconOffsetY = ((options.BeaconSupplyHeight - 1) / 2) + (options.BeaconHeight / 2);
                minX -= beaconOffsetX;
                minY -= beaconOffsetY;
                maxX += beaconOffsetX;
                maxY += beaconOffsetY;
            }
        }

        if (avoid.Count > 0)
        {
            minX = Math.Min(minX, avoid.EnumerateItems().Min(a => a.X));
            minY = Math.Min(minY, avoid.EnumerateItems().Min(a => a.Y));
            maxX = Math.Max(maxX, avoid.EnumerateItems().Max(a => a.X));
            maxY = Math.Max(maxY, avoid.EnumerateItems().Max(a => a.Y));
        }

        // Leave some space on all sides to cover:
        // - A spot for a pipe.
        // - A spot for a electric pole.
        minX -= 1 + options.ElectricPoleWidth;
        minY -= 1 + options.ElectricPoleHeight;
        maxX += 1 + options.ElectricPoleWidth;
        maxY += 1 + options.ElectricPoleHeight;

        deltaX = -minX;
        deltaY = -minY;

        var width = ToInt(maxX - minX, "The grid width is not an integer after translation.") + 1;
        var height = ToInt(maxY - minY, "The grid height is not an integer after translation.") + 1;

        if (minWidth > width)
        {
            deltaX += (minWidth - width) / 2;
            width = minWidth;
        }

        if (minHeight > height)
        {
            deltaY += (minHeight - height) / 2;
            height = minHeight;
        }

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


        return new TranslatedLocations(centerAndOriginalDirections, avoidLocations, deltaX, deltaY, width, height);
    }

    private static SquareGrid InitializeGrid(ITableList<Tuple<Location, Direction>> centerAndOriginalDirections, ITableList<Location> avoidLocations, int width, int height)
    {
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
