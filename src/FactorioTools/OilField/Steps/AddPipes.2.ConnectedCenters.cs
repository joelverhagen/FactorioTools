using System.Data;
using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

public static partial class AddPipes
{
    private static readonly IReadOnlyList<(int DeltaX, int DeltaY)> Translations = new[] { (1, 0), (0, 1) };

    private static Dictionary<Location, HashSet<Location>> GetConnectedPumpjacks(Context context, PipeStrategy strategy)
    {
        var centers = context
            .CenterToTerminals
            .Keys
            .OrderBy(c => c.X)
            .ThenBy(c => c.Y)
            .ToList();

        if (centers.Count == 2)
        {
            return new Dictionary<Location, HashSet<Location>>
            {
                { centers[0], new HashSet<Location> { centers[1] } },
                { centers[1], new HashSet<Location> { centers[0] } },
            };
        }

        // Check that nodes are not collinear
        if (AreLocationsCollinear(centers))
        {
            var connected = centers.ToDictionary(c => c, c => new HashSet<Location>());
            for (var j = 1; j < centers.Count; j++)
            {
                connected[centers[j - 1]].Add(centers[j]);
                connected[centers[j]].Add(centers[j - 1]);
            }

            return connected;
        }

        var connectedCenters = strategy switch
        {
            PipeStrategy.ConnectedCentersDelaunay => GetConnectedPumpjacksWithDelaunay(centers),
            PipeStrategy.ConnectedCentersDelaunayMst => GetConnectedPumpjacksWithDelaunayMst(context, centers),
            PipeStrategy.ConnectedCentersFlute => GetConnectedPumpjacksWithFLUTE(context),
            _ => throw new NotImplementedException(),
        };

        // check if all connected centers have edges in both directions
        if (context.Options.ValidateSolution)
        {
            foreach (var (center, others) in connectedCenters)
            {
                foreach (var other in others)
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

#if DEBUG
    private static void VisualizeConnectedCenters(Context context, Dictionary<Location, HashSet<Location>> connectedCenters)
    {
        var edges = new HashSet<IEdge>();

        foreach (var (center, centers) in connectedCenters)
        {
            foreach (var other in centers)
            {
                var edge = new Edge(e: 0, new Point(center.X, center.Y), new Point(other.X, other.Y));
                edges.Add(edge);
            }
        }

        Visualizer.Show(context.Grid, connectedCenters.Keys.Select(x => (IPoint)new Point(x.X, x.Y)), edges);
    }
#endif

    private static HashSet<Location> FindTrunksAndConnect(Context context, Dictionary<Location, HashSet<Location>> centerToConnectedCenters)
    {
        var selectedTrunks = FindTrunks(context, centerToConnectedCenters);

        var allIncludedCenters = selectedTrunks.SelectMany(t => t.Centers).ToHashSet();

        var groups = selectedTrunks
            .Select(trunk =>
            {
                return new PumpjackGroup(centerToConnectedCenters, allIncludedCenters, trunk);
            })
            .ToList();

        if (groups.Count == 0)
        {
            var group = ConnectTwoClosestPumpjacks(context, centerToConnectedCenters, allIncludedCenters);

            groups.Add(group);
        }

        /*
        var clone = new PipeGrid(context.Grid);
        Visualizer.Show(clone, groups.SelectMany(g => g.Pipes).Distinct().Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
        */

        while (groups.Count > 1 || groups[0].IncludedCenters.Count < context.CenterToTerminals.Count)
        {
            var bestGroup = groups
                .Select(group =>
                {
                    var centroidX = group.Pipes.Average(l => l.X);
                    var centroidY = group.Pipes.Average(l => l.Y);

                    var bestCenter = group
                        .FrontierCenters
                        .Select(center =>
                        {
                            var includedCenter = group
                                .IncludedCenterToChildCenters
                                .First(p => p.Value.Contains(center))
                                .Key;

                            // Prefer the terminal that has the shortest path, then prefer the terminal closer to the centroid
                            // of the child (unconnected) pumpjacks.
                            var bestTerminal = context
                                .CenterToTerminals[center]
                                .Select(terminal =>
                                {
                                    List<Location> path = GetShortestPathToGroup(context, terminal, group, centroidX, centroidY);
                                    return new
                                    {
                                        Terminal = terminal,
                                        Path = path,
                                        ChildCentroidDistanceSquared = group.GetChildCentroidDistanceSquared(includedCenter, terminal.Terminal),
                                    };
                                })
                                .MinBy(t => (t.Path.Count, t.ChildCentroidDistanceSquared))!;

                            return new { BestTerminal = bestTerminal, Center = center };
                        })
                        .MinBy(t => (t.BestTerminal.Path.Count, t.BestTerminal.ChildCentroidDistanceSquared))!;

                    return new { BestCenter = bestCenter, Group = group };
                })
                .MinBy(t => (t.BestCenter.BestTerminal.Path.Count, t.BestCenter.BestTerminal.ChildCentroidDistanceSquared))!;

            var group = bestGroup.Group;
            var center = bestGroup.BestCenter.Center;
            var terminal = bestGroup.BestCenter.BestTerminal.Terminal;
            var path = bestGroup.BestCenter.BestTerminal.Path;

            if (allIncludedCenters.Contains(terminal.Center))
            {
                var otherGroup = groups.Single(g => g.IncludedCenters.Contains(terminal.Center));
                group.MergeGroup(otherGroup, path);
                groups.Remove(otherGroup);

                /*
                var clone2 = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone2, new(), context.CenterToTerminals, groups.SelectMany(g => g.Pipes).ToHashSet(), allowMultipleTerminals: true);
                Visualizer.Show(clone2, path.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                */
            }
            else
            {
                // Add the newly connected pumpjack to the current group.
                group.ConnectPumpjack(center, path);
                EliminateOtherTerminals(context, terminal);

                /*
                var clone2 = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone2, new(), context.CenterToTerminals, groups.SelectMany(g => g.Pipes).ToHashSet(), allowMultipleTerminals: true);
                Visualizer.Show(clone2, path.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                */
            }
        }

        return groups.Single().Pipes;
    }

    private static List<Location> GetShortestPathToGroup(Context context, TerminalLocation terminal, PumpjackGroup group, double groupCentroidX, double groupCentroidY)
    {
        try
        {
#if NO_SHARED_INSTANCES
            var aStarResultV = AStar.GetShortestPath(context.SharedInstances, context.Grid, terminal.Terminal, group.Pipes, xWeight: 2);
            var aStarResultH = AStar.GetShortestPath(context.SharedInstances, context.Grid, terminal.Terminal, group.Pipes, yWeight: 2);
#else
            var aStarResultV = AStar.GetShortestPath(context.SharedInstances, context.Grid, terminal.Terminal, group.Pipes, xWeight: 2, outputList: context.SharedInstances.LocationListA);
            var aStarResultH = AStar.GetShortestPath(context.SharedInstances, context.Grid, terminal.Terminal, group.Pipes, yWeight: 2, outputList: context.SharedInstances.LocationListB);
#endif

            if (aStarResultV.ReachedGoal is null)
            {
                throw new NoPathBetweenTerminalsException(terminal.Terminal, group.Pipes.First());
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

#if NO_SHARED_INSTANCES
            var locationToCentroidDistanceSquared = new Dictionary<Location, double>(sizeEstimate);
#else
            var locationToCentroidDistanceSquared = context.SharedInstances.LocationToDouble;
#endif

            try
            {
                for (var i = 0; i < Math.Max(aStarResultV.Path.Count, aStarResultH.Path.Count); i++)
                {
                    if (i < aStarResultV.Path.Count)
                    {
                        var location = aStarResultV.Path[i];
                        if (context.LocationToAdjacentCount[location.X, location.Y] > 0)
                        {
                            adjacentPipesV++;
                        }

                        centroidDistanceSquaredV += GetCentroidDistanceSquared(groupCentroidX, groupCentroidY, locationToCentroidDistanceSquared, location);
                    }

                    if (i < aStarResultH.Path.Count)
                    {
                        var location = aStarResultH.Path[i];
                        if (context.LocationToAdjacentCount[location.X, location.Y] > 0)
                        {
                            adjacentPipesH++;
                        }

                        centroidDistanceSquaredH += GetCentroidDistanceSquared(groupCentroidX, groupCentroidY, locationToCentroidDistanceSquared, location);
                    }
                }
            }
            finally
            {
#if !NO_SHARED_INSTANCES
                locationToCentroidDistanceSquared.Clear();
#endif
            }

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
        }
        finally
        {
#if !NO_SHARED_INSTANCES
            context.SharedInstances.LocationListA.Clear();
            context.SharedInstances.LocationListB.Clear();
#endif
        }
    }

    private static double GetCentroidDistanceSquared(
        double groupCentroidX,
        double groupCentroidY,
        Dictionary<Location, double> locationToCentroidDistanceSquared,
        Location location)
    {
        if (!locationToCentroidDistanceSquared.TryGetValue(location, out var centroidDistanceSquared))
        {
            centroidDistanceSquared = location.GetEuclideanDistanceSquared(groupCentroidX, groupCentroidY);
            locationToCentroidDistanceSquared.Add(location, centroidDistanceSquared);
        }

        return centroidDistanceSquared;
    }

    private static List<Trunk> FindTrunks(Context context, Dictionary<Location, HashSet<Location>> centerToConnectedCenters)
    {
        /*
        Visualizer.Show(context.Grid, Array.Empty<IPoint>(), centerToConnectedCenters
            .SelectMany(p => p.Value.Select(o => (p.Key, o))
            .Select(p => (IEdge)new Edge(0, new Point(p.Key.X, p.Key.Y), new Point(p.o.X, p.o.Y)))
            .Distinct()));
        */

        var trunkCandidates = GetTrunkCandidates(context, centerToConnectedCenters);

        trunkCandidates = trunkCandidates
            .OrderByDescending(t => t.TerminalLocations.Count)
            .ThenBy(t => t.Length)
            .ThenBy(t =>
            {
                var neighbors = t.Centers.SelectMany(c => centerToConnectedCenters[c]).Except(t.Centers).ToHashSet();
                if (neighbors.Count == 0)
                {
                    return 0;
                }

                var centroidX = neighbors.Average(l => l.X);
                var centroidY = neighbors.Average(l => l.Y);
                return t.Start.GetEuclideanDistance(centroidX, centroidY) + t.End.GetEuclideanDistance(centroidX, centroidY);
            })
            .ToList();

        // Eliminate lower priority trunks that have any pipes shared with higher priority trunks.
        var includedPipes = new HashSet<Location>();
        var includedCenters = new HashSet<Location>();
        var selectedTrunks = new List<Trunk>();
        foreach (var trunk in trunkCandidates)
        {
            var path = MakeStraightLine(trunk.Start, trunk.End);
            if (!includedPipes.Intersect(path).Any() && !includedCenters.Intersect(trunk.Centers).Any())
            {
                selectedTrunks.Add(trunk);
                includedPipes.UnionWith(path);
                includedCenters.UnionWith(trunk.Centers);
            }
        }

        /*
        for (var i = 1; i <= selectedTrunks.Count; i++)
        {
            Visualizer.Show(context.Grid, selectedTrunks.Take(i).SelectMany(t => t.Centers).Distinct().Select(l => (IPoint)new Point(l.X, l.Y)), selectedTrunks
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

        // Visualize(context, locationToPoint, selectedTrunks.SelectMany(t => MakeStraightLine(t.Start, t.End)).ToHashSet());

        // Find the "child" unconnected pumpjacks of each connected pumpjack. These are pumpjacks are connected via the
        // given connected pumpjack.
        return selectedTrunks;
    }

    private static PumpjackGroup ConnectTwoClosestPumpjacks(Context context, Dictionary<Location, HashSet<Location>> centerToConnectedCenters, HashSet<Location> allIncludedCenters)
    {
        var bestConnection = centerToConnectedCenters
            .Keys
            .Select(center =>
            {
                return context
                    .CenterToTerminals[center]
                    .Select(terminal =>
                    {
                        var bestTerminal = centerToConnectedCenters[center]
                            .Select(otherCenter =>
                            {
                                var goals = context.CenterToTerminals[otherCenter].Select(t => t.Terminal).ToHashSet();
                                var result = AStar.GetShortestPath(context.SharedInstances, context.Grid, terminal.Terminal, goals);
                                var reachedGoal = result.ReachedGoal!.Value;
                                var closestTerminal = context.CenterToTerminals[otherCenter].Single(t => t.Terminal == reachedGoal);
                                var path = result.Path;

                                return new { Terminal = closestTerminal, Path = path };
                            })
                            .MinBy(t => t.Path.Count)!;

                        return new { Terminal = terminal, BestTerminal = bestTerminal };
                    })
                    .MinBy(t => t.BestTerminal.Path.Count)!;
            })
            .MinBy(t => (
                t.BestTerminal.Path.Count,
                -centerToConnectedCenters[t.Terminal.Center].Count,
                -centerToConnectedCenters[t.BestTerminal.Terminal.Center].Count,
                t.Terminal.Terminal.GetEuclideanDistanceSquared(context.Grid.Middle),
                t.BestTerminal.Terminal.Terminal.GetEuclideanDistanceSquared(context.Grid.Middle)
            ))!;

        EliminateOtherTerminals(context, bestConnection.Terminal);
        EliminateOtherTerminals(context, bestConnection.BestTerminal.Terminal);

        var group = new PumpjackGroup(
            centerToConnectedCenters,
            allIncludedCenters,
            new[] { bestConnection.Terminal.Center, bestConnection.BestTerminal.Terminal.Center },
            bestConnection.BestTerminal.Path);

        return group;
    }

    private static HashSet<Location> GetChildCenters(
        Dictionary<Location, HashSet<Location>> centerToConnectedCenters,
        HashSet<Location> ignoreCenters,
        HashSet<Location> shallowExploreCenters,
        Location startingCenter)
    {
        var queue = new Queue<(Location Location, bool ShouldRecurse)>();
        var visited = new HashSet<Location>();
        queue.Enqueue((startingCenter, ShouldRecurse: true));

        while (queue.Count > 0)
        {
            (var current, var shouldRecurse) = queue.Dequeue();
            if (!visited.Add(current) || !shouldRecurse)
            {
                continue;
            }

            foreach (var other in centerToConnectedCenters[current])
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

    private static List<Trunk> GetTrunkCandidates(Context context, Dictionary<Location, HashSet<Location>> centerToConnectedCenters)
    {
        var centerToMaxX = context
            .CenterToTerminals
            .Keys
            .ToDictionary(c => c, c => centerToConnectedCenters[c].Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.X)));
        var centerToMaxY = context
            .CenterToTerminals
            .Keys
            .ToDictionary(c => c, c => centerToConnectedCenters[c].Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.Y)));

        // Find paths that connect the most terminals of neighboring pumpjacks.
        var trunkCandidates = new List<Trunk>();
        foreach (var translation in Translations)
        {
            foreach (var startingCenter in context.CenterToTerminals.Keys.OrderBy(c => c.Y).ThenBy(c => c.X))
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
                            var centers = terminals.Select(t => t.Center);
                            var matchedCenters = centers.Intersect(nextCenters).ToHashSet();
                            if (matchedCenters.Count == 0)
                            {
                                // The pumpjack terminal we ran into does not belong to the a pumpjack that the current
                                // pumpjack should be connected to.
                                break;
                            }

                            var nextCenter = matchedCenters.First();

                            if (!expandedChildCenters)
                            {
                                nextCenters = GetChildCenters(
                                    centerToConnectedCenters,
                                    ignoreCenters: new HashSet<Location> { currentCenter },
                                    shallowExploreCenters: new HashSet<Location> { nextCenter },
                                    nextCenter);

                                if (nextCenters.Count == 0)
                                {
                                    break;
                                }

                                maxX = nextCenters.Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.X));
                                maxY = nextCenters.Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.Y));
                                expandedChildCenters = true;
                            }

                            if (trunk is null)
                            {
                                trunk = new Trunk(terminal, currentCenter);
                            }

                            trunk.Terminals.AddRange(terminals);
                            trunk.TerminalLocations.UnionWith(terminals.Select(t => t.Terminal));
                            trunk.Centers.UnionWith(centers);

                            currentCenter = nextCenter;
                        }

                        location = location.Translate(translation);
                    }

                    if (trunk is not null && trunk.Centers.Count > 1)
                    {
                        trunkCandidates.Add(trunk);
                    }
                }
            }
        }

        return trunkCandidates;
    }

    private class Trunk
    {
        public Trunk(TerminalLocation startingTerminal, Location center)
        {
            Terminals.Add(startingTerminal);
            TerminalLocations.Add(startingTerminal.Terminal);
            Centers.Add(center);
        }

        public List<TerminalLocation> Terminals { get; } = new List<TerminalLocation>(2);
        public HashSet<Location> TerminalLocations { get; } = new HashSet<Location>(2);
        public HashSet<Location> Centers { get; } = new HashSet<Location>(2);
        public int Length => Start.GetManhattanDistance(End) + 1;
        public Location Start => Terminals[0].Terminal;
        public Location End => Terminals.Last().Terminal;

        public override string ToString()
        {
            return $"{Start} -> {End}";
        }
    }

    private class PumpjackGroup
    {
        private readonly Dictionary<Location, HashSet<Location>> _centerToConnectedCenters;
        private readonly HashSet<Location> _allIncludedCenters;

        public PumpjackGroup(Dictionary<Location, HashSet<Location>> centerToConnectedCenters, HashSet<Location> allIncludedCenters, Trunk trunk)
            : this(centerToConnectedCenters, allIncludedCenters, trunk.Centers, MakeStraightLine(trunk.Start, trunk.End))
        {
        }

        public PumpjackGroup(Dictionary<Location, HashSet<Location>> centerToConnectedCenters, HashSet<Location> allIncludedCenters, IEnumerable<Location> includedCenters, IEnumerable<Location> pipes)
        {
            _centerToConnectedCenters = centerToConnectedCenters;
            _allIncludedCenters = allIncludedCenters;

            IncludedCenters = new HashSet<Location>(includedCenters);

            FrontierCenters = new HashSet<Location>();
            IncludedCenterToChildCenters = new Dictionary<Location, HashSet<Location>>();

            Pipes = new HashSet<Location>(pipes);

            UpdateFrontierCenters();
            UpdateIncludedCenterToChildCenters();
        }

        public HashSet<Location> IncludedCenters { get; }
        public HashSet<Location> FrontierCenters { get; }
        public Dictionary<Location, HashSet<Location>> IncludedCenterToChildCenters { get; }
        public HashSet<Location> Pipes { get; }

        public double GetChildCentroidDistanceSquared(Location includedCenter, Location terminalCandidate)
        {
            var sumX = 0;
            var sumY = 0;
            var count = 0;
            foreach (var center in IncludedCenterToChildCenters[includedCenter])
            {
                sumX += center.X;
                sumY += center.Y;
                count++;
            }

            var centroidX = (double)sumX / count;
            var centroidY = (double)sumY / count;

            return terminalCandidate.GetEuclideanDistanceSquared(centroidX, centroidY);
        }

        public void ConnectPumpjack(Location center, IEnumerable<Location> path)
        {
            _allIncludedCenters.Add(center);
            IncludedCenters.Add(center);
            Pipes.UnionWith(path);
            UpdateFrontierCenters();
            UpdateIncludedCenterToChildCenters();
        }

        public void MergeGroup(PumpjackGroup other, IEnumerable<Location> path)
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

            foreach (var center in IncludedCenters)
            {
                FrontierCenters.UnionWith(_centerToConnectedCenters[center]);
            }

            FrontierCenters.ExceptWith(IncludedCenters);
        }

        private void UpdateIncludedCenterToChildCenters()
        {
            IncludedCenterToChildCenters.Clear();

            foreach (var center in IncludedCenters)
            {
                HashSet<Location> visited = GetChildCenters(
                    _centerToConnectedCenters,
                    IncludedCenters,
                    _allIncludedCenters,
                    center);

                IncludedCenterToChildCenters.Add(center, visited);
            }
        }
    }
}
