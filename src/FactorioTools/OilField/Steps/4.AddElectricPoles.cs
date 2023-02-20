using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class AddElectricPoles
{
    private enum RetryStrategy
    {
        None = 0,
        RemoveUnpoweredEntities = 1,
        RemoveUnpoweredBeacons = 2,
        PreferUncoveredEntities = 3,
    }

    public static HashSet<Location>? Execute(Context context, bool avoidTerminals, bool retryWithUncovered)
    {
        HashSet<Location>? temporaryTerminals = null;
        if (avoidTerminals)
        {
            temporaryTerminals = new HashSet<Location>();

            foreach (var terminal in context.CenterToTerminals.Values.SelectMany(t => t).Select(t => t.Terminal))
            {
                if (context.Grid.IsEmpty(terminal))
                {
                    if (temporaryTerminals.Add(terminal))
                    {
                        context.Grid.AddEntity(terminal, new TemporaryEntity());
                    }
                }
            }   
        }

        // Visualizer.Show(context.Grid, poweredEntities.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.Center.X, c.Center.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        (var electricPoles, var poweredEntities) = AddElectricPolesAroundEntities(context, retryWithUncovered);
        if (electricPoles is null)
        {
            return null;
        }

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        ConnectElectricPoles(context, electricPoles);

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        RemoveExtraElectricPoles(context, poweredEntities, electricPoles);

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        // PruneNeighbors(context, electricPoles);

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        if (avoidTerminals)
        {
            foreach (var terminal in temporaryTerminals!)
            {
                context.Grid.RemoveEntity(terminal);
            }
        }

        return electricPoles.Keys.ToHashSet();
    }

    public static void VerifyAllEntitiesHasPower(Context context)
    {
        (var poweredEntities, _) = GetPoweredEntities(context);

        var electricPoleCenters = new List<Location>();
        foreach ((var entity, var location) in context.Grid.EntityToLocation)
        {
            if (entity is ElectricPoleCenter)
            {
                electricPoleCenters.Add(location);
            }
        }

        GetPoleCoverage(context, poweredEntities, electricPoleCenters);
    }

    private static (List<ProviderRecipient> PoweredEntities, bool HasBeacons) GetPoweredEntities(Context context)
    {
        var poweredEntities = new List<ProviderRecipient>();
        var hasBeacons = false;
        foreach ((var entity, var location) in context.Grid.EntityToLocation)
        {
            switch (entity)
            {
                case PumpjackCenter:
                    poweredEntities.Add(new ProviderRecipient(location, PumpjackWidth, PumpjackHeight));
                    break;
                case BeaconCenter:
                    poweredEntities.Add(new ProviderRecipient(location, context.Options.BeaconWidth, context.Options.BeaconHeight));
                    hasBeacons = true;
                    break;
            }
        }

        return (poweredEntities, hasBeacons);
    }

    private static void RemoveExtraElectricPoles(Context context, List<ProviderRecipient> poweredEntities, Dictionary<Location, ElectricPoleCenter> electricPoles)
    {
        (var poleCenterToCoveredCenters, var coveredCenterToPoleCenters) = GetPoleCoverage(context, poweredEntities, electricPoles.Keys);

        var removeCandidates = coveredCenterToPoleCenters
            .Where(p => p.Value.Count > 2) // Consider electric poles covering pumpjacks that are covered by at least one other electric pole.
            .SelectMany(p => p.Value)
            .Concat(poleCenterToCoveredCenters.Where(p => p.Value.Count == 0).Select(p => p.Key)) // Consider electric poles not covering any pumpjack.
            .Except(coveredCenterToPoleCenters.Where(p => p.Value.Count == 1).SelectMany(p => p.Value)) // Exclude electric poles covering pumpjacks that are only covered by one pole.
            .ToHashSet();

        while (removeCandidates.Count > 0)
        {
            var center = removeCandidates.First();
            var centerEntity = electricPoles[center];
            if (ArePolesConnectedWithout(electricPoles, centerEntity))
            {
                electricPoles.Remove(center);
                RemoveProvider(context.Grid, center, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight);

                foreach (var coveredCenter in poleCenterToCoveredCenters[center])
                {
                    var poleCenters = coveredCenterToPoleCenters[coveredCenter];
                    poleCenters.Remove(center);
                    if (poleCenters.Count == 1)
                    {
                        removeCandidates.ExceptWith(poleCenters);
                    }
                }

                poleCenterToCoveredCenters.Remove(center);
            }

            removeCandidates.Remove(center);
        }
    }

    private static (Dictionary<Location, HashSet<Location>> PoleCenterToCoveredCenters, Dictionary<Location, HashSet<Location>> CoveredCenterToPoleCenters) GetPoleCoverage(
        Context context,
        List<ProviderRecipient> poweredEntities,
        IEnumerable<Location> electricPoleCenters)
    {
        var poleCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
            context.Grid,
            context.Options.ElectricPoleWidth,
            context.Options.ElectricPoleHeight,
            context.Options.ElectricPoleSupplyWidth,
            context.Options.ElectricPoleSupplyHeight,
            electricPoleCenters,
            includePumpjacks: true,
            includeBeacons: true);

        var coveredCenterToPoleCenters = poleCenterToCoveredCenters
            .SelectMany(p => p.Value.Select(c => (PoleCenter: p.Key, RecipientCenter: c)))
            .GroupBy(p => p.RecipientCenter, p => p.PoleCenter)
            .ToDictionary(g => g.Key, g => g.ToHashSet());

        if (coveredCenterToPoleCenters.Count != poweredEntities.Count)
        {
            var uncoveredCenters = poweredEntities
                .Select(e => e.Center)
                .Except(coveredCenterToPoleCenters.Keys)
                .ToList();
            // Visualizer.Show(context.Grid, uncoveredCenters.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());
            throw new InvalidOperationException("Not all powered entities are covered by an electric pole.");
        }

        return (PoleCenterToCoveredCenters: poleCenterToCoveredCenters, CoveredCenterToPoleCenters: coveredCenterToPoleCenters);
    }

    private static bool ArePolesConnectedWithout(Dictionary<Location, ElectricPoleCenter> electricPoles, ElectricPoleCenter except)
    {
        var queue = new Queue<ElectricPoleCenter>();
        queue.Enqueue(electricPoles.Values.Where(x => x != except).First());
        var discovered = new HashSet<ElectricPoleCenter>();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == except)
            {
                continue;
            }

            if (discovered.Add(current))
            {
                foreach (var neighbor in current.Neighbors)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return discovered.Count == electricPoles.Count - 1;
    }

    private static void ConnectExistingElectricPoles(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles, Location center, ElectricPoleCenter centerEntity)
    {
        foreach ((var other, var otherCenter) in electricPoles)
        {
            if (center == other)
            {
                continue;
            }

            if (AreElectricPolesConnected(center, other, context.Options))
            {
                centerEntity.AddNeighbor(otherCenter);
            }
        }
    }

    public static bool AreElectricPolesConnected(Location a, Location b, Options options)
    {
        return GetElectricPoleDistanceSquared(a, b, options) <= options.ElectricPoleWireReachSquared;
    }

    private static int GetElectricPoleDistanceSquared(Location a, Location b, Options options)
    {
        var offsetX = (options.ElectricPoleWidth - 1) / 2;
        var offsetY = (options.ElectricPoleHeight - 1) / 2;

        return b.GetEuclideanDistanceSquared(a.X + offsetX, a.Y + offsetY);
    }

    private static (Dictionary<Location, ElectricPoleCenter>? ElectricPoles, List<ProviderRecipient> PoweredEntities) AddElectricPolesAroundEntities(
        Context context,
        bool retryWithUncovered)
    {
        var retryStrategy = retryWithUncovered ? RetryStrategy.PreferUncoveredEntities : RetryStrategy.None;
        CountedBitArray? entitiesToPowerFirst = null;

        while (true)
        {
            // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

            (var poweredEntities, var hasBeacons) = GetPoweredEntities(context);
            (var electricPoles, var electricPoleList, var coveredEntities) = AddElectricPolesAroundEntities(
                context,
                poweredEntities,
                entitiesToPowerFirst);

            // Visualizer.Show(context.Grid, poweredEntities.Where((e, i) => !coveredEntities[i]).Select(e => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(e.Center.X, e.Center.Y)), Array.Empty<DelaunatorSharp.IEdge>());

            if (retryStrategy == RetryStrategy.None || electricPoles is not null)
            {
                return (electricPoles, poweredEntities);
            }

            // Visualizer.Show(context.Grid, poweredEntities.Where((e, i) => !coveredEntities[i]).Select(e => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(e.Center.X, e.Center.Y)), Array.Empty<DelaunatorSharp.IEdge>());

            // Console.WriteLine("Applying retry strategy " + retryStrategy);

            for (var i = 0; i < electricPoleList.Count; i++)
            {
                var center = electricPoleList[i];
                RemoveProvider(context.Grid, center, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight);
            }

            if (retryStrategy == RetryStrategy.PreferUncoveredEntities)
            {
                entitiesToPowerFirst = coveredEntities;
                entitiesToPowerFirst.Not();
            }
            else if (retryStrategy == RetryStrategy.RemoveUnpoweredBeacons)
            {
                var centersToPowerFirst = new HashSet<Location>();
                for (var i = poweredEntities.Count - 1; i >= 0; i--)
                {
                    var entity = poweredEntities[i];
                    if (!coveredEntities[i])
                    {
                        if (context.Grid[entity.Center] is BeaconCenter)
                        {
                            poweredEntities.RemoveAt(i);
                            RemoveProvider(context.Grid, entity.Center, entity.Width, entity.Height);
                        }
                        else
                        {
                            centersToPowerFirst.Add(entity.Center);
                        }
                    }
                }

                if (centersToPowerFirst.Count == 0)
                {
                    entitiesToPowerFirst = null;
                }
                else
                {
                    entitiesToPowerFirst = new CountedBitArray(poweredEntities.Count);
                    for (var i = 0; centersToPowerFirst.Count > 0 && i < poweredEntities.Count; i++)
                    {
                        if (centersToPowerFirst.Remove(poweredEntities[i].Center))
                        {
                            entitiesToPowerFirst[i] = true;
                        }
                    }
                }
            }
            else if (retryStrategy == RetryStrategy.RemoveUnpoweredEntities)
            {
                for (var i = poweredEntities.Count - 1; i >= 0; i--)
                {
                    var entity = poweredEntities[i];
                    if (!coveredEntities[i])
                    {
                        var shouldRemove = retryStrategy == RetryStrategy.RemoveUnpoweredEntities
                            || (context.Grid[entity.Center] is BeaconCenter && retryStrategy == RetryStrategy.RemoveUnpoweredBeacons);

                        if (shouldRemove)
                        {
                            poweredEntities.RemoveAt(i);
                            RemoveProvider(context.Grid, entity.Center, entity.Width, entity.Height);
                        }
                    }
                }

                entitiesToPowerFirst = null;
            }

            // Visualizer.Show(context.Grid, poweredEntities.Where((e, i) => !coveredEntities[i]).Select(e => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(e.Center.X, e.Center.Y)), Array.Empty<DelaunatorSharp.IEdge>());

            retryStrategy--;
            if (retryStrategy == RetryStrategy.RemoveUnpoweredBeacons && !hasBeacons)
            {
                retryStrategy--;
            }
        }
    }

    private class CandidateFactory : ICandidateFactory<ElectricPoleCandidateInfo>
    {
        public static CandidateFactory Instance { get; } = new CandidateFactory();

        public ElectricPoleCandidateInfo Create(CountedBitArray covered)
        {
            return new ElectricPoleCandidateInfo(covered);
        }
    }
    
    private static (Dictionary<Location, ElectricPoleCenter>? ElectricPoles, List<Location> ElectricPoleList, CountedBitArray CoveredEntities) AddElectricPolesAroundEntities(
        Context context,
        List<ProviderRecipient> poweredEntities,
        CountedBitArray? entitiesToPowerFirst)
    {
        (var allCandidateToInfo, var coveredEntities, var electricPoles) = GetElectricPoleCandidateToCovered(
            context,
            poweredEntities,
            CandidateFactory.Instance,
            removeUnused: true);

        var electricPoleList = electricPoles.Keys.ToList();

        PopulateCandidateToInfo(context, allCandidateToInfo, entitiesToPowerFirst, poweredEntities, electricPoleList);

        var coveredToCandidates = new Dictionary<int, Dictionary<Location, ElectricPoleCandidateInfo>>(coveredEntities.Count);
        for (var i = 0; i < coveredEntities.Count; i++)
        {
            var candidates = new Dictionary<Location, ElectricPoleCandidateInfo>();
            foreach ((var candidate, var info) in allCandidateToInfo)
            {
                if (info.Covered[i])
                {
                    candidates.Add(candidate, info);
                }
            }

            coveredToCandidates.Add(i, candidates);
        }

        var allSubsets = new Queue<SortedBatches<ElectricPoleCandidateInfo>>();

        IComparer<ElectricPoleCandidateInfo> sorter;
        if (entitiesToPowerFirst is null)
        {
            sorter = CandidateComparerForSameCoveredCount.Instance;
        }
        else
        {
            sorter = CandidateComparerForSamePriorityPowered.Instance;
            allSubsets.Enqueue(new SortedBatches<ElectricPoleCandidateInfo>(
                allCandidateToInfo
                    .GroupBy(p => p.Value.PriorityPowered)
                    .Select(g => KeyValuePair.Create(g.Key, g.ToDictionary(p => p.Key, p => p.Value))),
                ascending: false));
        }

        var coveredCountBatches = new SortedBatches<ElectricPoleCandidateInfo>(
            allCandidateToInfo
                .GroupBy(p => p.Value.Covered.TrueCount)
                .Select(g => KeyValuePair.Create(g.Key, g.ToDictionary(p => p.Key, p => p.Value))),
            ascending: false);

        allSubsets.Enqueue(coveredCountBatches);

        var roundedReach = (int)Math.Ceiling(context.Options.ElectricPoleWireReach);
        Dictionary<Location, ElectricPoleCandidateInfo>? candidateToInfo = null;

        while (coveredEntities.Any(false))
        {
            if (candidateToInfo is null)
            {
                if (allSubsets.Count == 0)
                {
                    // There are not candidates or the candidates do not fit. No solution exists given the current grid (e.g.
                    // existing pipe placement eliminates all electric pole options).
                    return (null, electricPoleList, coveredEntities);
                }

                var subsets = allSubsets.Peek();
                if (subsets.Queue.Count == 0
                    || (allSubsets.Count > 1 && subsets.Queue.Count == 1))
                {
                    allSubsets.Dequeue();
                    continue;
                }

                candidateToInfo = subsets.Queue.Peek();
                if (candidateToInfo.Count == 0)
                {
                    candidateToInfo = null;
                    subsets.Queue.Dequeue();
                    continue;
                }
            }

            (var candidate, var candidateInfo) = candidateToInfo.MinBy(x => x.Value, sorter)!;

            if (!allCandidateToInfo.ContainsKey(candidate))
            {
                candidateToInfo.Remove(candidate);

                if (candidateToInfo.Count == 0)
                {
                    candidateToInfo = null;
                }

                continue;
            }

            if (context.Options.ValidateSolution)
            {
                var covered = candidateInfo.Covered;
                var isSubsetOf = true;
                for (var i = 0; i < poweredEntities.Count && isSubsetOf; i++)
                {
                    if (covered[i])
                    {
                        isSubsetOf = coveredEntities[i];
                    }
                }

                if (isSubsetOf)
                {
                    // Visualizer.Show(context.Grid, new[] { candidate }.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());
                    throw new InvalidOperationException($"Candidate {candidate} should have been eliminated.");
                }
            }

            var centerEntity = new ElectricPoleCenter();

            AddProviderAndUpdateCandidateState(
                context.Grid,
                context.SharedInstances,
                candidate,
                candidateInfo,
                centerEntity,
                c => new ElectricPoleSide(c),
                context.Options.ElectricPoleWidth,
                context.Options.ElectricPoleHeight,
                poweredEntities,
                coveredEntities,
                coveredToCandidates,
                allCandidateToInfo,
                candidateToInfo,
                coveredCountBatches);

            // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

            electricPoles.Add(candidate, centerEntity);
            electricPoleList.Add(candidate);

            UpdateCandidateInfo(context, allCandidateToInfo, roundedReach, candidate);

            if (candidateToInfo.Count == 0)
            {
                candidateToInfo = null;
                continue;
            }

            /*
            var test = GetCandidateToOthersConnected(context, candidateToCovered, electricPoleList);

            var keysOnlyInTest = test.Keys.Except(candidateToOthersConnected.Keys).ToList();
            var keysOnlyInRunning = candidateToOthersConnected.Keys.Except(test.Keys).ToList();
            var keysInBoth = test.Keys.Intersect(candidateToOthersConnected.Keys).ToList();
            var keysWithDifferentValues = keysInBoth.Where(x => candidateToOthersConnected[x] != test[x]).ToList();
            var testDiff = keysWithDifferentValues.ToDictionary(x => x, x => test[x]);
            var runningDiff = keysWithDifferentValues.ToDictionary(x => x, x => candidateToOthersConnected[x]);

            if (keysOnlyInTest.Count > 0 || keysWithDifferentValues.Count > 0)
            {
                throw new InvalidOperationException("The mapping from candidate to others connected count was not updated properly.");
            }
            */

            // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());
        }

        foreach ((var center, var centerEntity) in electricPoles)
        {
            ConnectExistingElectricPoles(context, electricPoles, center, centerEntity);
        }

        return (electricPoles, electricPoleList, coveredEntities);
    }

    private class ElectricPoleCandidateInfo : CandidateInfo
    {
        public ElectricPoleCandidateInfo(CountedBitArray covered) : base(covered)
        {
        }

        public int PriorityPowered;
        public int OthersConnected;
        public int PoleDistance;
        public int MiddleDistance;
    }

    private static void PopulateCandidateToInfo(
        Context context,
        Dictionary<Location, ElectricPoleCandidateInfo> candidateToInfo,
        CountedBitArray? entitiesToPowerFirst,
        List<ProviderRecipient> poweredEntities,
        List<Location> electricPoleList)
    {
        foreach ((var candidate, var info) in candidateToInfo)
        {
            if (entitiesToPowerFirst is not null)
            {
                info.PriorityPowered = new CountedBitArray(entitiesToPowerFirst).And(info.Covered).TrueCount;
                if (info.PriorityPowered > 0)
                {

                }
            }

            var othersConnected = 0;
            var min = int.MaxValue;
            for (var i = 0; i < electricPoleList.Count; i++)
            {
                if (AreElectricPolesConnected(candidate, electricPoleList[i], context.Options))
                {
                    othersConnected++;
                }

                var val = electricPoleList[i].GetEuclideanDistanceSquared(candidate);
                if (val < min)
                {
                    min = val;
                }
            }

            info.OthersConnected = othersConnected;
            info.PoleDistance = min;

            info.EntityDistance = GetEntityDistance(poweredEntities, candidate, info.Covered);
            info.MiddleDistance = candidate.GetEuclideanDistanceSquared(context.Grid.Middle);
        }
    }

    private class CandidateComparerForSameCoveredCount : IComparer<ElectricPoleCandidateInfo>
    {
        public static CandidateComparerForSameCoveredCount Instance { get; } = new CandidateComparerForSameCoveredCount();

        public int Compare(ElectricPoleCandidateInfo? x, ElectricPoleCandidateInfo? y)
        {
            return CompareWithoutPriorityPowered(x!, y!);
        }

        public static int CompareWithoutPriorityPowered(
            ElectricPoleCandidateInfo x,
            ElectricPoleCandidateInfo y)
        {
            var xi = x.OthersConnected > 0 ? x.OthersConnected : int.MaxValue;
            var yi = y.OthersConnected > 0 ? y.OthersConnected : int.MaxValue;
            var c = xi.CompareTo(yi);
            if (c != 0)
            {
                return c;
            }

            xi = x.OthersConnected > 0 ? 0 : x.PoleDistance;
            yi = y.OthersConnected > 0 ? 0 : y.PoleDistance;
            c = xi.CompareTo(yi);
            if (c != 0)
            {
                return c;
            }

            c = x.EntityDistance.CompareTo(y.EntityDistance);
            if (c != 0)
            {
                return c;
            }

            return x.MiddleDistance.CompareTo(y.MiddleDistance);
        }
    }

    private class CandidateComparerForSamePriorityPowered : IComparer<ElectricPoleCandidateInfo>
    {
        public static CandidateComparerForSamePriorityPowered Instance { get; } = new CandidateComparerForSamePriorityPowered();

        public int Compare(ElectricPoleCandidateInfo? x, ElectricPoleCandidateInfo? y)
        {
            var c = y!.Covered.TrueCount.CompareTo(x!.Covered.TrueCount);
            if (c != 0)
            {
                return c;
            }

            return CandidateComparerForSameCoveredCount.CompareWithoutPriorityPowered(x, y);
        }
    }

    private static void UpdateCandidateInfo(
        Context context,
        Dictionary<Location, ElectricPoleCandidateInfo> candidateToInfo,
        int roundedReach,
        Location candidate)
    {
        var minX = candidate.X - roundedReach;
        var maxX = candidate.X + roundedReach;
        var minY = candidate.Y - roundedReach;
        var maxY = candidate.Y + roundedReach;

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var other = new Location(x, y);
                var distanceSquared = GetElectricPoleDistanceSquared(candidate, other, context.Options);

                if (candidateToInfo.TryGetValue(other, out var info))
                {
                    if (distanceSquared <= context.Options.ElectricPoleWireReachSquared)
                    {
                        info.OthersConnected++;
                    }

                    if (distanceSquared < info.PoleDistance)
                    {
                        info.PoleDistance = distanceSquared;
                    }
                }
            }
        }
    }

    private static ElectricPoleCenter AddElectricPole(
        Context context,
        Dictionary<Location, ElectricPoleCenter> electricPoles,
        Location center)
    {
        var centerEntity = new ElectricPoleCenter();

        AddProvider(
            context.Grid,
            center,
            centerEntity,
            c => new ElectricPoleSide(c),
            context.Options.ElectricPoleWidth,
            context.Options.ElectricPoleHeight);

        electricPoles.Add(center, centerEntity);
        ConnectExistingElectricPoles(context, electricPoles, center, centerEntity);

        return centerEntity;
    }

    private static void ConnectElectricPoles(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles)
    {
        var groups = GetElectricPoleGroups(context, electricPoles);

        while (groups.Count > 1)
        {
            var closest = PointsToLines(electricPoles.Keys)
                .Select(e => new
                {
                    Endpoints = e,
                    GroupA = groups.Single(g => g.Contains(e.A)),
                    GroupB = groups.Single(g => g.Contains(e.B)),
                    DistanceSquared = GetElectricPoleDistanceSquared(e.A, e.B, context.Options),
                })
                .Where(c => c.GroupA != c.GroupB)
                .Where(c => c.DistanceSquared > context.Options.ElectricPoleWireReachSquared)
                .MinBy(c => c.DistanceSquared);

            if (closest is null)
            {
                throw new NotImplementedException();
            }

            AddSinglePoleForConnection(context, electricPoles, groups, Math.Sqrt(closest.DistanceSquared), closest.Endpoints);
        }
    }

    private static void AddSinglePoleForConnection(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles, List<HashSet<Location>> groups, double distance, Endpoints endpoints)
    {
        var segments = (int)Math.Ceiling(distance / context.Options.ElectricPoleWireReach);
        var idealLine = BresenhamsLine.GetPath(endpoints.A, endpoints.B);
        var idealIndex = idealLine.Count / segments;
        if (!AreElectricPolesConnected(idealLine[0], idealLine[idealIndex], context.Options))
        {
            idealIndex--;
        }
        var idealPoint = idealLine[idealIndex];

#if USE_SHARED_INSTANCES
        var candidates = context.SharedInstances.LocationQueue;
        var attempted = context.SharedInstances.LocationSetA;
#else
        var candidates = new Queue<Location>();
        var attempted = new HashSet<Location>();
#endif

        Location? selectedPoint = null;
        try
        {
            candidates.Enqueue(idealPoint);
            attempted.Add(idealPoint);

            Span<Location> adjacent = stackalloc Location[4];
            while (candidates.Count > 0)
            {
                var candidate = candidates.Dequeue();
                if (DoesProviderFit(context.Grid, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight, candidate)
                    && IsProviderInBounds(context.Grid, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight, candidate))
                {
                    selectedPoint = candidate;
                    break;
                }

                context.Grid.GetAdjacent(adjacent, candidate);
                for (var i = 0; i < adjacent.Length; i++)
                {
                    if (adjacent[i].IsValid
                        && AreElectricPolesConnected(idealLine[0], adjacent[i], context.Options)
                        && attempted.Add(adjacent[i]))
                    {
                        candidates.Enqueue(adjacent[i]);
                    }
                }
            }
        }
        finally
        {
#if USE_SHARED_INSTANCES
            candidates.Clear();
            attempted.Clear();
#endif
        }

        if (!selectedPoint.HasValue)
        {
            throw new InvalidOperationException("Could not find a pole that can be connected");
        }
        var center = AddElectricPole(context, electricPoles, selectedPoint.Value);
        var connectedGroups = groups.Where(g => g.Intersect(center.Neighbors.Select(n => context.Grid.EntityToLocation[n])).Any()).ToList();

        if (connectedGroups.Count == 0)
        {
            throw new NotImplementedException();
        }

        connectedGroups[0].Add(selectedPoint.Value);
        for (var i = 1; i < connectedGroups.Count; i++)
        {
            connectedGroups[0].UnionWith(connectedGroups[i]);
            groups.Remove(connectedGroups[i]);
        }

        // Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());
    }

    private static List<HashSet<Location>> GetElectricPoleGroups(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles)
    {
        var groups = new List<HashSet<Location>>();
        var remaining = new HashSet<Location>(electricPoles.Keys);
        while (remaining.Count > 0)
        {
            var current = remaining.First();
            remaining.Remove(current);

            var entities = new HashSet<ElectricPoleCenter>();
            var explore = new Queue<ElectricPoleCenter>();
            explore.Enqueue(electricPoles[current]);

            while (explore.Count > 0)
            {
                var entity = explore.Dequeue();
                if (entities.Add(entity))
                {
                    foreach (var neighbor in entity.Neighbors)
                    {
                        explore.Enqueue(neighbor);
                    }
                }
            }

            var group = entities.Select(e => context.Grid.EntityToLocation[e]).ToHashSet();
            remaining.ExceptWith(group);
            groups.Add(group);
        }

        return groups;
    }
}
