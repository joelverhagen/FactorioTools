using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

/// <summary>
/// This "FBE" implementation is copied from Teoxoy's Factorio Blueprint Editor (FBE).
/// Source:
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/beacon.ts
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts
/// </summary>
internal static partial class AddBeacons
{
    private const int MinAffectedEntities = 1;

    private static List<Location> AddBeacons_FBE(Context context, HashSet<Location> pipes)
    {
        if (context.Options.BeaconWidth != context.Options.BeaconHeight
            || context.Options.BeaconSupplyWidth != context.Options.BeaconSupplyHeight
            || context.Options.BeaconWidth % 2 != 1
            || context.Options.BeaconSupplyWidth != context.Options.BeaconWidth * 3)
        {
            throw new NotImplementedException("The beacon must be a square, have an odd number width and height, and have a supply area that is 3 times the width.");
        }

        var grid = new LocationInfo[context.Grid.Width, context.Grid.Height];

        AddEntityAreas(context, grid, pipes);

        // Visualizer.Show(context.Grid, occupiedPositions.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        // GENERATE VALID BEACON POSITIONS
        var validBeaconPositions = AddValidBeaconPositions(context, grid);

        // Visualizer.Show(context.Grid, grid.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        // GENERATE POSSIBLE BEACON AREAS
        var possibleBeaconAreas = GetPossibleBeaconAreas(context, grid, validBeaconPositions);

        // GENERATE POSSIBLE BEACONS
        var possibleBeacons = GetPossibleBeacons(
            context,
            grid,
            possibleBeaconAreas);

        possibleBeacons = SortPossibleBeacons(possibleBeacons);

        var pointToBeacons = GetPointsToBeacons(possibleBeacons);

        // GENERATE BEACONS
        return GetBeacons(possibleBeacons, pointToBeacons);
    }

    private static List<Location> GetBeacons(List<BeaconCandidate> possibleBeacons, Dictionary<Location, List<BeaconCandidate>> pointToBeacons)
    {
        var beacons = new List<Location>();
        var collided = new HashSet<BeaconCandidate>();
        while (possibleBeacons.Count > 0)
        {
            var beacon = possibleBeacons[possibleBeacons.Count - 1];
            possibleBeacons.RemoveAt(possibleBeacons.Count - 1);

            if (collided.Contains(beacon))
            {
                continue;
            }

            beacons.Add(beacon.Center);
            collided.UnionWith(beacon.CollisionArea.SelectMany(p => pointToBeacons[p]));
        }

        return beacons;
    }

    private static List<BeaconCandidate> SortPossibleBeacons(List<BeaconCandidate> possibleBeacons)
    {
        possibleBeacons = possibleBeacons
            .OrderByDescending(b => b.EffectsGiven)
            .ThenBy(b => b.EffectsGiven == 1 ? -b.AverageDistanceToEntities : b.NumberOfOverlaps)
            .ToList();
        possibleBeacons.Reverse();
        return possibleBeacons;
    }

    private static int GetBeaconEffectRadius(Context context)
    {
        return (context.Options.BeaconSupplyWidth - context.Options.BeaconWidth) / 2;
    }

    private static List<BeaconCandidate> GetPossibleBeacons(
        Context context,
        LocationInfo[,] grid,
        List<Location[]> possibleBeaconAreas)
    {
        var beaconEffectRadius = GetBeaconEffectRadius(context);
        var centerIndex = (context.Options.BeaconWidth * context.Options.BeaconHeight) / 2;
        var possibleBeacons = new List<BeaconCandidate>(possibleBeaconAreas.Count);
        for (var i = 0; i < possibleBeaconAreas.Count; i++)
        {
            var collisionArea = possibleBeaconAreas[i];
            var center = collisionArea[centerIndex];

            var d = context.Options.BeaconWidth + beaconEffectRadius * 2;
            var d2 = d * d;

            var effectsGiven = new HashSet<Area[]>();
            for (var j = 0; j < d2; j++)
            {
                var x = center.X + ((j % d) - (d / 2));
                var y = center.Y + ((j / d) - (d / 2));
                var area = grid[x, y]?.EntityArea;

                if (area is not null)
                {
                    effectsGiven.Add(area);
                }
            }

            if (effectsGiven.Count < MinAffectedEntities)
            {
                continue;
            }

            var avgDistToEntities = effectsGiven.Average(p => p[centerIndex].Location.GetManhattanDistance(center));
            var nrOfOverlaps = collisionArea.Sum(p => grid[p.X, p.Y].BeaconCount);

            possibleBeacons.Add(new BeaconCandidate(
                center,
                collisionArea,
                effectsGiven.Count,
                avgDistToEntities,
                nrOfOverlaps));
        }

        return possibleBeacons;
    }

    private static void AddEntityAreas(Context context, LocationInfo[,] grid, HashSet<Location> pipes)
    {
        foreach (var pipe in pipes)
        {
            grid[pipe.X, pipe.Y] = new LocationInfo { Occupied = true };
        }

        foreach ((var entity, var location) in context.Grid.EntityToLocation)
        {
            int width;
            int height;
            bool effect;

            switch (entity)
            {
                case ElectricPoleCenter:
                    width = context.Options.ElectricPoleWidth;
                    height = context.Options.ElectricPoleHeight;
                    effect = false;
                    break;
                case PumpjackCenter:
                    width = PumpjackWidth;
                    height = PumpjackHeight;
                    effect = true;
                    break;
                case Pipe:
                case BeaconCenter:
                case BeaconSide:
                case ElectricPoleSide:
                case PumpjackSide:
                    continue;
                default:
                    throw new NotImplementedException();
            }

            var minX = location.X - ((width - 1) / 2);
            var maxX = location.X + (width / 2);
            var minY = location.Y - ((height - 1) / 2);
            var maxY = location.Y + (height / 2);

            var area = new Area[width * height];
            var i = 0;
            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    area[i++] = new Area(new Location(x, y), effect);
                    grid[x, y] = new LocationInfo
                    {
                        Occupied = true,
                        EntityArea = effect ? area : null,
                    };
                }
            }
        }
    }

    private static HashSet<Location> AddValidBeaconPositions(Context context, LocationInfo[,] grid)
    {
        var validBeaconPositions = new HashSet<Location>();
        var beaconEffectRadius = GetBeaconEffectRadius(context);
        var searchSize = PumpjackWidth + context.Options.BeaconWidth * 2 + (beaconEffectRadius - 1) * 2;
        var searchSize2 = searchSize * searchSize;
        foreach (var center in context.CenterToTerminals.Keys)
        {
            for (var i = 0; i < searchSize2; i++)
            {
                var location = new Location(
                    x: center.X + ((i % searchSize) - (searchSize / 2)),
                    y: center.Y + ((i / searchSize) - (searchSize / 2)));

                if (grid[location.X, location.Y] is null)
                {
                    validBeaconPositions.Add(location);
                    grid[location.X, location.Y] = new LocationInfo
                    {
                        ValidBeaconPosition = true,
                    };
                }
            }
        }

        return validBeaconPositions;
    }

    private static List<Location[]> GetPossibleBeaconAreas(Context context, LocationInfo[,] grid, HashSet<Location> validBeaconPositions)
    {
        var beaconArea = context.Options.BeaconWidth * context.Options.BeaconHeight;
        var possibleBeaconAreas = new List<Location[]>();
        foreach (var position in validBeaconPositions)
        {
            var allContains = true;
            var area = new Location[beaconArea];
            for (var i = 0; allContains && i < beaconArea; i++)
            {
                var location = new Location(
                    x: position.X + (i % context.Options.BeaconWidth),
                    y: position.Y + (i / context.Options.BeaconWidth));

                var existingInfo = grid[location.X, location.Y];

                if (existingInfo is not null && existingInfo.ValidBeaconPosition)
                {
                    existingInfo.BeaconCount++;
                    area[i] = location;
                }
                else
                {
                    allContains = false;
                }
            }

            if (allContains)
            {
                possibleBeaconAreas.Add(area);
            }
        }

        return possibleBeaconAreas;
    }

    private static Dictionary<Location, int> GetPointToBeaconCount(List<Location[]> possibleBeaconAreas, LocationInfo[,] grid)
    {
        var pointToBeaconCount = new Dictionary<Location, int>();
        for (var i = 0; i < possibleBeaconAreas.Count; i++)
        {
            var areas = possibleBeaconAreas[i];
            for (var j = 0; j < areas.Length; j++)
            {
                var point = areas[j];
                if (!pointToBeaconCount.TryGetValue(point, out var sum))
                {
                    pointToBeaconCount.Add(point, 1);
                }
                else
                {
                    pointToBeaconCount[point] = sum + 1;
                }
            }
        }

        return pointToBeaconCount;
    }

    private static Dictionary<Location, List<BeaconCandidate>> GetPointsToBeacons(List<BeaconCandidate> possibleBeacons)
    {
        var pointToBeacons = new Dictionary<Location, List<BeaconCandidate>>();
        for (var i = 0; i < possibleBeacons.Count; i++)
        {
            var beacon = possibleBeacons[i];
            for (var j = 0; j < beacon.CollisionArea.Length; j++)
            {
                var point = beacon.CollisionArea[j];
                if (!pointToBeacons.TryGetValue(point, out var candidates))
                {
                    candidates = new List<BeaconCandidate> { beacon };
                    pointToBeacons.Add(point, candidates);
                }
                else
                {
                    candidates.Add(beacon);
                }
            }
        }

        return pointToBeacons;
    }

    private class LocationInfo
    {
        public bool Occupied { get; set; }
        public bool ValidBeaconPosition { get; set; }
        public int BeaconCount { get; set; }
        public Area[]? EntityArea { get; set; }
        public List<BeaconCandidate>? Beacons { get; set; }
    }

    private record Area(Location Location, bool Effect);

    private record BeaconCandidate(Location Center, Location[] CollisionArea, int EffectsGiven, double AverageDistanceToEntities, int NumberOfOverlaps);
}