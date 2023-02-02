using System.Data;
using PumpjackPipeOptimizer.Algorithms;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

internal static partial class PlanPipes
{
    private static readonly IReadOnlyList<(int DeltaX, int DeltaY)> Translations = new[] { (1, 0), (0, 1) };

    private static Dictionary<Location, HashSet<Location>> GetConnectedPumpjacks(Context context, PlanPipesStrategy strategy)
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

        return strategy switch
        {
            PlanPipesStrategy.ConnectedCenters_Delaunay => GetConnectedPumpjacksWithDelaunay(centers),
            PlanPipesStrategy.ConnectedCenters_DelaunayMst => GetConnectedPumpjacksWithDelaunayMst(context, centers),
            PlanPipesStrategy.ConnectedCenters_FLUTE => GetConnectedPumpjacksWithFLUTE(context),
            _ => throw new NotImplementedException(),
        };
    }

    private static HashSet<Location> FindTrunksAndConnect(Context context, Dictionary<Location, HashSet<Location>> centerToConnectedCenters)
    {
        var selectedTrunks = FindTrunks(context, centerToConnectedCenters);

        var allIncludedCenters = selectedTrunks.SelectMany(t => t.Centers).ToHashSet();

        var groups = selectedTrunks
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

        // Visualize(context, locationToPoint, groups.SelectMany(g => g.Pipes).ToHashSet());

        while (groups.Count > 1 || groups[0].IncludedCenters.Count < context.CenterToTerminals.Count)
        {
            var bestGroup = groups
                .Select(group =>
                {
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
                                    var result = Dijkstras.GetShortestPaths(context.Grid, terminal.Terminal, group.Pipes, stopOnFirstGoal: true);
                                    var paths = result.GetStraightPaths(result.ReachedGoals.Single());

                                    List<Location> path;
                                    if (paths.Count > 1)
                                    {
                                        // Prefer the path that is adjacent to more pumpjack sides, then prefer one that is
                                        // cumulatively closer to the group centroid.
                                        var pathToAdjacentPipes = paths.ToDictionary(p => p, p => p.Count(l => context
                                            .Grid
                                            .GetAdjacent(l)
                                            .Any(a => context.Grid.IsEntityType<PumpjackSide>(a))));

                                        var pathToCentroidDistance = paths.ToDictionary(p => p, p => p.Sum(l => group.GetCentroidDistance(l)));

                                        path = paths
                                            .OrderByDescending(p => pathToAdjacentPipes[p])
                                            .ThenBy(p => pathToCentroidDistance[p])
                                            .First();
                                    }
                                    else
                                    {
                                        path = paths.Single();
                                    }


                                    return new
                                    {
                                        Terminal = terminal,
                                        Path = path,
                                        ChildCentroidDistance = group.GetChildCentroidDistance(includedCenter, terminal.Terminal),
                                    };
                                })
                                .OrderBy(t => t.Path.Count)
                                .ThenBy(t => t.ChildCentroidDistance)
                                .First();

                            return new { BestTerminal = bestTerminal, Center = center };
                        })
                        .OrderBy(t => t.BestTerminal.Path.Count)
                        .ThenBy(t => t.BestTerminal.ChildCentroidDistance)
                        .First();

                    return new { BestCenter = bestCenter, Group = group };
                })
                .OrderBy(t => t.BestCenter.BestTerminal.Path.Count)
                .ThenBy(t => t.BestCenter.BestTerminal.ChildCentroidDistance)
                .First();

            var group = bestGroup.Group;
            var center = bestGroup.BestCenter.Center;
            var terminal = bestGroup.BestCenter.BestTerminal.Terminal;
            var path = bestGroup.BestCenter.BestTerminal.Path;

            if (allIncludedCenters.Contains(terminal.Center))
            {
                var otherGroup = groups.Single(g => g.IncludedCenters.Contains(terminal.Center));
                group.MergeGroup(otherGroup, path);
                groups.Remove(otherGroup);

                // Visualize(context, locationToPoint, groups.SelectMany(g => g.Pipes).ToHashSet());
            }
            else
            {
                // Add the newly connected pumpjack to the current group.
                group.ConnectPumpjack(center, path);
                EliminateOtherTerminals(context, terminal);

                // Visualize(context, locationToPoint, groups.SelectMany(g => g.Pipes).ToHashSet());
            }
        }

        return groups.Single().Pipes;
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
            .OrderByDescending(t => t.Centers.Count)
            .ThenBy(t => t.Length)
            .ThenBy(t =>
            {
                var neighbors = t.Centers.SelectMany(c => centerToConnectedCenters[c]).Except(t.Centers).ToHashSet();
                var centroidX = neighbors.Average(l => l.X);
                var centroidY = neighbors.Average(l => l.Y);
                return GetEuclideanDistance(t.Start, centroidX, centroidY) + GetEuclideanDistance(t.End, centroidX, centroidY);
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
                                var result = Dijkstras.GetShortestPaths(context.Grid, terminal.Terminal, goals, stopOnFirstGoal: true);
                                var reachedGoal = result.ReachedGoals.Single();
                                var closestTerminal = context.CenterToTerminals[otherCenter].Single(t => t.Terminal == reachedGoal);
                                var path = result.GetStraightPaths(reachedGoal).First();

                                return new { Terminal = closestTerminal, Path = path };
                            })
                            .OrderBy(t => t.Path.Count)
                            .First();

                        return new { Terminal = terminal, BestTerminal = bestTerminal };
                    })
                    .OrderBy(t => t.BestTerminal.Path.Count)
                    .First();
            })
            .OrderBy(t => t.BestTerminal.Path.Count)
            .ThenByDescending(t => centerToConnectedCenters[t.Terminal.Center].Count)
            .ThenByDescending(t => centerToConnectedCenters[t.BestTerminal.Terminal.Center].Count)
            .ThenBy(t => t.Terminal.Terminal.GetEuclideanDistance(context.Grid.Middle))
            .ThenBy(t => t.BestTerminal.Terminal.Terminal.GetEuclideanDistance(context.Grid.Middle))
            .First();

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
        var locationToTerminals = context
            .CenterToTerminals
            .Values
            .SelectMany(ts => ts)
            .GroupBy(t => t.Terminal)
            .ToDictionary(t => t.Key, t => t.ToHashSet());

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

                    var trunk = new Trunk(terminal, currentCenter);

                    while (location.X <= maxX && location.Y <= maxY && context.Grid.IsEmpty(location))
                    {
                        if (locationToTerminals.TryGetValue(location, out var terminals))
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

                            trunk.Terminals.AddRange(terminals);
                            trunk.Centers.Add(nextCenter);

                            currentCenter = nextCenter;
                        }

                        location = location.Translate(translation);
                    }

                    if (trunk.Centers.Count > 1)
                    {
                        trunkCandidates.Add(trunk);
                    }
                }
            }
        }

        return trunkCandidates;
    }

    private static double GetEuclideanDistance(Location a, double bX, double bY)
    {
        return Math.Sqrt(Math.Pow(a.X - bX, 2) + Math.Pow(a.Y - bY, 2));
    }

    private static List<Location> MakeStraightLine(Location a, Location b)
    {
        if (a.X == b.X)
        {
            return Enumerable
                .Range(Math.Min(a.Y, b.Y), Math.Abs(a.Y - b.Y) + 1)
                .Select(y => new Location(a.X, y))
                .ToList();
        }

        if (a.Y == b.Y)
        {
            return Enumerable
                .Range(Math.Min(a.X, b.X), Math.Abs(a.X - b.X) + 1)
                .Select(x => new Location(x, a.Y))
                .ToList();
        }

        throw new ArgumentException("The two points must be one the same line either horizontally or vertically.");
    }

    private static void EliminateOtherTerminals(Context context, TerminalLocation selectedTerminal)
    {
        var terminalOptions = context.CenterToTerminals[selectedTerminal.Center];

        if (terminalOptions.Count == 1)
        {
            return;
        }

        terminalOptions.Clear();
        terminalOptions.Add(selectedTerminal);
    }

    private class Trunk
    {
        public Trunk(TerminalLocation startingTerminal, Location center)
        {
            Terminals.Add(startingTerminal);
            Centers.Add(center);
        }

        public List<TerminalLocation> Terminals { get; } = new List<TerminalLocation>();
        public HashSet<Location> Centers { get; } = new HashSet<Location>();
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
        private readonly Context _context;
        private readonly Dictionary<Location, HashSet<Location>> _centerToConnectedCenters;
        private readonly HashSet<Location> _allIncludedCenters;

        public PumpjackGroup(Context context, Dictionary<Location, HashSet<Location>> centerToConnectedCenters, HashSet<Location> allIncludedCenters, Trunk trunk)
            : this(context, centerToConnectedCenters, allIncludedCenters, trunk.Centers, MakeStraightLine(trunk.Start, trunk.End))
        {
        }

        public PumpjackGroup(Context context, Dictionary<Location, HashSet<Location>> centerToConnectedCenters, HashSet<Location> allIncludedCenters, IEnumerable<Location> includedCenters, IEnumerable<Location> pipes)
        {
            _context = context;
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

        public double GetCentroidDistance(Location location)
        {
            var centroidX = Pipes.Average(l => l.X);
            var centroidY = Pipes.Average(l => l.Y);
            return GetEuclideanDistance(location, centroidX, centroidY);
        }

        public double GetChildCentroidDistance(Location includedCenter, Location terminalCandidate)
        {
            var centers = IncludedCenterToChildCenters[includedCenter];
            var terminals = centers.SelectMany(c => _context.CenterToTerminals[c]);
            var centroidX = terminals.Average(t => t.Terminal.X);
            var centroidY = terminals.Average(t => t.Terminal.Y);

            return GetEuclideanDistance(terminalCandidate, centroidX, centroidY);
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
