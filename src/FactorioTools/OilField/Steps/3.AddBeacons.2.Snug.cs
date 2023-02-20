﻿using System.Reflection.Metadata.Ecma335;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddBeacons
{
    private static List<Location> AddBeacons_Snug(Context context)
    {
        var poweredEntities = context
            .CenterToTerminals
            .Keys
            .Select(c => new ProviderRecipient(c, PumpjackWidth, PumpjackHeight))
            .ToList();

        // We don't try to remove unused beacons here because there should not be any existing beacons at this point.
        (var candidateToCovered, var coveredEntities, var existingBeacons) = GetBeaconCandidateToCovered(
            context,
            poweredEntities,
            removeUnused: false);

        if (context.Options.ValidateSolution && existingBeacons.Count > 0)
        {
            throw new InvalidOperationException("There should not be any existing beacons.");
        }

        var candidateToInfo = GetCandidateToInfo(context, candidateToCovered, poweredEntities);

        // Visualizer.Show(context.Grid, candidateToCovered.Keys.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        var sorter = new SnugCandidateSorter(candidateToCovered, candidateToInfo);

#if USE_SHARED_INSTANCES
        var scopedCandidates = context.SharedInstances.LocationListA;
        var scopedCandidatesSet = context.SharedInstances.LocationSetA;
#else
        var scopedCandidates = new List<Location>();
        var scopedCandidatesSet = new HashSet<Location>();
#endif

        var beacons = new List<Location>();

        try
        {
            while (candidateToCovered.Count > 0)
            {
                var startingCandidate = candidateToCovered
                    .MinBy(pair =>
                    {
                        var info = candidateToInfo[pair.Key];
                        return (
                            beacons.Count > 0 ? beacons.Min(x => x.GetManhattanDistance(pair.Key)) : 0,
                            -pair.Value.TrueCount,
                            -info.EntityDistance,
                            info.MiddleDistance
                        );
                    })!.Key;

                scopedCandidates.Clear();
                scopedCandidates.Add(startingCandidate);
                scopedCandidatesSet.Clear();
                scopedCandidatesSet.Add(startingCandidate);

                while (scopedCandidates.Count > 0)
                {
                    var candidate = scopedCandidates[scopedCandidates.Count - 1];
                    scopedCandidates.RemoveAt(scopedCandidates.Count - 1);

                    if (!candidateToCovered.ContainsKey(candidate))
                    {
                        continue;
                    }

                    RemoveCandidates(
                        context.Grid,
                        candidate,
                        context.Options.BeaconWidth,
                        context.Options.BeaconHeight,
                        candidateToCovered);

                    beacons.Add(candidate);

                    AddNeighborsAndSort(
                        context,
                        candidateToCovered,
                        scopedCandidates,
                        scopedCandidatesSet,
                        sorter,
                        candidate);
                    
                    /*
                    Visualizer.Show(
                        context.Grid,
                        scopedCandidates.Concat(new[] { candidate }).Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)),
                        Array.Empty<DelaunatorSharp.IEdge>());
                    */
                }

                // Visualizer.Show(context.Grid, candidateToCovered.Keys.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
            }
        }
        finally
        {
#if USE_SHARED_INSTANCES
            scopedCandidates.Clear();
            scopedCandidatesSet.Clear();
#endif
        }

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        return beacons;
    }

    private static Dictionary<Location, CandidateInfo> GetCandidateToInfo(
        Context context,
        Dictionary<Location, CountedBitArray> candidateToCovered,
        List<ProviderRecipient> poweredEntities)
    {
        var candidateToInfo = new Dictionary<Location, CandidateInfo>(candidateToCovered.Count);
        foreach ((var candidate, var covered) in candidateToCovered)
        {
            var info = new CandidateInfo();
            info.EntityDistance = GetEntityDistance(poweredEntities, candidate, covered);
            info.MiddleDistance = candidate.GetEuclideanDistanceSquared(context.Grid.Middle);

            candidateToInfo.Add(candidate, info);
        }

        return candidateToInfo;
    }

    private class CandidateInfo : ICandidateInfo
    {
        public double EntityDistance;
        public int MiddleDistance;

        public void SetEntityDistance(double entityDistance)
        {
            EntityDistance = entityDistance;
        }
    }

    private static void AddNeighborsAndSort(
        Context context,
        Dictionary<Location, CountedBitArray> candidateToCovered,
        List<Location> scopedCandidates,
        HashSet<Location> scopedCandidatesSet,
        SnugCandidateSorter sorter,
        Location candidate)
    {
        var (_, _, _, _,
            overlapMinX,
            overlapMinY,
            overlapMaxX,
            overlapMaxY
        ) = GetProviderBounds(context.Grid, candidate, context.Options.BeaconWidth, context.Options.BeaconHeight);

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
                if (candidateToCovered.ContainsKey(location) && scopedCandidatesSet.Add(location))
                {
                    scopedCandidates.Add(location);
                }
            }
        }

        // bottom bound of the neighbor rectangle
        if (overlapMaxY + 1 == maxY)
        {
            for (var x = minX; x <= maxX; x++)
            {
                var location = new Location(x, maxY);
                if (candidateToCovered.ContainsKey(location) && scopedCandidatesSet.Add(location))
                {
                    scopedCandidates.Add(location);
                }
            }
        }

        // left bound of the neighbor rectangle, avoiding the corners
        if (overlapMinX - 1 == minX)
        {
            for (var y = minY + 1; y <= maxY - 1; y++)
            {
                var location = new Location(minX, y);
                if (candidateToCovered.ContainsKey(location) && scopedCandidatesSet.Add(location))
                {
                    scopedCandidates.Add(location);
                }
            }
        }

        // right bound of the neighbor rectangle, avoiding the corners
        if (overlapMaxX + 1 == maxX)
        {
            for (var y = minY + 1; y <= maxY - 1; y++)
            {
                var location = new Location(maxX, y);
                if (candidateToCovered.ContainsKey(location) && scopedCandidatesSet.Add(location))
                {
                    scopedCandidates.Add(location);
                }
            }
        }

        if (initialCount < scopedCandidates.Count)
        {
            scopedCandidates.Sort(initialCount, scopedCandidates.Count - initialCount, sorter);
        }
    }

    private class SnugCandidateSorter : IComparer<Location>
    {
        internal readonly Dictionary<Location, CountedBitArray> _candidateToCovered;
        internal readonly Dictionary<Location, CandidateInfo> _candidateToInfo;

        public SnugCandidateSorter(
            Dictionary<Location, CountedBitArray> candidateToCovered,
            Dictionary<Location, CandidateInfo> candidateToInfo)
        {
            _candidateToCovered = candidateToCovered;
            _candidateToInfo = candidateToInfo;
        }

        public int Compare(Location x, Location y)
        {
            int xi, yi, c;

            xi = _candidateToCovered[x].TrueCount;
            yi = _candidateToCovered[y].TrueCount;
            c = xi.CompareTo(yi);
            if (c != 0)
            {
                return c;
            }

            var xInfo = _candidateToInfo[x];
            var yInfo = _candidateToInfo[y];

            c = xInfo.EntityDistance.CompareTo(yInfo.EntityDistance);
            if (c != 0)
            {
                return c;
            }

            return yInfo.MiddleDistance.CompareTo(xInfo.MiddleDistance);
        }
    }
}