﻿using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

/// <summary>
/// This "FBE" implementation is copied from Teoxoy's Factorio Blueprint Editor (FBE).
/// Source:
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/beacon.ts
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts
/// 
/// It has been modified with some performance improvements (some are .NET specific) and some quality improvements which
/// yield better results. Most notably, the beacon candidate sorting only happens once.
/// </summary>
internal static partial class PlanBeacons
{
    private const int MinAffectedEntities = 1;

    private static List<Location> AddBeacons_FBE(Context context)
    {
        if (context.Options.BeaconWidth != context.Options.BeaconHeight
            || context.Options.BeaconSupplyWidth != context.Options.BeaconSupplyHeight
            || context.Options.BeaconWidth % 2 != 1
            || context.Options.BeaconSupplyWidth != context.Options.BeaconWidth * 3)
        {
            throw new NotImplementedException("The beacon must be a square, have an odd number width and height, and have a supply area that is 3 times the width.");
        }

        var entityAreas = GetEntityAreas(context);
        var occupiedPositions = GetOccupiedPositions(entityAreas);

        // Visualizer.Show(context.Grid, occupiedPositions.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        // GENERATE VALID BEACON POSITIONS
        var validBeaconPositions = GetValidBeaconPositions(context, occupiedPositions);

        // Visualizer.Show(context.Grid, grid.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        // GENERATE POSSIBLE BEACON AREAS
        var possibleBeaconAreas = GetPossibleBeaconAreas(context, validBeaconPositions);
        var pointToBeaconCount = GetPointToBeaconCount(possibleBeaconAreas);
        var pointToEntityArea = GetPointToEntityArea(entityAreas);

        // GENERATE POSSIBLE BEACONS
        var possibleBeacons = GetPossibleBeacons(
            context,
            possibleBeaconAreas,
            pointToBeaconCount,
            pointToEntityArea);

        possibleBeacons = SortPossibleBeacons(possibleBeacons);

        // GENERATE BEACONS
        return GetBeacons(possibleBeacons);
    }

    private static List<Location> GetBeacons(List<BeaconCandidate> possibleBeacons)
    {
        var beacons = new List<Location>();
        var collisionArea = new HashSet<Location>();
        while (possibleBeacons.Count > 0)
        {
            var beacon = possibleBeacons[possibleBeacons.Count - 1];
            possibleBeacons.RemoveAt(possibleBeacons.Count - 1);

            if (collisionArea.Overlaps(beacon.CollisionArea))
            {
                continue;
            }

            beacons.Add(beacon.Center);
            collisionArea.UnionWith(beacon.CollisionArea);
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
        List<Location[]> possibleBeaconAreas,
        Dictionary<Location, int> pointToBeaconCount,
        Dictionary<Location, Area[]> pointToEntityArea)
    {
        (int entityMinX, int entityMinY, int entityMaxX, int entityMaxY) = GetBounds(pointToEntityArea.Keys);

        var centerIndex = (context.Options.BeaconWidth * context.Options.BeaconHeight) / 2;
        var possibleBeacons = new List<BeaconCandidate>(possibleBeaconAreas.Count);
        var effectsGiven = new HashSet<Area[]>();
        for (var i = 0; i < possibleBeaconAreas.Count; i++)
        {
            var collisionArea = possibleBeaconAreas[i];
            var center = collisionArea[centerIndex];

            var minX = Math.Max(entityMinX, center.X - (context.Options.BeaconSupplyWidth / 2));
            var minY = Math.Max(entityMinY, center.Y - (context.Options.BeaconSupplyHeight / 2));
            var maxX = Math.Min(entityMaxX, center.X + (context.Options.BeaconSupplyWidth / 2));
            var maxY = Math.Min(entityMaxY, center.Y + (context.Options.BeaconSupplyHeight / 2));

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var location = new Location(x, y);
                    if (pointToEntityArea.TryGetValue(location, out var area))
                    {
                        effectsGiven.Add(area);
                    }
                }
            }

            if (effectsGiven.Count < MinAffectedEntities)
            {
                effectsGiven.Clear();
                continue;
            }

            var sumDistance = 0;
            foreach (var p in effectsGiven)
            {
                sumDistance += p[centerIndex].Location.GetManhattanDistance(center);
            }

            var averageDistanceToEntities = ((double)sumDistance) / effectsGiven.Count;

            var numberOfOverlaps = 0;
            for (var j = 0; j < collisionArea.Length; j++)
            {
                numberOfOverlaps += pointToBeaconCount[collisionArea[j]];
            }

            possibleBeacons.Add(new BeaconCandidate(
                center,
                collisionArea,
                effectsGiven.Count,
                averageDistanceToEntities,
                numberOfOverlaps));
            effectsGiven.Clear();
        }

        return possibleBeacons;
    }

    private static (int entityMinX, int entityMinY, int entityMaxX, int entityMaxY) GetBounds(IReadOnlyCollection<Location> locations)
    {
        var entityMinX = int.MaxValue;
        var entityMinY = int.MaxValue;
        var entityMaxX = int.MinValue;
        var entityMaxY = int.MinValue;

        foreach (var location in locations)
        {
            if (location.X < entityMinX)
            {
                entityMinX = location.X;
            }

            if (location.Y < entityMinY)
            {
                entityMinY = location.Y;
            }

            if (location.X > entityMaxX)
            {
                entityMaxX = location.X;
            }
            if (location.Y > entityMaxY)
            {
                entityMaxY = location.Y;
            }
        }

        return (entityMinX, entityMinY, entityMaxX, entityMaxY);
    }

    private static HashSet<Location> GetOccupiedPositions(List<Area[]> entityAreas)
    {
        return entityAreas
            .SelectMany(a => a)
            .Select(a => a.Location)
            .ToHashSet();
    }

    private static List<Area[]> GetEntityAreas(Context context)
    {
        GridEntity pipe = new Pipe();
        var entityAreas = context
            .Grid
            .EntityToLocation
            .Select(pair =>
            {
                int width;
                int height;
                bool effect;

                switch (pair.Key)
                {
                    case TemporaryEntity:
                        width = 1;
                        height = 1;
                        effect = false;
                        break;
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
                    case BeaconCenter:
                    case BeaconSide:
                    case ElectricPoleSide:
                    case PumpjackSide:
                        return null;
                    default:
                        throw new NotImplementedException();
                }

                var minX = pair.Value.X - ((width - 1) / 2);
                var maxX = pair.Value.X + (width / 2);
                var minY = pair.Value.Y - ((height - 1) / 2);
                var maxY = pair.Value.Y + (height / 2);

                var area = new Area[width * height];
                var i = 0;
                for (var x = minX; x <= maxX; x++)
                {
                    for (var y = minY; y <= maxY; y++)
                    {
                        area[i++] = new Area(new Location(x, y), effect);
                    }
                }

                return area;
            })
            .Where(a => a is not null)
            .Select(a => a!)
            .ToList();
        return entityAreas;
    }

    private static HashSet<Location> GetValidBeaconPositions(Context context, HashSet<Location> occupiedPositions)
    {
        var validBeaconPositions = new HashSet<Location>();
        var beaconEffectRadius = GetBeaconEffectRadius(context);
        var searchSize = PumpjackWidth + context.Options.BeaconWidth * 2 + (beaconEffectRadius - 1) * 2;
        var searchSize2 = searchSize * searchSize;
        foreach (var center in context.CenterToTerminals.Keys)
        {
            for (var i = 0; i < searchSize2; i++)
            {
                validBeaconPositions.Add(new Location(
                    x: center.X + ((i % searchSize) - (searchSize / 2)),
                    y: center.Y + ((i / searchSize) - (searchSize / 2))));
            }
        }

        validBeaconPositions.ExceptWith(occupiedPositions);
        return validBeaconPositions;
    }

    private static List<Location[]> GetPossibleBeaconAreas(Context context, HashSet<Location> validBeaconPositions)
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

                if (validBeaconPositions.Contains(location))
                {
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

    private static Dictionary<Location, int> GetPointToBeaconCount(List<Location[]> possibleBeaconAreas)
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

    private static Dictionary<Location, Area[]> GetPointToEntityArea(List<Area[]> entityAreas)
    {
        var pointToEntityArea = new Dictionary<Location, Area[]>();
        for (var i = 0; i < entityAreas.Count; i++)
        {
            var area = entityAreas[i];
            if (!area[0].Effect)
            {
                continue;
            }

            for (var j = 0; j < area.Length; j++)
            {
                pointToEntityArea.Add(area[j].Location, area);
            }
        }

        return pointToEntityArea;
    }

    private record Area(Location Location, bool Effect);

    private record BeaconCandidate(Location Center, Location[] CollisionArea, int EffectsGiven, double AverageDistanceToEntities, int NumberOfOverlaps);
}