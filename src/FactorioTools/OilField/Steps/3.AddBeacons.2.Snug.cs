﻿using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddBeacons
{
    private static Dictionary<Location, BeaconCenter> AddBeacons_Snug(Context context)
    {
        var poweredEntities = context
            .CenterToTerminals
            .Keys
            .Select(c => new ProviderRecipient(c, PumpjackWidth, PumpjackHeight))
            .ToList();

        // We don't try to remove unused beacons here because there should not be any existing beacons at this point.
        (var candidateToCovered, var coveredEntities, var beacons) = GetBeaconCandidateToCovered(context, poweredEntities, removeUnused: false);

        if (context.Options.ValidateSolution && beacons.Count > 0)
        {
            throw new InvalidOperationException("There should not be any existing beacons.");
        }

        var candidateToMiddleDistance = candidateToCovered.ToDictionary(
            x => x.Key,
            x => x.Key.GetEuclideanDistance(context.Grid.Middle));

        var candidateToEntityDistance = GetCandidateToEntityDistance(poweredEntities, candidateToCovered);

        // Visualizer.Show(context.Grid, candidateToCovered.Keys.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());

#if USE_SHARED_INSTANCES
        var scopedCandidates = context.SharedInstances.LocationListA;
        var scopedCandidatesSet = context.SharedInstances.LocationSetA;
#else
        var scopedCandidates = new List<Location>();
        var scopedCandidatesSet = new HashSet<Location>();
#endif

        var sorter = new SnugCandidateSorter(
            beacons,
            candidateToCovered,
            candidateToEntityDistance,
            candidateToMiddleDistance);

        try
        {
            while (candidateToCovered.Count > 0)
            {
                var startingCandidate = candidateToCovered
                    .Keys
                    .MinBy(c => (
                        beacons.Count > 0 ? beacons.Keys.Min(x => x.GetManhattanDistance(c)) : 0,
                        -candidateToCovered[c].TrueCount,
                        -candidateToEntityDistance[c],
                        candidateToMiddleDistance[c]
                    ))!;

                scopedCandidates.Clear();
                scopedCandidates.Add(startingCandidate);
                scopedCandidatesSet.Clear();
                scopedCandidatesSet.Add(startingCandidate);

                while (scopedCandidates.Count > 0)
                {
                    var candidate = scopedCandidates[scopedCandidates.Count - 1];
                    scopedCandidates.RemoveAt(scopedCandidates.Count - 1);

                    var centerEntity = new BeaconCenter();

                    AddProvider(
                        context.Grid,
                        candidate,
                        centerEntity,
                        c => new BeaconSide(c),
                        context.Options.BeaconWidth,
                        context.Options.BeaconHeight,
                        candidateToCovered);

                    beacons.Add(candidate, centerEntity);

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

        // remove inelligable candidates
        scopedCandidates.RemoveAll(c => !candidateToCovered.ContainsKey(c));

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

        var lookup = scopedCandidates.ToDictionary(c => c, c => (
                    -sorter._candidateToCovered[c].TrueCount,
                    -sorter._candidateToEntityDistance[c],
                    sorter._candidateToMiddleDistance[c]
                )).OrderByDescending(p => p.Value).ToList();

        // Sort the candidates. We have to sort the whole list because the sort order of existing items in the scoped
        // candidates list may have also changed. Sort of any candidate can change when a beacon is added to the grid.
        scopedCandidates.Sort(sorter);
    }

    public class SnugCandidateSorter : IComparer<Location>
    {
        internal readonly Dictionary<Location, BeaconCenter> _beacons;
        internal readonly Dictionary<Location, CountedBitArray> _candidateToCovered;
        internal readonly Dictionary<Location, double> _candidateToEntityDistance;
        internal readonly Dictionary<Location, double> _candidateToMiddleDistance;

        public SnugCandidateSorter(
            Dictionary<Location, BeaconCenter> beacons,
            Dictionary<Location, CountedBitArray> candidateToCovered,
            Dictionary<Location, double> candidateToEntityDistance,
            Dictionary<Location, double> candidateToMiddleDistance)
        {
            _beacons = beacons;
            _candidateToCovered = candidateToCovered;
            _candidateToEntityDistance = candidateToEntityDistance;
            _candidateToMiddleDistance = candidateToMiddleDistance;
        }

        public int Compare(Location x, Location y)
        {
            int xi, yi, c;

            /*
            if (_beacons.Count > 0)
            {
                xi = _beacons.Keys.Min(c => c.GetManhattanDistance(x));
                yi = _beacons.Keys.Min(c => c.GetManhattanDistance(y));
                c = yi.CompareTo(xi);
                if (c != 0)
                {
                    return c;
                }
            }
            */

            xi = _candidateToCovered[x].TrueCount;
            yi = _candidateToCovered[y].TrueCount;
            c = xi.CompareTo(yi);
            if (c != 0)
            {
                return c;
            }

            var xd = _candidateToEntityDistance[x];
            var yd = _candidateToEntityDistance[y];
            c = xd.CompareTo(yd);
            if (c != 0)
            {
                return c;
            }

            xd = _candidateToMiddleDistance[x];
            yd = _candidateToMiddleDistance[y];
            return yd.CompareTo(xd);
        }
    }
}