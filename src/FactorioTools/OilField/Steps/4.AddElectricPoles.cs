using System.Collections;
using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class AddElectricPoles
{
    public static HashSet<Location>? Execute(Context context, bool avoidTerminals)
    {
        var temporaryTerminals = new HashSet<Location>();
        if (avoidTerminals)
        {
            foreach (var terminal in context.CenterToTerminals.Values.SelectMany(t => t).Select(t => t.Terminal))
            {
                if (context.Grid.IsEmpty(terminal))
                {
                    if (temporaryTerminals.Add(terminal))
                    {
                        context.Grid.AddEntity(terminal, new Terminal());
                    }
                }
            }
        }

        var poweredEntities = new List<ProviderRecipient>();
        foreach ((var entity, var location) in context.Grid.EntityToLocation)
        {
            switch (entity)
            {
                case PumpjackCenter:
                    poweredEntities.Add(new ProviderRecipient(location, Width: 3, Height: 3));
                    break;
                case BeaconCenter:
                    poweredEntities.Add(new ProviderRecipient(location, context.Options.BeaconWidth, context.Options.BeaconHeight));
                    break;
            }
        }

        var electricPoles = AddElectricPolesAroundEntities(context, poweredEntities);
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
            foreach (var terminal in temporaryTerminals)
            {
                context.Grid.RemoveEntity(terminal);
            }
        }

        return electricPoles.Keys.ToHashSet();
    }

    private static void PruneNeighbors(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles)
    {
        var graph = new Dictionary<Location, Dictionary<Location, double>>();
        foreach ((var location, var center) in electricPoles)
        {
            var neighbors = new Dictionary<Location, double>();
            foreach (var neighbor in center.Neighbors)
            {
                var neighborLocation = context.Grid.EntityToLocation[neighbor];
                neighbors.Add(neighborLocation, GetElectricPoleDistance(location, neighborLocation, context.Options));
            }

            graph.Add(location, neighbors);
        }

        var firstNode = graph.Keys.MinBy(context.Grid.Middle.GetEuclideanDistance)!;
        var mst = Prims.GetMinimumSpanningTree(graph, firstNode, digraph: false);

        foreach ((var location, var neighbors) in mst)
        {
            var center = electricPoles[location];
            center.ClearNeighbors();
            foreach (var neighbor in neighbors)
            {
                var neighborCenter = electricPoles[neighbor];
                center.AddNeighbor(neighborCenter);
            }
        }
    }

    private static void RemoveExtraElectricPoles(Context context, List<ProviderRecipient> poweredEntities, Dictionary<Location, ElectricPoleCenter> electricPoles)
    {
        var poleCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
            context.Grid,
            context.Options.ElectricPoleWidth,
            context.Options.ElectricPoleHeight,
            context.Options.ElectricPoleSupplyWidth,
            context.Options.ElectricPoleSupplyHeight,
            electricPoles.Keys);

        var coveredCenterToPoleCenters = poleCenterToCoveredCenters
            .SelectMany(p => p.Value.Select(c => (PoleCenter: p.Key, PumpjackCenter: c)))
            .GroupBy(p => p.PumpjackCenter, p => p.PoleCenter)
            .ToDictionary(g => g.Key, g => g.ToHashSet());

        if (coveredCenterToPoleCenters.Count != poweredEntities.Count)
        {
            throw new InvalidOperationException("Not all pumpjacks are covered by an electric pole.");
        }

        var removeCandidates = coveredCenterToPoleCenters
            .Where(p => p.Value.Count > 2) // Consider electric poles covering pumpjacks that are covered by at least one other electric pole.
            .SelectMany(p => p.Value)
            .Concat(poleCenterToCoveredCenters.Where(p => p.Value.Count == 0).Select(p => p.Key)) // Consider electric poles not covering any pumpjack.
            .Except(coveredCenterToPoleCenters.Where(p => p.Value.Count == 1).SelectMany(p => p.Value)) // Exclude electric poles covering pumpjacks that are only covered by one pole.
            .ToList();

        foreach (var center in removeCandidates)
        {
            var centerEntity = electricPoles[center];
            if (ArePolesConnectedWithout(electricPoles, centerEntity))
            {
                electricPoles.Remove(center);
                centerEntity.ClearNeighbors();
                RemoveProvider(context.Grid, center, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight);
            }
        }
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
        return GetElectricPoleDistance(a, b, options) <= options.ElectricPoleWireReach;
    }

    private static double GetElectricPoleDistance(Location a, Location b, Options options)
    {
        var offsetX = (options.ElectricPoleWidth - 1) / 2;
        var offsetY = (options.ElectricPoleHeight - 1) / 2;

        return b.GetEuclideanDistance(a.X + offsetX, a.Y + offsetY);
    }

    private static Dictionary<Location, ElectricPoleCenter>? AddElectricPolesAroundEntities(Context context, List<ProviderRecipient> poweredEntities)
    {
        var candidateToCovered = GetCandidateToCovered(
            context,
            poweredEntities,
            context.Options.ElectricPoleWidth,
            context.Options.ElectricPoleHeight,
            context.Options.ElectricPoleSupplyWidth,
            context.Options.ElectricPoleSupplyHeight);

        var candidateToMiddleDistance = candidateToCovered.ToDictionary(
            x => x.Key,
            x => x.Key.GetEuclideanDistance(context.Grid.Middle));

        var candidateToEntityDistance = GetCandidateToEntityDistance(poweredEntities, candidateToCovered);

        var coveredEntities = new BitArray(poweredEntities.Count);
        var electricPoles = new Dictionary<Location, ElectricPoleCenter>();
        var electricPoleList = new List<Location>();
        var toRemove = new List<Location>();

        while (coveredEntities.CountTrue() < poweredEntities.Count)
        {
            if (candidateToCovered.Count == 0)
            {
                // There are not candidates or the candidates do not fit. No solution exists given the current grid (e.g.
                // existing pipe placement eliminates all electric pole options).
                return null;
            }

            var candidate = candidateToCovered
                .Keys
                .Select(x =>
                {
                    var othersConnected = 0;
                    for (var i = 0; i < electricPoleList.Count; i++)
                    {
                        var distance = x.GetEuclideanDistance(electricPoleList[i]);
                        if (distance <= context.Options.ElectricPoleWireReach)
                        {
                            othersConnected++;
                        }
                    }

                    return (Location: x, Covered: candidateToCovered[x], OthersConnected: othersConnected);
                })
                .MinBy(x => (
                    int.MaxValue - x.Covered.CountTrue(),
                    x.OthersConnected > 0 ? x.OthersConnected : int.MaxValue,
                    x.OthersConnected > 0 ? 0 : GetDistanceToClosestCandidate(poweredEntities, coveredEntities, electricPoleList, x.Covered, x.Location),
                    candidateToEntityDistance[x.Location],
                    candidateToMiddleDistance[x.Location]
                ))!.Location;

            if (context.Options.ValidateSolution)
            {
                var covered = candidateToCovered[candidate];
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
                    throw new InvalidOperationException($"Candidate {candidate} should have been eliminated.");
                }
            }

            var centerEntity = new ElectricPoleCenter();

            AddProviderAndUpdateCandidateState(
                context.Grid,
                context.SharedInstances,
                candidate,
                centerEntity,
                c => new ElectricPoleSide(c),
                context.Options.ElectricPoleWidth,
                context.Options.ElectricPoleHeight,
                poweredEntities,
                coveredEntities,
                candidateToCovered,
                candidateToEntityDistance);

            electricPoles.Add(candidate, centerEntity);
            electricPoleList.Add(candidate);
            ConnectExistingElectricPoles(context, electricPoles, candidate, centerEntity);

            // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());
        }

        return electricPoles;
    }

    private static double GetDistanceToClosestCandidate(
        List<ProviderRecipient> recipients,
        BitArray coveredEntities,
        List<Location> providerCenters,
        BitArray covered,
        Location location)
    {
        var min = double.MaxValue;

        for (var i = 0; i < recipients.Count; i++)
        {
            if (!covered[i] && !coveredEntities[i])
            {
                var val = recipients[i].Center.GetEuclideanDistance(location);
                if (val < min)
                {
                    min = val;
                }    
            }
        }

        for (int i = 0; i < providerCenters.Count; i++)
        {
            var val = providerCenters[i].GetEuclideanDistance(location);
            if (val < min)
            {
                min = val;
            }
        }

        return min;
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
                    Distance = GetElectricPoleDistance(e.A, e.B, context.Options),
                })
                .Where(c => c.GroupA != c.GroupB)
                .Where(c => c.Distance > context.Options.ElectricPoleWireReach)
                .MinBy(c => c.Distance);

            if (closest is null)
            {
                throw new NotImplementedException();
            }

            AddSinglePoleForConnection(context, electricPoles, groups, closest.Distance, closest.Endpoints);
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

        var candidates = new Queue<Location>();
        candidates.Enqueue(idealPoint);

        Location? selectedPoint = null;

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
                if (adjacent[i].IsValid && AreElectricPolesConnected(idealLine[0], adjacent[i], context.Options))
                {
                    candidates.Enqueue(adjacent[i]);
                }
            }
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
