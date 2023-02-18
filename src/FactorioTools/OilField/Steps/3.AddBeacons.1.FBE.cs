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
    private static HashSet<Location> AddBeacons_FBE(Context context)
    {
        const int beaconSize = 3;
        const int beaconEffectRadius = 3;
        const int minAffectedEntities = 1;

        if (context.Options.BeaconWidth != context.Options.BeaconHeight
            || context.Options.BeaconSupplyWidth != context.Options.BeaconSupplyHeight
            || context.Options.BeaconWidth != beaconSize
            || context.Options.BeaconSupplyWidth != beaconEffectRadius * 2 + beaconSize)
        {
            throw new NotImplementedException();
        }

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
                    case Pipe:
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

        var occupiedPositions = entityAreas.SelectMany(a => a).Select(a => a.Location).ToHashSet();

        // Visualizer.Show(context.Grid, occupiedPositions.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        // GENERATE VALID BEACON POSITIONS
        var validBeaconPositions = context
            .Grid
            .EntityToLocation
            .Where(pair => pair.Key is PumpjackCenter)
            .SelectMany(pair =>
            {
                var searchSize = PumpjackWidth + beaconSize * 2 + (beaconEffectRadius - 1) * 2;
                return Enumerable
                    .Range(0, searchSize * searchSize)
                    .Select(i => new Location(
                        x: pair.Value.X + ((i % searchSize) - (searchSize / 2)),
                        y: pair.Value.Y + ((i / searchSize) - (searchSize / 2))));
            })
            .ToHashSet();
        validBeaconPositions.ExceptWith(occupiedPositions);

        var grid = new HashSet<Location>(validBeaconPositions);

        // Visualizer.Show(context.Grid, grid.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        // GENERATE POSSIBLE BEACON AREAS
        var possibleBeaconAreas = validBeaconPositions
            .Select(p =>
            {
                return Enumerable
                    .Range(0, beaconSize * beaconSize)
                    .Select(i => new Location(
                        x: p.X + (i % beaconSize),
                        y: p.Y + (i / beaconSize)))
                    .Where(p => grid.Contains(p))
                    .ToList();
            })
            .Where(arr => arr.Count == beaconSize * beaconSize)
            .ToList();

        var pointToBeaconCount = possibleBeaconAreas
            .SelectMany(arr => arr)
            .Aggregate(new Dictionary<Location, int>(), (map, p) =>
            {
                if (!map.TryGetValue(p, out var count))
                {
                    map.Add(p, 1);
                }
                else
                {
                    map[p] = count + 1;
                }

                return map;
            });

        var pointToEntityArea = entityAreas
            .Where(area => area.All(p => p.Effect))
            .Aggregate(new Dictionary<Location, Area[]>(), (map, area) =>
            {
                foreach (var p in area)
                {
                    map.Add(p.Location, area);
                }

                return map;
            });

        // GENERATE POSSIBLE BEACONS
        var possibleBeacons = possibleBeaconAreas
            .Select(collisionArea =>
            {
                var mid = collisionArea[4];

                var d = beaconSize + beaconEffectRadius * 2;
                var effectsGiven = Enumerable
                    .Range(0, d * d)
                    .Select(i => new Location(
                        x: mid.X + ((i % d) - (d / 2)),
                        y: mid.Y + ((i / d) - (d / 2))))
                    .Aggregate(new List<Area[]>(), (acc, p) =>
                    {
                        if (pointToEntityArea.TryGetValue(p, out var area))
                        {
                            if (!acc.Contains(area))
                            {
                                acc.Add(area);
                            }
                        }

                        return acc;
                    });

                var avgDistToEntities = effectsGiven.Select(p => p[4].Location).DefaultIfEmpty(mid).Average(p => p.GetManhattanDistance(mid));
                var nrOfOverlaps = collisionArea.Sum(p => pointToBeaconCount[p]);

                return new BeaconCandidate(
                    mid,
                    collisionArea,
                    effectsGiven.Count,
                    avgDistToEntities,
                    nrOfOverlaps);
            })
            .Where(c => c.EffectsGiven >= minAffectedEntities)
            .ToList();

        var pointToBeacons = possibleBeacons.Aggregate(new Dictionary<Location, List<BeaconCandidate>>(), (map, b) =>
        {
            foreach (var p in b.CollisionArea)
            {
                if (!map.TryGetValue(p, out var c))
                {
                    c = new List<BeaconCandidate>();
                    map.Add(p, c);
                }

                c.Add(b);
            }

            return map;
        });

        // GENERATE BEACONS
        var beacons = new List<BeaconCandidate>();
        while (possibleBeacons.Count > 0)
        {
            possibleBeacons
                .Sort((a, b) =>
                {
                    if (a.EffectsGiven == 1 || b.EffectsGiven == 1)
                    {
                        return b.AvgDistToEntities.CompareTo(a.AvgDistToEntities);
                    }

                    return a.NrOfOverlaps.CompareTo(b.NrOfOverlaps);
                });

            possibleBeacons.Sort((a, b) => b.EffectsGiven.CompareTo(a.EffectsGiven));

            var beacon = possibleBeacons[0];
            possibleBeacons.RemoveAt(0);
            beacons.Add(beacon);

            var toRemove = beacon.CollisionArea.SelectMany(p => pointToBeacons[p]).ToHashSet();
            possibleBeacons = possibleBeacons.Where(b => !toRemove.Contains(b)).ToList();
        }

        return beacons.Select(b => b.Mid).ToHashSet();
    }

    private record Area(Location Location, bool Effect);

    private record BeaconCandidate(Location Mid, List<Location> CollisionArea, int EffectsGiven, double AvgDistToEntities, int NrOfOverlaps);
}