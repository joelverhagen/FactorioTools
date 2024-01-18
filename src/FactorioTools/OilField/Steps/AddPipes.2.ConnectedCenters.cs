using System;
using System.Collections.Generic;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

public static partial class AddPipes
{
    private static readonly IReadOnlyList<Location> Translations = new[]
    {
        new Location(1, 0),
        new Location(0, 1),
    };

    private static ILocationDictionary<ILocationSet> GetConnectedPumpjacks(Context context, PipeStrategy strategy)
    {
        var centers = context.Centers;

        if (centers.Count == 2)
        {
            var simpleConnectedCenters = context.GetLocationDictionary<ILocationSet>();
            simpleConnectedCenters.Add(centers[0], context.GetSingleLocationSet(centers[1]));
            simpleConnectedCenters.Add(centers[1], context.GetSingleLocationSet(centers[0]));
            return simpleConnectedCenters;
        }

        // Check that nodes are not collinear
        if (AreLocationsCollinear(centers))
        {
            var connected = centers.ToDictionary(context, c => c, c => context.GetLocationSet(allowEnumerate: true));
            for (var j = 1; j < centers.Count; j++)
            {
                connected[centers[j - 1]].Add(centers[j]);
                connected[centers[j]].Add(centers[j - 1]);
            }

            return connected;
        }

        var connectedCenters = strategy switch
        {
            PipeStrategy.ConnectedCentersDelaunay => GetConnectedPumpjacksWithDelaunay(context, centers),
            PipeStrategy.ConnectedCentersDelaunayMst => GetConnectedPumpjacksWithDelaunayMst(context, centers),
            PipeStrategy.ConnectedCentersFlute => GetConnectedPumpjacksWithFLUTE(context),
            _ => throw new NotImplementedException(),
        };

        // check if all connected centers have edges in both directions
        if (context.Options.ValidateSolution)
        {
            foreach (var (center, others) in connectedCenters.EnumeratePairs())
            {
                foreach (var other in others.EnumerateItems())
                {
                    if (!connectedCenters[other].Contains(center))
                    {
                        throw new FactorioToolsException("The edges in the connected centers graph are not bidirectional.");
                    }
                }
            }
        }

        // VisualizeConnectedCenters(context, connectedCenters);

        return connectedCenters;
    }

#if ENABLE_VISUALIZER
    private static void VisualizeConnectedCenters(Context context, ILocationDictionary<ILocationSet> connectedCenters)
    {
        var edges = new HashSet<DelaunatorSharp.IEdge>();
        foreach (var (center, centers) in connectedCenters.EnumeratePairs())
        {
            foreach (var other in centers.EnumerateItems())
            {
                var edge = new DelaunatorSharp.Edge(e: 0, new DelaunatorSharp.Point(center.X, center.Y), new DelaunatorSharp.Point(other.X, other.Y));
                edges.Add(edge);
            }
        }

        Visualizer.Show(context.Grid, connectedCenters.ToDelaunatorPoints(), edges);
    }
#endif

    private record GroupCandidate(
        PumpjackGroup Group,
        Location Center,
        Location IncludedCenter,
        TerminalLocation Terminal,
        List<Location> Path);

    private static ILocationSet FindTrunksAndConnect(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters)
    {
        var selectedTrunks = FindTrunks(context, centerToConnectedCenters);

        var allIncludedCenters = context.GetLocationSet(selectedTrunks.Count * 2);
        for (var i = 0; i < selectedTrunks.Count; i++)
        {
            foreach (var center in selectedTrunks[i].Centers.EnumerateItems())
            {
                allIncludedCenters.Add(center);
            }
        }

        var groups = new List<PumpjackGroup>(selectedTrunks.Count);
        for (var i = 0; i < selectedTrunks.Count; i++)
        {
            groups.Add(new PumpjackGroup(context, centerToConnectedCenters, allIncludedCenters, selectedTrunks[i]));
        }

        if (groups.Count == 0)
        {
            var group = ConnectTwoClosestPumpjacks(context, centerToConnectedCenters, allIncludedCenters);

            groups.Add(group);
        }

        /*
        var clone = new PipeGrid(context.Grid);
        Visualizer.Show(clone, groups.SelectMany(g => g.Pipes).Distinct(context).Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
        */

        while (groups.Count > 1 || groups[0].IncludedCenters.Count < context.CenterToTerminals.Count)
        {
            double? shortestDistance = null;
            GroupCandidate? candidate = null;

            foreach (var group in groups)
            {
                var centroidX = group.Pipes.EnumerateItems().Average(l => l.X);
                var centroidY = group.Pipes.EnumerateItems().Average(l => l.Y);

                foreach (var center in group.FrontierCenters.EnumerateItems())
                {
                    var includedCenter = group
                        .IncludedCenterToChildCenters
                        .EnumeratePairs()
                        .First(p => p.Value.Contains(center))
                        .Key;

                    // Prefer the terminal that has the shortest path, then prefer the terminal closer to the centroid
                    // of the child (unconnected) pumpjacks.
                    foreach (var terminal in context.CenterToTerminals[center])
                    {
                        var path = GetShortestPathToGroup(context, terminal, group, centroidX, centroidY);

                        if (candidate is null)
                        {
                            candidate = new GroupCandidate(group, center, includedCenter, terminal, path);
                            continue;
                        }

                        var comparison = path.Count.CompareTo(candidate.Path.Count);
                        if (comparison < 0)
                        {
                            candidate = new GroupCandidate(group, center, includedCenter, terminal, path);
                            shortestDistance = null;
                            continue;
                        }
                        
                        if (comparison > 0)
                        {
                            continue;
                        }

                        if (!shortestDistance.HasValue)
                        {
                            shortestDistance = candidate.Group.GetChildCentroidDistanceSquared(candidate.IncludedCenter, candidate.Terminal.Terminal);
                        }

                        var distance = group.GetChildCentroidDistanceSquared(includedCenter, terminal.Terminal);
                        comparison = distance.CompareTo(shortestDistance.Value);
                        if (comparison < 0)
                        {
                            candidate = new GroupCandidate(group, center, includedCenter, terminal, path);
                            shortestDistance = distance;
                        }
                    }
                }
            }

            if (candidate is null)
            {
                throw new FactorioToolsException("No group candidate was found.");
            }

            if (allIncludedCenters.Contains(candidate.Terminal.Center))
            {
                var otherGroup = groups.Single(g => g.IncludedCenters.Contains(candidate.Terminal.Center));
                candidate.Group.MergeGroup(otherGroup, candidate.Path);
                groups.Remove(otherGroup);

                /*
                var clone2 = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone2, new(), context.CenterToTerminals, groups.SelectMany(g => g.Pipes).ToSet(), allowMultipleTerminals: true);
                Visualizer.Show(clone2, path.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                */
            }
            else
            {
                // Add the newly connected pumpjack to the current group.
                candidate.Group.ConnectPumpjack(candidate.Center, candidate.Path);
                EliminateOtherTerminals(context, candidate.Terminal);

                /*
                var clone2 = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone2, new(), context.CenterToTerminals, groups.SelectMany(g => g.Pipes).ToSet(), allowMultipleTerminals: true);
                Visualizer.Show(clone2, path.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                */
            }
        }

        return groups.Single().Pipes;
    }

    private static List<Location> GetShortestPathToGroup(Context context, TerminalLocation terminal, PumpjackGroup group, double groupCentroidX, double groupCentroidY)
    {
#if !USE_SHARED_INSTANCES
        var aStarResultV = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, xWeight: 2);
        var aStarResultH = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, yWeight: 2);
#else
        try
        {
            var aStarResultV = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, xWeight: 2, outputList: context.SharedInstances.LocationListA);
            var aStarResultH = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, yWeight: 2, outputList: context.SharedInstances.LocationListB);
#endif

            if (!aStarResultV.Success)
            {
                throw new NoPathBetweenTerminalsException(terminal.Terminal, group.Pipes.EnumerateItems().First());
            }

            if (aStarResultV.Path.SequenceEqual(aStarResultH.Path))
            {
                return aStarResultV.Path.ToList();
            }

            var adjacentPipesV = 0;
            double centroidDistanceSquaredV = 0;

            var adjacentPipesH = 0;
            double centroidDistanceSquaredH = 0;

            var sizeEstimate = aStarResultV.Path.Count + aStarResultH.Path.Count;

#if !USE_SHARED_INSTANCES
            var locationToCentroidDistanceSquared = context.GetLocationDictionary<double>(sizeEstimate);
#else
            var locationToCentroidDistanceSquared = context.SharedInstances.LocationToDouble;
            try
            {
#endif
                var width = context.Grid.Width;
                for (var i = 0; i < Math.Max(aStarResultV.Path.Count, aStarResultH.Path.Count); i++)
                {
                    if (i < aStarResultV.Path.Count)
                    {
                        var location = aStarResultV.Path[i];
                        if (context.LocationToAdjacentCount[location.Y * width + location.X] > 0)
                        {
                            adjacentPipesV++;
                        }

                        centroidDistanceSquaredV += GetCentroidDistanceSquared(groupCentroidX, groupCentroidY, locationToCentroidDistanceSquared, location);
                    }

                    if (i < aStarResultH.Path.Count)
                    {
                        var location = aStarResultH.Path[i];
                        if (context.LocationToAdjacentCount[location.Y * width + location.X] > 0)
                        {
                            adjacentPipesH++;
                        }

                        centroidDistanceSquaredH += GetCentroidDistanceSquared(groupCentroidX, groupCentroidY, locationToCentroidDistanceSquared, location);
                    }
                }
#if USE_SHARED_INSTANCES
            }
            finally
            {
                locationToCentroidDistanceSquared.Clear();
            }
#endif

            if (adjacentPipesV > adjacentPipesH)
            {
                return aStarResultV.Path.ToList();
            }
            else if (adjacentPipesV < adjacentPipesH)
            {
                return aStarResultH.Path.ToList();
            }
            else if (centroidDistanceSquaredV < centroidDistanceSquaredH)
            {
                return aStarResultV.Path.ToList();
            }
            else
            {
                return aStarResultH.Path.ToList();
            }
#if USE_SHARED_INSTANCES
        }
        finally
        {
            context.SharedInstances.LocationListA.Clear();
            context.SharedInstances.LocationListB.Clear();
        }
#endif
    }

    private static double GetCentroidDistanceSquared(
        double groupCentroidX,
        double groupCentroidY,
        ILocationDictionary<double> locationToCentroidDistanceSquared,
        Location location)
    {
        if (!locationToCentroidDistanceSquared.TryGetValue(location, out var centroidDistanceSquared))
        {
            centroidDistanceSquared = location.GetEuclideanDistanceSquared(groupCentroidX, groupCentroidY);
            locationToCentroidDistanceSquared.Add(location, centroidDistanceSquared);
        }

        return centroidDistanceSquared;
    }

    private static List<Trunk> FindTrunks(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters)
    {
        /*
        Visualizer.Show(context.Grid, Array.Empty<IPoint>(), centerToConnectedCenters
            .SelectMany(p => p.Value.Select(o => (p.Key, o))
            .Select(p => (IEdge)new Edge(0, new Point(p.Key.X, p.Key.Y), new Point(p.o.X, p.o.Y)))
            .Distinct()));
        */

        var trunkCandidates = GetTrunkCandidates(context, centerToConnectedCenters);

        trunkCandidates.Sort((a, b) =>
        {
            var c = b.TerminalLocations.Count.CompareTo(a.TerminalLocations.Count);
            if (c != 0)
            {
                return c;
            }

            c = a.Length.CompareTo(b.Length);
            if (c != 0)
            {
                return c;
            }

            var aC = a.GetTrunkEndDistance(centerToConnectedCenters);
            var bC = b.GetTrunkEndDistance(centerToConnectedCenters);
            c = aC.CompareTo(bC);
            if (c != 0)
            {
                return c;
            }

            return a.OriginalIndex.CompareTo(b.OriginalIndex);
        });

        // Eliminate lower priority trunks that have any pipes shared with higher priority trunks.
        var includedPipes = context.GetLocationSet();
        var includedCenters = context.GetLocationSet(allowEnumerate: true);
        var selectedTrunks = new List<Trunk>();
        foreach (var trunk in trunkCandidates)
        {
            var path = MakeStraightLine(trunk.Start, trunk.End);
            if (!includedPipes.Overlaps(path) && !includedCenters.Overlaps(trunk.Centers.EnumerateItems()))
            {
                selectedTrunks.Add(trunk);
                includedPipes.UnionWith(path);
                includedCenters.UnionWith(trunk.Centers);
            }
        }

        /*
        for (var i = 1; i <= selectedTrunks.Count; i++)
        {
            Visualizer.Show(context.Grid, selectedTrunks.Take(i).SelectMany(t => t.Centers).Distinct(context).Select(l => (IPoint)new Point(l.X, l.Y)), selectedTrunks
                .Take(i)
                .Select(t => (IEdge)new Edge(0, new Point(t.Start.X, t.Start.Y), new Point(t.End.X, t.End.Y)))
                .ToList());
        }
        */

        // Eliminate unused terminals for pumpjacks included in all of the trunks. A pumpjack connected to a trunk has
        // its terminal selected.
        foreach (var trunk in selectedTrunks)
        {
            foreach (var terminal in trunk.Terminals)
            {
                EliminateOtherTerminals(context, terminal);
            }
        }

        // Visualize(context, locationToPoint, selectedTrunks.SelectMany(t => MakeStraightLine(t.Start, t.End)).ToSet());

        // Find the "child" unconnected pumpjacks of each connected pumpjack. These are pumpjacks are connected via the
        // given connected pumpjack.
        return selectedTrunks;
    }

    private record BestConnection(List<Location> Path, TerminalLocation Terminal, TerminalLocation BestTerminal);

    private static PumpjackGroup ConnectTwoClosestPumpjacks(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters, ILocationSet allIncludedCenters)
    {
        var centerToGoals = context.GetLocationDictionary<ILocationSet>();
        BestConnection? bestConnection = null;
        int bestConnectedCentersCount = default;
        int? bestOtherConnectedCentersCount = null;
        double? bestTerminalDistance = null;
        double? bestOtherTerminalDistance = null;

        for (var i = 0; i < context.Centers.Count; i++)
        {
            var center = context.Centers[i];
            var terminals = context.CenterToTerminals[center];

            for (var j = 0; j < terminals.Count; j++)
            {
                var terminal = terminals[j];
                var connectedCenters = centerToConnectedCenters[center];

                foreach (var otherCenter in connectedCenters.EnumerateItems())
                {
                    var otherTerminals = context.CenterToTerminals[otherCenter];

                    if (!centerToGoals.TryGetValue(otherCenter, out var goals))
                    {
                        goals = context.GetLocationSet(allowEnumerate: true);
                        for (var k = 0; k < otherTerminals.Count; k++)
                        {
                            goals.Add(otherTerminals[k].Terminal);
                        }

                        centerToGoals.Add(otherCenter, goals);
                    }

                    var result = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, goals);
                    if (!result.Success)
                    {
                        throw new NoPathBetweenTerminalsException(terminal.Terminal, goals.EnumerateItems().First());
                    }

                    var reachedGoal = result.ReachedGoal;
                    var closestTerminal = otherTerminals.Single(t => t.Terminal == reachedGoal);
                    var path = result.Path;

                    if (bestConnection is null)
                    {
                        bestConnection = new BestConnection(path, terminal, closestTerminal);
                        bestConnectedCentersCount = connectedCenters.Count;
                        bestOtherConnectedCentersCount = null;
                        bestTerminalDistance = null;
                        bestOtherTerminalDistance = null;
                        continue;
                    }

                    var c = path.Count.CompareTo(bestConnection.Path.Count);
                    if (c < 0)
                    {
                        bestConnection = new BestConnection(path, terminal, closestTerminal);
                        bestConnectedCentersCount = connectedCenters.Count;
                        bestOtherConnectedCentersCount = null;
                        bestTerminalDistance = null;
                        bestOtherTerminalDistance = null;
                        continue;
                    }
                    else if (c > 0)
                    {
                        continue;
                    }

                    c = connectedCenters.Count.CompareTo(bestConnectedCentersCount);
                    if (c > 0)
                    {
                        bestConnection = new BestConnection(path, terminal, closestTerminal);
                        bestConnectedCentersCount = connectedCenters.Count;
                        bestOtherConnectedCentersCount = null;
                        bestTerminalDistance = null;
                        bestOtherTerminalDistance = null;
                        continue;
                    }
                    else if (c < 0)
                    {
                        continue;
                    }

                    if (!bestOtherConnectedCentersCount.HasValue)
                    {
                        bestOtherConnectedCentersCount = centerToConnectedCenters[bestConnection.BestTerminal.Center].Count;
                    }

                    var otherConnectedCentersCount = centerToConnectedCenters[otherCenter].Count;
                    c = otherConnectedCentersCount.CompareTo(bestOtherConnectedCentersCount.Value);
                    if (c > 0)
                    {
                        bestConnection = new BestConnection(path, terminal, closestTerminal);
                        bestConnectedCentersCount = connectedCenters.Count;
                        bestOtherConnectedCentersCount = otherConnectedCentersCount;
                        bestTerminalDistance = null;
                        bestOtherTerminalDistance = null;
                        continue;
                    }
                    else if (c < 0)
                    {
                        continue;
                    }

                    if (!bestTerminalDistance.HasValue)
                    {
                        bestTerminalDistance = bestConnection.Terminal.Terminal.GetEuclideanDistanceSquared(context.Grid.Middle);
                    }

                    var terminalDistance = terminal.Terminal.GetEuclideanDistance(context.Grid.Middle);
                    c = terminalDistance.CompareTo(bestTerminalDistance.Value);
                    if (c < 0)
                    {
                        bestConnection = new BestConnection(path, terminal, closestTerminal);
                        bestConnectedCentersCount = connectedCenters.Count;
                        bestOtherConnectedCentersCount = otherConnectedCentersCount;
                        bestTerminalDistance = terminalDistance;
                        bestOtherTerminalDistance = null;
                        continue;
                    }
                    else if (c > 0)
                    {
                        continue;
                    }
                    
                    if (!bestOtherTerminalDistance.HasValue)
                    {
                        bestOtherTerminalDistance = bestConnection.BestTerminal.Terminal.GetEuclideanDistanceSquared(context.Grid.Middle);
                    }

                    var otherTerminalDistance = closestTerminal.Terminal.GetEuclideanDistance(context.Grid.Middle);
                    c = otherTerminalDistance.CompareTo(bestOtherTerminalDistance.Value);
                    if (c < 0)
                    {
                        bestConnection = new BestConnection(path, terminal, closestTerminal);
                        bestConnectedCentersCount = connectedCenters.Count;
                        bestOtherConnectedCentersCount = otherConnectedCentersCount;
                        bestTerminalDistance = terminalDistance;
                        bestOtherTerminalDistance = otherTerminalDistance;
                    }
                }
            }
        }

        if (bestConnection is null)
        {
            throw new FactorioToolsException("A new connection should have been found.");
        }

        EliminateOtherTerminals(context, bestConnection.Terminal);
        EliminateOtherTerminals(context, bestConnection.BestTerminal);

        var group = new PumpjackGroup(
            context,
            centerToConnectedCenters,
            allIncludedCenters,
            new[] { bestConnection.Terminal.Center, bestConnection.BestTerminal.Center },
            bestConnection.Path);

        return group;
    }

    private static ILocationSet GetChildCenters(
        Context context,
        ILocationDictionary<ILocationSet> centerToConnectedCenters,
        ILocationSet ignoreCenters,
        ILocationSet shallowExploreCenters,
        Location startingCenter)
    {
        var queue = new Queue<(Location Location, bool ShouldRecurse)>();
        var visited = context.GetLocationSet(allowEnumerate: true);
        queue.Enqueue((startingCenter, ShouldRecurse: true));

        while (queue.Count > 0)
        {
            (var current, var shouldRecurse) = queue.Dequeue();
            if (!visited.Add(current) || !shouldRecurse)
            {
                continue;
            }

            foreach (var other in centerToConnectedCenters[current].EnumerateItems())
            {
                if (ignoreCenters.Contains(other))
                {
                    continue;
                }

                // If the other center is in another group, don't recursively explore.
                queue.Enqueue((other, ShouldRecurse: !shallowExploreCenters.Contains(other)));
            }
        }

        visited.Remove(startingCenter);
        return visited;
    }

    private static List<Trunk> GetTrunkCandidates(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters)
    {
        var centerToMaxX = context
            .Centers
            .ToDictionary(context, c => c, c => centerToConnectedCenters[c].EnumerateItems().Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.X)));
        var centerToMaxY = context
            .Centers
            .ToDictionary(context, c => c, c => centerToConnectedCenters[c].EnumerateItems().Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.Y)));

        // Find paths that connect the most terminals of neighboring pumpjacks.
        var trunkCandidates = new List<Trunk>();
        foreach (var translation in Translations)
        {
            foreach (var startingCenter in context.Centers)
            {
                foreach (var terminal in context.CenterToTerminals[startingCenter])
                {
                    var currentCenter = startingCenter;
                    var expandedChildCenters = false;
                    var nextCenters = centerToConnectedCenters[currentCenter];
                    var maxX = centerToMaxX[currentCenter];
                    var maxY = centerToMaxY[currentCenter];

                    var location = terminal.Terminal.Translate(translation);

                    Trunk? trunk = null;

                    while (location.X <= maxX && location.Y <= maxY && context.Grid.IsEmpty(location))
                    {
                        if (context.LocationToTerminals.TryGetValue(location, out var terminals))
                        {
                            Location nextCenter = Location.Invalid;
                            bool hasMatch = false;
                            foreach (var nextTerminal in terminals)
                            {
                                if (nextCenters.Contains(nextTerminal.Center))
                                {
                                    nextCenter = nextTerminal.Center;
                                    hasMatch = true;
                                    break;
                                }
                            }

                            if (!hasMatch)
                            {
                                // The pumpjack terminal we ran into does not belong to the a pumpjack that the current
                                // pumpjack should be connected to.
                                break;
                            }

                            if (!expandedChildCenters)
                            {
                                nextCenters = GetChildCenters(
                                    context,
                                    centerToConnectedCenters,
                                    ignoreCenters: context.GetSingleLocationSet(currentCenter),
                                    shallowExploreCenters: context.GetSingleLocationSet(nextCenter),
                                    nextCenter);

                                if (nextCenters.Count == 0)
                                {
                                    break;
                                }

                                maxX = nextCenters.EnumerateItems().Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.X));
                                maxY = nextCenters.EnumerateItems().Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.Y));
                                expandedChildCenters = true;
                            }

                            if (trunk is null)
                            {
                                trunk = new Trunk(context, terminal, currentCenter);
                            }

                            trunk.Terminals.AddRange(terminals);
                            foreach (var other in terminals)
                            {
                                trunk.TerminalLocations.Add(other.Terminal);
                            }
                            foreach (var nextTerminal in terminals)
                            {
                                trunk.Centers.Add(nextTerminal.Center);
                            }

                            currentCenter = nextCenter;
                        }

                        location = location.Translate(translation);
                    }

                    if (trunk is not null && trunk.Centers.Count > 1)
                    {
                        trunk.OriginalIndex = trunkCandidates.Count;
                        trunkCandidates.Add(trunk);
                    }
                }
            }
        }

        return trunkCandidates;
    }

    private class Trunk
    {
        private readonly Context _context;
        private double? _trunkEndDistance;

        public Trunk(Context context, TerminalLocation startingTerminal, Location center)
        {
            _context = context;
            TerminalLocations = context.GetLocationSet(startingTerminal.Terminal, capacity: 2);
            Terminals.Add(startingTerminal);
            Centers = context.GetLocationSet(center, capacity: 2, allowEnumerate: true);
        }

        public int OriginalIndex { get; set; }
        public List<TerminalLocation> Terminals { get; } = new List<TerminalLocation>(2);
        public ILocationSet TerminalLocations { get; }
        public ILocationSet Centers { get; }
        public int Length => Start.GetManhattanDistance(End) + 1;
        public Location Start => Terminals[0].Terminal;
        public Location End => Terminals[Terminals.Count - 1].Terminal;

        public double GetTrunkEndDistance(ILocationDictionary<ILocationSet> centerToConnectedCenters)
        {
            if (_trunkEndDistance.HasValue)
            {
                return _trunkEndDistance.Value;
            }

            var neighbors = _context.GetLocationSet(allowEnumerate: true);
            foreach (var center in Centers.EnumerateItems())
            {
                foreach (var otherCenter in centerToConnectedCenters[center].EnumerateItems())
                {
                    neighbors.Add(otherCenter);
                }
            }

            neighbors.ExceptWith(Centers);

            if (neighbors.Count == 0)
            {
                _trunkEndDistance = 0;
                return _trunkEndDistance.Value;
            }

            var centroidX = neighbors.EnumerateItems().Average(l => l.X);
            var centroidY = neighbors.EnumerateItems().Average(l => l.Y);
            _trunkEndDistance = Start.GetEuclideanDistance(centroidX, centroidY) + End.GetEuclideanDistance(centroidX, centroidY);
            return _trunkEndDistance.Value;
        }

#if ENABLE_GRID_TOSTRING
        public override string ToString()
        {
            return $"{Start} -> {End}";
        }
#endif
    }

    private class PumpjackGroup
    {
        private readonly Context _context;
        private readonly ILocationDictionary<ILocationSet> _centerToConnectedCenters;
        private readonly ILocationSet _allIncludedCenters;

        public PumpjackGroup(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters, ILocationSet allIncludedCenters, Trunk trunk)
            : this(context, centerToConnectedCenters, allIncludedCenters, trunk.Centers.EnumerateItems(), MakeStraightLine(trunk.Start, trunk.End))
        {
        }

        public PumpjackGroup(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters, ILocationSet allIncludedCenters, IReadOnlyCollection<Location> includedCenters, IReadOnlyCollection<Location> pipes)
        {
            _context = context;
            _centerToConnectedCenters = centerToConnectedCenters;
            _allIncludedCenters = allIncludedCenters;

            IncludedCenters = includedCenters.ToReadOnlySet(context, allowEnumerate: true);

            FrontierCenters = context.GetLocationSet(allowEnumerate: true);
            IncludedCenterToChildCenters = context.GetLocationDictionary<ILocationSet>();

            Pipes = pipes.ToSet(context, allowEnumerate: true);

            UpdateFrontierCenters();
            UpdateIncludedCenterToChildCenters();
        }

        public ILocationSet IncludedCenters { get; }
        public ILocationSet FrontierCenters { get; }
        public ILocationDictionary<ILocationSet> IncludedCenterToChildCenters { get; }
        public ILocationSet Pipes { get; }

        public double GetChildCentroidDistanceSquared(Location includedCenter, Location terminalCandidate)
        {
            var sumX = 0;
            var sumY = 0;
            var count = 0;
            foreach (var center in IncludedCenterToChildCenters[includedCenter].EnumerateItems())
            {
                sumX += center.X;
                sumY += center.Y;
                count++;
            }

            var centroidX = (double)sumX / count;
            var centroidY = (double)sumY / count;

            return terminalCandidate.GetEuclideanDistanceSquared(centroidX, centroidY);
        }

        public void ConnectPumpjack(Location center, List<Location> path)
        {
            _allIncludedCenters.Add(center);
            IncludedCenters.Add(center);
            Pipes.UnionWith(path);
            UpdateFrontierCenters();
            UpdateIncludedCenterToChildCenters();
        }

        public void MergeGroup(PumpjackGroup other, List<Location> path)
        {
            IncludedCenters.UnionWith(other.IncludedCenters);
            Pipes.UnionWith(path);
            Pipes.UnionWith(other.Pipes);
            UpdateFrontierCenters();
            UpdateIncludedCenterToChildCenters();
        }

        private void UpdateFrontierCenters()
        {
            FrontierCenters.Clear();

            foreach (var center in IncludedCenters.EnumerateItems())
            {
                FrontierCenters.UnionWith(_centerToConnectedCenters[center]);
            }

            FrontierCenters.ExceptWith(IncludedCenters);
        }

        private void UpdateIncludedCenterToChildCenters()
        {
            IncludedCenterToChildCenters.Clear();

            foreach (var center in IncludedCenters.EnumerateItems())
            {
                ILocationSet visited = GetChildCenters(
                    _context,
                    _centerToConnectedCenters,
                    IncludedCenters,
                    _allIncludedCenters,
                    center);

                IncludedCenterToChildCenters.Add(center, visited);
            }
        }
    }
}
