using DelaunatorSharp;
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

        var electricPoles = AddElectricPolesAroundPumpjacks(context);
        if (electricPoles is null)
        {
            return null;
        }

        // ConnectExistingElectricPoles(context, electricPoles);

        // Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());

        ConnectElectricPoles(context, electricPoles);

        // Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());

        RemoveExtraElectricPoles(context, electricPoles);

        // Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());

        PruneNeighbors(context, electricPoles);

        // Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());

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

        var firstNode = graph.Keys.OrderBy(context.Grid.Middle.GetEuclideanDistance).First();
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

    private static void RemoveExtraElectricPoles(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles)
    {
        var poleCenterToCoveredCenters = new Dictionary<Location, HashSet<Location>>();
        foreach (var center in electricPoles.Keys)
        {
            var coveredCenters = new HashSet<Location>();
            var offsetX = context.Options.ElectricPoleSupplyWidth / 2 - context.Options.ElectricPoleWidth / 2;
            var offsetY = context.Options.ElectricPoleSupplyHeight / 2 - context.Options.ElectricPoleHeight / 2;

            for (var x = Math.Max(center.X - offsetX, 0); x <= Math.Min(center.X + offsetX + context.Options.ElectricPoleWidth / 2, context.Grid.Width - 1); x++)
            {
                for (var y = Math.Max(center.Y - offsetY, 0); y <= Math.Min(center.Y + offsetY + context.Options.ElectricPoleWidth / 2, context.Grid.Height - 1); y++)
                {
                    var location = new Location(x, y);

                    var entity = context.Grid[location];
                    if (entity is PumpjackCenter)
                    {
                        coveredCenters.Add(location);
                    }
                    else if (entity is PumpjackSide pumpjackSide)
                    {
                        coveredCenters.Add(context.Grid.EntityToLocation[pumpjackSide.Center]);
                    }
                }
            }

            poleCenterToCoveredCenters.Add(center, coveredCenters);
        }

        var coveredCenterToPoleCenters = poleCenterToCoveredCenters
            .SelectMany(p => p.Value.Select(c => new { PoleCenter = p.Key, PumpjackCenter = c }))
            .GroupBy(p => p.PumpjackCenter)
            .ToDictionary(g => g.Key, g => g.Select(p => p.PoleCenter).ToHashSet());

        if (coveredCenterToPoleCenters.Count != context.CenterToTerminals.Count)
        {
            throw new InvalidOperationException("Not all pumpjacks are covered by an electric pole.");
        }

        var removeCandidates = coveredCenterToPoleCenters
            .Where(p => p.Value.Count > 2) // Consider electric poles covering pumpjacks that are covered by at least one other electric pole.
            .SelectMany(p => p.Value)
            .Concat(poleCenterToCoveredCenters.Where(p => p.Value.Count == 0).Select(p => p.Key)) // Consider electric poles not covering any pumpjack.
            .Except(coveredCenterToPoleCenters.Where(p => p.Value.Count == 1).SelectMany(p => p.Value)) // Exclude electric poles covering pumpjacks that are only covered by one pole.
            .ToList();

        foreach (var candidate in removeCandidates)
        {
            if (ArePolesConnectedWithout(electricPoles, electricPoles[candidate]))
            {

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

        return discovered.Count == electricPoles.Count;
    }

    private static void ConnectExistingElectricPoles(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles, Location location, ElectricPoleCenter center)
    {
        foreach ((var otherLocation, var otherCenter) in electricPoles)
        {
            if (location == otherLocation)
            {
                continue;
            }

            if (AreElectricPolesConnected(location, otherLocation, context.Options))
            {
                center.AddNeighbor(otherCenter);
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

    private static Dictionary<Location, ElectricPoleCenter>? AddElectricPolesAroundPumpjacks(Context context)
    {
        var pumpjackArea = (X: 3, Y: 3);
        var offsetX = pumpjackArea.X / 2 + context.Options.ElectricPoleSupplyWidth / 2 - context.Options.ElectricPoleWidth / 2;
        var offsetY = pumpjackArea.Y / 2 + context.Options.ElectricPoleSupplyHeight / 2 - context.Options.ElectricPoleHeight / 2;

        // First, find the spots for electric poles that cover the most pumpjacks.
        var candidateToCovered = new Dictionary<Location, HashSet<Location>>();

        var allTerminals = context.CenterToTerminals.SelectMany(t => t.Value).Select(t => t.Terminal).ToHashSet();

        // Generate electric pole locations
        foreach (var center in context.CenterToTerminals.Keys)
        {
            for (var x = center.X - offsetX - context.Options.ElectricPoleWidth / 2; x <= center.X + offsetX; x++)
            {
                for (var y = center.Y - offsetY - context.Options.ElectricPoleWidth / 2; y <= center.Y + offsetY; y++)
                {
                    var candidate = new Location(x, y);
                    (var fits, _) = GetElectricPoleLocations(context.Grid, context.Options, candidate, populateSides: false);

                    if (!fits)
                    {
                        continue;
                    }

                    if (!candidateToCovered.TryGetValue(candidate, out var covered))
                    {
                        candidateToCovered.Add(candidate, new HashSet<Location> { center });
                    }
                    else
                    {
                        covered.Add(center);
                    }
                }
            }
        }

        var candidateToMiddleDistance = candidateToCovered.ToDictionary(
            x => x.Key,
            x => x.Key.GetEuclideanDistance(context.Grid.Middle));

        var coveredPumpjacks = new HashSet<Location>();
        var electricPoles = new Dictionary<Location, ElectricPoleCenter>();

        while (coveredPumpjacks.Count < context.CenterToTerminals.Count)
        {
            var candidateToPumpjackDistance = candidateToCovered.ToDictionary(
                x => x.Key,
                x => x.Value.Sum(y => y.GetEuclideanDistance(x.Key)));

            // Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());

            // Prefer spots that cover the most pumpjacks.
            // Break ties with spots in range of existing power poles.
            // Break further ties with spots closer to their pumpjack centers.
            // Break further ties with spots closer to the center of the grid.
            var sortedCandidates = candidateToCovered
                .Keys
                .Select(x =>
                {
                    var others = electricPoles
                        .Keys
                        .Select(y =>
                        {
                            var distance = x.GetEuclideanDistance(y);
                            return new { Location = y, Distance = distance, Connected = distance <= context.Options.ElectricPoleWireReach };
                        })
                        .ToList();

                    return new
                    {
                        Location = x,
                        Covered = candidateToCovered[x],
                        Others = others,
                        Connected = others.Any(c => c.Connected),
                    };
                })
                .OrderByDescending(x => x.Covered.Count)
                .ThenBy(x => x.Connected ? x.Others.Count(o => o.Connected) : int.MaxValue)
                .ThenBy(x => !x.Connected ? context
                    .CenterToTerminals
                    .Keys
                    .Except(x.Covered)
                    .Except(coveredPumpjacks)
                    .Concat(electricPoles.Keys)
                    .Select(l => l.GetEuclideanDistance(x.Location))
                    .DefaultIfEmpty(double.MaxValue)
                    .Min() : 0)
                .ThenBy(x => candidateToPumpjackDistance[x.Location])
                .ThenBy(x => candidateToMiddleDistance[x.Location])
                .ToList();

            var poleAdded = false;
            foreach (var candidateInfo in sortedCandidates)
            {
                var candidate = candidateInfo.Location;
                var covered = candidateToCovered[candidate];
                if (covered.IsSubsetOf(coveredPumpjacks))
                {
                    throw new InvalidOperationException($"Candidate {candidate} should have been eliminated.");
                }

                (var fits, var sides) = GetElectricPoleLocations(context.Grid, context.Options, candidate, populateSides: true);

                if (fits)
                {
                    AddElectricPole(context, electricPoles, candidate, sides!);
                    coveredPumpjacks.UnionWith(candidateToCovered[candidate]);

                    // Remove the covered pumpjacks from the candidate data, so that the next candidates are discounted
                    // by the pumpjacks that no longer need power.
                    foreach ((var otherCandidate, var otherCovered) in candidateToCovered.ToList())
                    {
                        otherCovered.ExceptWith(coveredPumpjacks);
                        if (otherCovered.Count == 0)
                        {
                            candidateToCovered.Remove(otherCandidate);
                        }
                    }

                    poleAdded = true;
                    break;
                }
            }

            if (!poleAdded)
            {
                // There are not candidates or the candidates do not fit. No solution exists given the current grid (e.g.
                // existing pipe placement eliminates all electric pole options).
                return null;
            }
        }

        return electricPoles;
    }

    private static ElectricPoleCenter AddElectricPole(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles, Location candidate, List<Location> sides)
    {
        var center = new ElectricPoleCenter();
        context.Grid.AddEntity(candidate, center);
        electricPoles.Add(candidate, center);

        foreach (var location in sides)
        {
            context.Grid.AddEntity(location, new ElectricPoleSide(center));
        }

        ConnectExistingElectricPoles(context, electricPoles, candidate, center);

        return center;
    }

    public static (bool Fits, List<Location>? Sides) GetElectricPoleLocations(SquareGrid grid, Options options, Location center, bool populateSides)
    {
        var fits = true;
        var sides = populateSides ? new List<Location>() : null;

        var offsetX = (options.ElectricPoleWidth - 1) / 2 * -1;
        var offsetY = (options.ElectricPoleHeight - 1) / 2 * -1;

        for (var w = 0; w < options.ElectricPoleWidth && fits; w++)
        {
            for (var h = 0; h < options.ElectricPoleHeight && fits; h++)
            {
                var location = center.Translate((offsetX + w, offsetY + h));
                fits = grid.IsInBounds(location) && grid.IsEmpty(location);

                if (fits && location != center && populateSides)
                {
                    sides!.Add(location);
                }
            }
        }

        return (fits, sides);
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
                .OrderBy(c => c.Distance)
                .FirstOrDefault();

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
        List<Location>? sides = null;
        while (candidates.Count > 0)
        {
            var candidate = candidates.Dequeue();
            (var fits, sides) = GetElectricPoleLocations(context.Grid, context.Options, candidate, populateSides: true);
            if (fits)
            {
                selectedPoint = candidate;
                break;
            }

            foreach (var adjacent in context.Grid.GetAdjacent(candidate))
            {
                if (AreElectricPolesConnected(idealLine[0], adjacent, context.Options))
                {
                    candidates.Enqueue(adjacent);
                }
            }
        }

        if (!selectedPoint.HasValue || sides is null)
        {
            throw new InvalidOperationException("Could not find a pole that can be connected");
        }

        var center = AddElectricPole(context, electricPoles, selectedPoint.Value, sides);
        var connectedGroups = groups.Where(g => g.Intersect(center.Neighbors.Select(n => context.Grid.EntityToLocation[n])).Any()).ToList();

        if (connectedGroups.Count == 0)
        {
            Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());
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
