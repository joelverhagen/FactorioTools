using System;
using System.Collections.Generic;
using System.Linq;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

/// <summary>
/// This "FBE" implementation is based on Teoxoy's Factorio Blueprint Editor (FBE).
/// Source:
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/beacon.ts
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts
/// 
/// It has been modified:
/// 
/// - Some performance improvements (some are .NET specific)
///   - Many .NET specific performance improvemants.
///   - The beacon candidate sorting only happens once.
/// - Some quality improvements which yield better and more consistent results.
///   - Sorting only once
///   - Sort with a stable tie-breaking criteria (distance from the middle)
/// - Add support for non-standard beacon sizes
/// - Add support for non-overlapping beacons (for Space Exploration beacon overlap)
/// </summary>
public static partial class PlanBeacons
{
    private const int MinAffectedEntities = 1;

    private static (List<Location> Beacons, int Effects) AddBeaconsFbe(Context context, BeaconStrategy strategy)
    {
        var entityAreas = GetEntityAreas(context);
        var occupiedPositions = GetOccupiedPositions(entityAreas);

        // Visualizer.Show(context.Grid, occupiedPositions.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        var possibleBeaconAreas = GetPossibleBeaconAreas(context, occupiedPositions);

        // Visualizer.Show(context.Grid, possibleBeaconAreas.SelectMany(l => l).Distinct().Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        var pointToBeaconCount = GetPointToBeaconCount(possibleBeaconAreas);
        var effectEntityAreas = GetEffectEntityAreas(entityAreas);
        var pointToEntityArea = GetPointToEntityArea(effectEntityAreas);

        // GENERATE POSSIBLE BEACONS
        var possibleBeacons = GetPossibleBeacons(
            context,
            effectEntityAreas,
            possibleBeaconAreas,
            pointToBeaconCount,
            pointToEntityArea);

        if (strategy == BeaconStrategy.Fbe)
        {
            possibleBeacons = SortPossibleBeacons(context, possibleBeacons);
        }
        else if (strategy == BeaconStrategy.FbeOriginal)
        {
            SortPossibleBeaconsOriginal(possibleBeacons);
        }

        // GENERATE BEACONS
        return GetBeacons(context, strategy, effectEntityAreas, possibleBeacons);
    }

    private static (List<Location> Beacons, int Effects) GetBeacons(Context context, BeaconStrategy strategy, List<Area> effectEntityAreas, List<BeaconCandidate> possibleBeacons)
    {
        var beacons = new List<Location>();
        var effects = 0;
        var collisionArea = new LocationSet();
        var coveredEntityAreas = context.Options.OverlapBeacons ? null : new CountedBitArray(effectEntityAreas.Count);
        while (possibleBeacons.Count > 0)
        {
            var beacon = possibleBeacons[possibleBeacons.Count - 1];
            possibleBeacons.RemoveAt(possibleBeacons.Count - 1);

            if (collisionArea.Overlaps(beacon.CollisionArea))
            {
                continue;
            }
            
            if (!context.Options.OverlapBeacons)
            {
                var overlapping = new CountedBitArray(beacon.EffectsGiven!);
                overlapping.And(coveredEntityAreas!);

                if (overlapping.TrueCount > 0)
                {
                    continue;
                }

                coveredEntityAreas!.Or(beacon.EffectsGiven!);

                if (coveredEntityAreas.TrueCount == coveredEntityAreas.Count)
                {
                    break;
                }
            }

            beacons.Add(beacon.Center);
            effects += beacon.EffectsGivenCount;
            // Console.WriteLine($"{beacon.Center} --- {beacon.EffectsGivenCount}");
            collisionArea.UnionWith(beacon.CollisionArea);
        }

        return (beacons, effects);
    }

    private static List<BeaconCandidate> SortPossibleBeacons(Context context, List<BeaconCandidate> possibleBeacons)
    {
        possibleBeacons = possibleBeacons
            .OrderByDescending(b => b.EffectsGivenCount)
            .ThenBy(b => b.EffectsGivenCount == 1 ? -b.AverageDistanceToEntities : b.NumberOfOverlaps)
            .ThenByDescending(b => b.Center.GetEuclideanDistance(context.Grid.Middle))
            .ToList();
        possibleBeacons.Reverse();
        return possibleBeacons;
    }

    private static void SortPossibleBeaconsOriginal(List<BeaconCandidate> possibleBeacons)
    {
        /*
        possibleBeacons
            .Sort((a, b) =>
            {
                var c = b.EffectsGivenCount.CompareTo(a.EffectsGivenCount);
                if (c != 0)
                {
                    return c;
                }

                var aN = a.EffectsGivenCount == 1 ? -a.AverageDistanceToEntities : a.NumberOfOverlaps;
                var bN = b.EffectsGivenCount == 1 ? -b.AverageDistanceToEntities : b.NumberOfOverlaps;
                c = aN.CompareTo(bN);
                if (c != 0)
                {
                    return c;
                }

                aN = a.Center.GetEuclideanDistance(context.Grid.Middle);
                bN = b.Center.GetEuclideanDistance(context.Grid.Middle);
                return bN.CompareTo(aN);
            });
        */

        // This is not exactly like FBE because it causes inconsistent sorting results causing an exception.
        // The original is here. The original comparer violates expectations held by List<T>.Sort in .NET:
        // https://github.com/teoxoy/factorio-blueprint-editor/blob/83343e6a6c91608c43a823326fb16c01c934b4bd/packages/editor/src/core/generators/beacon.ts#L177-L183
        possibleBeacons
            .Sort((a, b) =>
            {
                var c = b.EffectsGivenCount.CompareTo(a.EffectsGivenCount);
                if (c != 0)
                {
                    return c;
                }

                c = a.NumberOfOverlaps.CompareTo(b.NumberOfOverlaps);
                if (c != 0)
                {
                    return c;
                }

                return b.AverageDistanceToEntities.CompareTo(a.AverageDistanceToEntities);
            });

        /*
        possibleBeacons
            .Sort((a, b) =>
            {
                if (a.EffectsGivenCount == 1 || b.EffectsGivenCount == 1)
                {
                    return b.AverageDistanceToEntities.CompareTo(a.AverageDistanceToEntities);
                }

                return a.NumberOfOverlaps.CompareTo(b.NumberOfOverlaps);
            });

        possibleBeacons.Sort((a, b) => b.EffectsGivenCount.CompareTo(a.EffectsGivenCount));
        */

        possibleBeacons.Reverse();
    }

    private static List<BeaconCandidate> GetPossibleBeacons(
        Context context,
        List<Area> effectEntityAreas,
        List<Location[]> possibleBeaconAreas,
        Dictionary<Location, int> pointToBeaconCount,
        Dictionary<Location, Area> pointToEntityArea)
    {
        (int entityMinX, int entityMinY, int entityMaxX, int entityMaxY) = GetBounds(pointToEntityArea.Keys);

        var centerX = (context.Options.BeaconWidth - 1) / 2;
        var centerY = (context.Options.BeaconHeight - 1) / 2;
        var centerIndex = centerY * context.Options.BeaconWidth + centerX;

        var possibleBeacons = new List<BeaconCandidate>(possibleBeaconAreas.Count);
        var effectsGiven = new CountedBitArray(effectEntityAreas.Count);
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
                        effectsGiven[area.Index] = true;
                    }
                }
            }

            if (effectsGiven.TrueCount < MinAffectedEntities)
            {
                effectsGiven.SetAll(false);
                continue;
            }

            var sumDistance = 0;
            for (var j = 0; j < effectsGiven.Count; j++)
            {
                if (effectsGiven[j])
                {
                    sumDistance += effectEntityAreas[j].Locations[centerIndex].GetManhattanDistance(center);
                }
            }

            var averageDistanceToEntities = ((double)sumDistance) / effectsGiven.TrueCount;

            var numberOfOverlaps = 0;
            for (var j = 0; j < collisionArea.Length; j++)
            {
                numberOfOverlaps += pointToBeaconCount[collisionArea[j]];
            }

            possibleBeacons.Add(new BeaconCandidate(
                center,
                collisionArea,
                effectsGiven.TrueCount,
                averageDistanceToEntities,
                numberOfOverlaps,
                context.Options.OverlapBeacons ? null : new CountedBitArray(effectsGiven)));

            effectsGiven.SetAll(false);
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

    private static LocationSet GetOccupiedPositions(List<Area> entityAreas)
    {
        return entityAreas
            .SelectMany(a => a.Locations)
            .ToSet();
    }

    private static List<Area> GetEntityAreas(Context context)
    {
        GridEntity pipe = new Pipe();

        return context
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

                var area = new Location[width * height];
                var i = 0;
                for (var x = minX; x <= maxX; x++)
                {
                    for (var y = minY; y <= maxY; y++)
                    {
                        area[i++] = new Location(x, y);
                    }
                }

                return new { Locations = area, Effect = effect };
            })
            .Where(a => a is not null)
            .Select(a => a!)
            .Select((a, i) => new Area(i, a.Effect, a.Locations))
            .ToList();
    }

    private static List<Area> GetEffectEntityAreas(List<Area> entityAreas)
    {
        var effectEntityArea = new List<Area>();
        for (var i = 0; i < entityAreas.Count; i++)
        {
            var area = entityAreas[i];
            if (area.Effect)
            {
                area.Index = effectEntityArea.Count;
                effectEntityArea.Add(area);
            }
            else
            {
                area.Index = -1;
            }
        }

        return effectEntityArea;
    }

    private static List<Location[]> GetPossibleBeaconAreas(Context context, LocationSet occupiedPositions)
    {
        var validBeaconCenters = new LocationSet();
        var possibleBeaconAreas = new List<Location[]>();

        var gridMinX = (context.Options.BeaconWidth - 1) / 2;
        var gridMinY = (context.Options.BeaconHeight - 1) / 2;
        var gridMaxX = context.Grid.Width - (context.Options.BeaconWidth / 2) - 1;
        var gridMaxY = context.Grid.Height - (context.Options.BeaconHeight / 2) - 1;

        var supplyLeft = ((PumpjackWidth - 1) / 2) + (context.Options.BeaconSupplyWidth / 2);
        var supplyUp = ((PumpjackHeight - 1) / 2) + (context.Options.BeaconSupplyHeight / 2);
        var supplyRight = (PumpjackWidth / 2) + ((context.Options.BeaconSupplyWidth - 1) / 2);
        var supplyDown = (PumpjackHeight / 2) + ((context.Options.BeaconSupplyHeight - 1) / 2);

        var beaconLeft = (context.Options.BeaconWidth - 1) / 2;
        var beaconUp = (context.Options.BeaconHeight - 1) / 2;
        var beaconRight = context.Options.BeaconWidth / 2;
        var beaconDown = context.Options.BeaconHeight / 2;

        var area = new Location[context.Options.BeaconWidth * context.Options.BeaconHeight];

        foreach (var center in context.CenterToTerminals.Keys)
        {
            if (center == new Location(47, 13))
            {
            }

            var supplyMinX = Math.Max(gridMinX, center.X - supplyLeft);
            var supplyMinY = Math.Max(gridMinY, center.Y - supplyUp);
            var supplyMaxX = Math.Min(gridMaxX, center.X + supplyRight);
            var supplyMaxY = Math.Min(gridMaxY, center.Y + supplyDown);

            for (var centerX = supplyMinX; centerX <= supplyMaxX; centerX++)
            {
                for (var centerY = supplyMinY; centerY <= supplyMaxY; centerY++)
                {
                    var beaconCenter = new Location(centerX, centerY);
                    if (!validBeaconCenters.Add(beaconCenter))
                    {
                        continue;
                    }

                    var minX = beaconCenter.X - beaconLeft;
                    var minY = beaconCenter.Y - beaconUp;
                    var maxX = beaconCenter.X + beaconRight;
                    var maxY = beaconCenter.Y + beaconDown;
                    var fits = true;

                    var i = 0;
                    for (var y = minY; fits && y <= maxY; y++)
                    {
                        for (var x = minX; fits && x <= maxX; x++)
                        {
                            var location = new Location(x, y);
                            if (occupiedPositions.Contains(location))
                            {
                                fits = false;
                            }
                            else
                            {
                                area[i++] = location;
                            }
                        }
                    }

                    if (fits)
                    {
                        possibleBeaconAreas.Add(area);
                        area = new Location[context.Options.BeaconWidth * context.Options.BeaconHeight];
                    }
                }
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

    private static Dictionary<Location, Area> GetPointToEntityArea(List<Area> effectEntityAreas)
    {
        var pointToEntityArea = new Dictionary<Location, Area>();
        for (var i = 0; i < effectEntityAreas.Count; i++)
        {
            var area = effectEntityAreas[i];
            for (var j = 0; j < area.Locations.Length; j++)
            {
                pointToEntityArea.Add(area.Locations[j], area);
            }
        }

        return pointToEntityArea;
    }

    private class Area
    {
        public Area(int index, bool effect, Location[] locations)
        {
            Index = index;
            Effect = effect;
            Locations = locations;
        }

        public int Index { get; set; }
        public bool Effect { get; }
        public Location[] Locations { get; }
    }

    private record BeaconCandidate(
        Location Center,
        Location[] CollisionArea,
        int EffectsGivenCount,
        double AverageDistanceToEntities,
        int NumberOfOverlaps,
        CountedBitArray? EffectsGiven);
}