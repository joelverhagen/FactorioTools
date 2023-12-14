using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

public static partial class PlanBeacons
{
    private static (List<Location> Beacons, int Effects) AddBeaconsSnug(Context context)
    {
        var poweredEntities = context
            .CenterToTerminals
            .Keys
            .Select(c => new ProviderRecipient(c, PumpjackWidth, PumpjackHeight))
            .ToList();

        // We don't try to remove unused beacons here because there should not be any existing beacons at this point.
        (var candidateToInfo, var coveredEntities, var existingBeacons) = GetBeaconCandidateToCovered(
            context,
            poweredEntities,
            CandidateFactory.Instance,
            removeUnused: false);

        Validate.NoExistingBeacons(context, existingBeacons);

        PopulateCandidateToInfo(context, candidateToInfo, poweredEntities);

        // Visualizer.Show(context.Grid, candidateToInfo.Keys.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        var sorter = new SnugCandidateSorter();

        var scopedCandidates = new List<KeyValuePair<Location, BeaconCandidateInfo>>();
        var scopedCandidatesSet = new Dictionary<Location, BeaconCandidateInfo>();

        Dictionary<int, Dictionary<Location, BeaconCandidateInfo>>? coveredToCandidates = null;
        if (!context.Options.OverlapBeacons)
        {
            coveredToCandidates = GetCoveredToCandidates(candidateToInfo, coveredEntities);
        }

        var beacons = new List<Location>();
        var effects = 0;

        while (candidateToInfo.Count > 0)
        {
            var pair = candidateToInfo
                .MinBy(pair =>
                {
                    return (
                        beacons.Count > 0 && context.Options.OverlapBeacons ? beacons.Min(x => x.GetManhattanDistance(pair.Key)) : 0,
                        -pair.Value.Covered.TrueCount,
                        -pair.Value.EntityDistance,
                        pair.Value.MiddleDistance
                    );
                })!;

            scopedCandidates.Clear();
            scopedCandidates.Add(pair);
            scopedCandidatesSet.Clear();
            scopedCandidatesSet.Add(pair.Key, pair.Value);

            while (scopedCandidates.Count > 0)
            {
                (var candidate, var info) = scopedCandidates[scopedCandidates.Count - 1];
                scopedCandidates.RemoveAt(scopedCandidates.Count - 1);

                if (!candidateToInfo.ContainsKey(candidate))
                {
                    continue;
                }

                if (context.Options.OverlapBeacons)
                {
                    RemoveOverlappingCandidates(
                        context.Grid,
                        candidate,
                        context.Options.BeaconWidth,
                        context.Options.BeaconHeight,
                        candidateToInfo);
                }
                else
                {
                    AddProviderAndPreventMultipleProviders(
                        context.Grid,
                        context.SharedInstances,
                        candidate,
                        info,
                        context.Options.BeaconWidth,
                        context.Options.BeaconHeight,
                        poweredEntities,
                        coveredEntities,
                        coveredToCandidates!,
                        candidateToInfo);
                }

                beacons.Add(candidate);
                effects += info.CoveredCount;
                // Console.WriteLine($"{candidate} --- {info.CoveredCount}");

                AddNeighborsAndSort(
                    context,
                    candidateToInfo,
                    scopedCandidates,
                    scopedCandidatesSet,
                    sorter,
                    candidate);

                /*
                var clone = new PipeGrid(context.Grid);
                foreach (var beaconCenter in beacons)
                {
                    AddProvider(clone, beaconCenter, new BeaconCenter(), c => new BeaconSide(c), context.Options.BeaconWidth, context.Options.BeaconHeight);
                }
                Visualizer.Show(
                    clone,
                    candidateToInfo.Keys.Concat(new[] { candidate }).Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)),
                    Array.Empty<DelaunatorSharp.IEdge>());
                */
            }

            // Visualizer.Show(context.Grid, candidateToCovered.Keys.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
        }

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        return (beacons, effects);
    }

    private class CandidateFactory : ICandidateFactory<BeaconCandidateInfo>
    {
        public static CandidateFactory Instance { get; } = new CandidateFactory();

        public BeaconCandidateInfo Create(CountedBitArray covered)
        {
            return new BeaconCandidateInfo(covered);
        }
    }

    private static void PopulateCandidateToInfo(
        Context context,
        Dictionary<Location, BeaconCandidateInfo> candidateToInfo,
        List<ProviderRecipient> poweredEntities)
    {
        foreach ((var candidate, var info) in candidateToInfo)
        {
            info.EntityDistance = GetEntityDistance(poweredEntities, candidate, info.Covered);
            info.MiddleDistance = candidate.GetEuclideanDistanceSquared(context.Grid.Middle);
            info.CoveredCount = info.Covered.TrueCount;
        }
    }

    private class BeaconCandidateInfo : CandidateInfo
    {
        public BeaconCandidateInfo(CountedBitArray covered) : base(covered)
        {
        }

        public int MiddleDistance;
        public int CoveredCount;
    }

    private static void AddNeighborsAndSort(
        Context context,
        Dictionary<Location, BeaconCandidateInfo> candidateToInfo,
        List<KeyValuePair<Location, BeaconCandidateInfo>> scopedCandidates,
        Dictionary<Location, BeaconCandidateInfo> scopedCandidatesSet,
        SnugCandidateSorter sorter,
        Location candidate)
    {
        var (
            overlapMinX,
            overlapMinY,
            overlapMaxX,
            overlapMaxY
        ) = GetProviderOverlapBounds(context.Grid, candidate, context.Options.BeaconWidth, context.Options.BeaconHeight);

        var minX = Math.Max((context.Options.BeaconWidth - 1) / 2, overlapMinX - 1);
        var minY = Math.Max((context.Options.BeaconHeight - 1) / 2, overlapMinY - 1);
        var maxX = Math.Min(context.Grid.Width - (context.Options.BeaconWidth / 2) - 1, overlapMaxX + 1);
        var maxY = Math.Min(context.Grid.Height - (context.Options.BeaconHeight / 2) - 1, overlapMaxY + 1);

        var initialCount = scopedCandidates.Count;

        // top bound of the neighbor rectangle
        if (overlapMinY - 1 == minY)
        {
            for (var x = minX; x <= maxX; x++)
            {
                var location = new Location(x, minY);
                if (candidateToInfo.TryGetValue(location, out var info) && scopedCandidatesSet.TryAdd(location, info))
                {
                    scopedCandidates.Add(KeyValuePair.Create(location, info));
                }
            }
        }

        // bottom bound of the neighbor rectangle
        if (overlapMaxY + 1 == maxY)
        {
            for (var x = minX; x <= maxX; x++)
            {
                var location = new Location(x, maxY);
                if (candidateToInfo.TryGetValue(location, out var info) && scopedCandidatesSet.TryAdd(location, info))
                {
                    scopedCandidates.Add(KeyValuePair.Create(location, info));
                }
            }
        }

        // left bound of the neighbor rectangle, avoiding the corners
        if (overlapMinX - 1 == minX)
        {
            for (var y = minY + 1; y <= maxY - 1; y++)
            {
                var location = new Location(minX, y);
                if (candidateToInfo.TryGetValue(location, out var info) && scopedCandidatesSet.TryAdd(location, info))
                {
                    scopedCandidates.Add(KeyValuePair.Create(location, info));
                }
            }
        }

        // right bound of the neighbor rectangle, avoiding the corners
        if (overlapMaxX + 1 == maxX)
        {
            for (var y = minY + 1; y <= maxY - 1; y++)
            {
                var location = new Location(maxX, y);
                if (candidateToInfo.TryGetValue(location, out var info) && scopedCandidatesSet.TryAdd(location, info))
                {
                    scopedCandidates.Add(KeyValuePair.Create(location, info));
                }
            }
        }

        if (initialCount < scopedCandidates.Count)
        {
            scopedCandidates.Sort(initialCount, scopedCandidates.Count - initialCount, sorter);
        }
    }

    private class SnugCandidateSorter : IComparer<KeyValuePair<Location, BeaconCandidateInfo>>
    {
        public static SnugCandidateSorter Instance { get; } = new SnugCandidateSorter();

        public int Compare(KeyValuePair<Location, BeaconCandidateInfo> x, KeyValuePair<Location, BeaconCandidateInfo> y)
        {
            var c = x.Value.Covered.TrueCount.CompareTo(y.Value.Covered.TrueCount);
            if (c != 0)
            {
                return c;
            }

            c = x.Value.EntityDistance.CompareTo(y.Value.EntityDistance);
            if (c != 0)
            {
                return c;
            }

            return y.Value.MiddleDistance.CompareTo(x.Value.MiddleDistance);
        }
    }
}