using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Cathei.LinqGen;
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
        var centers = context
            .CenterToTerminals
            .Keys
            .Gen()
            .OrderBy(c => c.X)
            .ThenBy(c => c.Y)
            .ToList();

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
            var connected = centers.ToDictionary(context, c => c, c => context.GetLocationSet());
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

        Visualizer.Show(context.Grid, connectedCenters.Keys.Select(x => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(x.X, x.Y)), edges);
    }
#endif

    private static ILocationSet FindTrunksAndConnect(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters)
    {
        var selectedTrunks = FindTrunks(context, centerToConnectedCenters);

        var allIncludedCenters = selectedTrunks.SelectMany(t => t.Centers.EnumerateItems()).ToSet(context);

        var groups = selectedTrunks
            .Gen()
            .Select(trunk =>
            {
                return new PumpjackGroup(context, centerToConnectedCenters, allIncludedCenters, trunk);
            })
            .ToList();

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
            var bestGroup = groups
                .Gen()
                .Select(group =>
                {
                    var centroidX = group.Pipes.EnumerateItems().Average(l => l.X);
                    var centroidY = group.Pipes.EnumerateItems().Average(l => l.Y);

                    var bestCenter = group
                        .FrontierCenters
                        .EnumerateItems()
                        .Gen()
                        .Select(center =>
                        {
                            var includedCenter = group
                                .IncludedCenterToChildCenters
                                .EnumeratePairs()
                                .First(p => p.Value.Contains(center))
                                .Key;

                            // Prefer the terminal that has the shortest path, then prefer the terminal closer to the centroid
                            // of the child (unconnected) pumpjacks.
                            var bestTerminal = context
                                .CenterToTerminals[center]
                                .Gen()
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
                                .MinBy(t => Tuple.Create(t.Path.Count, t.ChildCentroidDistanceSquared))!;

                            return KeyValuePair.Create(bestTerminal, center);
                        })
                        .MinBy(t => Tuple.Create(t.Key.Path.Count, t.Key.ChildCentroidDistanceSquared))!;

                    return KeyValuePair.Create(bestCenter, group);
                })
                .MinBy(t => Tuple.Create(t.Key.Key.Path.Count, t.Key.Key.ChildCentroidDistanceSquared))!;

            var group = bestGroup.Value;
            var center = bestGroup.Key.Value;
            var terminal = bestGroup.Key.Key.Terminal;
            var path = bestGroup.Key.Key.Path;

            if (allIncludedCenters.Contains(terminal.Center))
            {
                var otherGroup = groups.Single(g => g.IncludedCenters.Contains(terminal.Center));
                group.MergeGroup(otherGroup, path);
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
                group.ConnectPumpjack(center, path);
                EliminateOtherTerminals(context, terminal);

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
        try
        {
#if USE_SHARED_INSTANCES
            var aStarResultV = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, xWeight: 2, outputList: context.SharedInstances.LocationListA);
            var aStarResultH = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, yWeight: 2, outputList: context.SharedInstances.LocationListB);
#else
            var aStarResultV = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, xWeight: 2);
            var aStarResultH = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, yWeight: 2);
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

#if USE_SHARED_INSTANCES
            var locationToCentroidDistanceSquared = context.SharedInstances.LocationToDouble;
#else
            var locationToCentroidDistanceSquared = context.GetLocationDictionary<double>(sizeEstimate);
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
#if USE_SHARED_INSTANCES
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
#if USE_SHARED_INSTANCES
            context.SharedInstances.LocationListA.Clear();
            context.SharedInstances.LocationListB.Clear();
#endif
        }
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

        trunkCandidates = trunkCandidates
            .Gen()
            .OrderByDescending(t => t.TerminalLocations.Count)
            .ThenBy(t => t.Length)
            .ThenBy(t =>
            {
                var neighbors = t
                    .Centers
                    .EnumerateItems()
                    .SelectMany(c => centerToConnectedCenters[c].EnumerateItems())
                    .ToSet(context, allowEnumerate: true);

                neighbors.ExceptWith(t.Centers);

                if (neighbors.Count == 0)
                {
                    return 0;
                }

                var centroidX = neighbors.EnumerateItems().Average(l => l.X);
                var centroidY = neighbors.EnumerateItems().Average(l => l.Y);
                return t.Start.GetEuclideanDistance(centroidX, centroidY) + t.End.GetEuclideanDistance(centroidX, centroidY);
            })
            .ToList();

        // Eliminate lower priority trunks that have any pipes shared with higher priority trunks.
        var includedPipes = context.GetLocationSet();
        var includedCenters = context.GetLocationSet(allowEnumerate: true);
        var selectedTrunks = new List<Trunk>();
        foreach (var trunk in trunkCandidates)
        {
            var path = MakeStraightLine(trunk.Start, trunk.End);
            if (!path.Any(includedPipes.Contains) && !includedCenters.Overlaps(trunk.Centers.EnumerateItems()))
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

    private static PumpjackGroup ConnectTwoClosestPumpjacks(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters, ILocationSet allIncludedCenters)
    {
        var bestConnection = centerToConnectedCenters
            .Keys
            .Gen()
            .Select(center =>
            {
                return context
                    .CenterToTerminals[center]
                    .Gen()
                    .Select(terminal =>
                    {
                        var bestTerminal = centerToConnectedCenters[center]
                            .EnumerateItems()
                            .Select(otherCenter =>
                            {
                                var goals = context.CenterToTerminals[otherCenter].Select(t => t.Terminal).ToReadOnlySet(context, allowEnumerate: true);
                                var result = AStar.GetShortestPath(context, context.Grid, terminal.Terminal, goals);
                                if (!result.Success)
                                {
                                    throw new NoPathBetweenTerminalsException(terminal.Terminal, goals.EnumerateItems().First());
                                }
                                var reachedGoal = result.ReachedGoal;
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
            context,
            centerToConnectedCenters,
            allIncludedCenters,
            new[] { bestConnection.Terminal.Center, bestConnection.BestTerminal.Terminal.Center },
            bestConnection.BestTerminal.Path);

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
            .CenterToTerminals
            .Keys
            .ToDictionary(context, c => c, c => centerToConnectedCenters[c].EnumerateItems().Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.X)));
        var centerToMaxY = context
            .CenterToTerminals
            .Keys
            .ToDictionary(context, c => c, c => centerToConnectedCenters[c].EnumerateItems().Max(c => context.CenterToTerminals[c].Max(t => t.Terminal.Y)));

        // Find paths that connect the most terminals of neighboring pumpjacks.
        var trunkCandidates = new List<Trunk>();
        foreach (var translation in Translations)
        {
            foreach (var startingCenter in context.CenterToTerminals.Keys.Gen().OrderBy(c => c.Y).ThenBy(c => c.X))
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
                            Location nextCenter = default;
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
                            trunk.TerminalLocations.UnionWith(terminals.Select(t => t.Terminal));
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
                        trunkCandidates.Add(trunk);
                    }
                }
            }
        }

        return trunkCandidates;
    }

    internal class Trunk
    {
        public Trunk(Context context, TerminalLocation startingTerminal, Location center)
        {
            TerminalLocations = context.GetLocationSet(startingTerminal.Terminal, capacity: 2);
            Terminals.Add(startingTerminal);
            Centers = context.GetLocationSet(center, capacity: 2, allowEnumerate: true);
        }

        public List<TerminalLocation> Terminals { get; } = new List<TerminalLocation>(2);
        public ILocationSet TerminalLocations { get; }
        public ILocationSet Centers { get; }
        public int Length => Start.GetManhattanDistance(End) + 1;
        public Location Start => Terminals[0].Terminal;
        public Location End => Terminals.Gen().Last().Terminal;

#if ENABLE_GRID_TOSTRING
        public override string ToString()
        {
            return $"{Start} -> {End}";
        }
#endif
    }

    internal class PumpjackGroup
    {
        private readonly Context _context;
        private readonly ILocationDictionary<ILocationSet> _centerToConnectedCenters;
        private readonly ILocationSet _allIncludedCenters;

        public PumpjackGroup(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters, ILocationSet allIncludedCenters, Trunk trunk)
            : this(context, centerToConnectedCenters, allIncludedCenters, trunk.Centers.EnumerateItems(), MakeStraightLine(trunk.Start, trunk.End))
        {
        }

        public PumpjackGroup(Context context, ILocationDictionary<ILocationSet> centerToConnectedCenters, ILocationSet allIncludedCenters, IEnumerable<Location> includedCenters, IEnumerable<Location> pipes)
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
