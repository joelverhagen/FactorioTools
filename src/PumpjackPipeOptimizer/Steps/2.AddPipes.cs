using System.Data;
using System.Reflection;
using DelaunatorSharp;
using Knapcode.FluteSharp;
using PumpjackPipeOptimizer.Algorithms;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

internal static class AddPipes
{
    private static readonly Lazy<FLUTE> LazyFlute = new Lazy<FLUTE>(() =>
    {
        var d = 9;
        var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        using var powvStream = File.OpenRead(Path.Combine(assemblyDir, $"POWV{d}.dat"));
        using var postStream = File.OpenRead(Path.Combine(assemblyDir, $"POST{d}.dat"));
        var lookUpTable = new LookUpTable(d, powvStream, postStream);
        return new FLUTE(lookUpTable, maxD: 200);
    });

    private static FLUTE FLUTE => LazyFlute.Value;

    private class FlutePoint
    {
        public FlutePoint(Location location)
        {
            Location = location;
        }

        public bool IsEliminated { get; set; }
        public bool IsSteinerPoint => Centers.Count == 0;
        public Location Location { get; }
        public HashSet<Location> Centers { get; } = new HashSet<Location>();
        public HashSet<TerminalLocation> Terminals { get; } = new HashSet<TerminalLocation>();
        public HashSet<FlutePoint> Neighbors { get; } = new HashSet<FlutePoint>();

        public override string ToString()
        {
            return Location.ToString();
        }
    }

    private static readonly IReadOnlyList<(int DeltaX, int DeltaY)> Translations = new[] { (1, 0), (0, 1) };

    private class Trunk
    {
        public Trunk(FlutePoint startingPoint, Location center)
        {
            Points.Add(startingPoint);
            Centers.Add(center);
        }

        public List<FlutePoint> Points { get; } = new List<FlutePoint>();
        public HashSet<Location> Centers { get; } = new HashSet<Location>();
        public int Length => Start.GetManhattanDistance(End);
        public Location Start => Points[0].Location;
        public Location End => Points.Last().Location;
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
                var queue = new Queue<(Location Location, bool ShouldRecurse)>();
                var visited = new HashSet<Location>();
                queue.Enqueue((center, ShouldRecurse: true));

                while (queue.Count > 0)
                {
                    (var current, var shouldRecurse) = queue.Dequeue();
                    if (!visited.Add(current) || !shouldRecurse)
                    {
                        continue;
                    }

                    foreach (var other in _centerToConnectedCenters[current])
                    {
                        if (IncludedCenters.Contains(other))
                        {
                            continue;
                        }

                        // If the other center is in another group, don't recursively explore.
                        queue.Enqueue((other, ShouldRecurse: !_allIncludedCenters.Contains(other)));
                    }
                }

                visited.Remove(center);
                IncludedCenterToChildCenters.Add(center, visited);
            }
        }
    }

    private record ClosestTerminal(TerminalLocation Terminal, List<Location> Path, double ChildCentroidDistance);

    private static double GetEuclideanDistance(Location a, double bX, double bY)
    {
        return Math.Sqrt(Math.Pow(a.X - bX, 2) + Math.Pow(a.Y - bY, 2));
    }

    public static HashSet<Location> Execute(Context context)
    {
        var locationToPoint = GetLocationToFlutePoint(context);

        var centerToPoints = context
            .CenterToTerminals
            .ToDictionary(p => p.Key, p => p.Value.Select(t => locationToPoint[t.Terminal]).ToHashSet());

        var centerToConnectedCenters = GetConnectedPumpjacks(context, centerToPoints);

        var trunkCandidates = GetTrunkCandidates(context, locationToPoint, centerToConnectedCenters);

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

        // Eliminate lower priority trunks that have pumpjacks shared with higher priority trunks.
        var includedCenters = new HashSet<Location>();
        var selectedTrunks = new List<Trunk>();
        foreach (var trunk in trunkCandidates)
        {
            if (!includedCenters.Intersect(trunk.Centers).Any())
            {
                selectedTrunks.Add(trunk);
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
            foreach (var point in trunk.Points)
            {
                foreach (var terminal in point.Terminals)
                {
                    EliminateOtherTerminals(context, locationToPoint, terminal);
                }
            }
        }

        // Visualize(context, locationToPoint, selectedTrunks.SelectMany(t => MakeStraightLine(t.Start, t.End)).ToHashSet());

        // Find the "child" unconnected pumpjacks of each connected pumpjack. These are pumpjacks are connected via the
        // given connected pumpjack.

        var allIncludedCenters = selectedTrunks.SelectMany(t => t.Centers).ToHashSet();

        var groups = selectedTrunks
            .Select(trunk =>
            {
                return new PumpjackGroup(context, centerToConnectedCenters, allIncludedCenters, trunk);
            })
            .ToList();

        if (groups.Count == 0)
        {
            // If there are no groups at all, create an arbitrary one with the two pumpjacks that have the shortest
            // connection.
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

            EliminateOtherTerminals(context, locationToPoint, bestConnection.Terminal);
            EliminateOtherTerminals(context, locationToPoint, bestConnection.BestTerminal.Terminal);

            var group = new PumpjackGroup(
                context,
                centerToConnectedCenters,
                allIncludedCenters,
                new[] { bestConnection.Terminal.Center, bestConnection.BestTerminal.Terminal.Center },
                bestConnection.BestTerminal.Path);

            groups.Add(group);
        }

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

                                    // Prefer the path that is cumulatively closer to the group centroid.
                                    var path = paths
                                        .OrderBy(p => p.Sum(l => group.GetCentroidDistance(l)))
                                        .First();

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
                EliminateOtherTerminals(context, locationToPoint, terminal);

                // Visualize(context, locationToPoint, groups.SelectMany(g => g.Pipes).ToHashSet());
            }
        }

        // Visualize(context, locationToPoint, groups.SelectMany(g => g.Pipes).ToHashSet());

        AddGridEntities(context, groups.Single().Pipes);

        return groups.Single().Pipes;
    }

    private static void AddGridEntities(Context context, HashSet<Location> pipes)
    {
        var addedTerminals = new HashSet<Location>();
        foreach (var terminals in context.CenterToTerminals.Values)
        {
            var location = terminals.Single().Terminal;
            if (addedTerminals.Add(location))
            {
                context.Grid.AddEntity(location, new Terminal());
            }
        }

        foreach (var pipe in pipes.Except(addedTerminals))
        {
            context.Grid.AddEntity(pipe, new Pipe());
        }
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

    private static List<Trunk> GetTrunkCandidates(Context context, Dictionary<Location, FlutePoint> locationToPoint, Dictionary<Location, HashSet<Location>> centerToConnectedCenters)
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
            foreach (var startingCenter in context.CenterToTerminals.Keys.OrderBy(c => c.X).ThenBy(c => c.Y))
            {
                foreach (var terminal in context.CenterToTerminals[startingCenter])
                {
                    var currentCenter = startingCenter;
                    var nextCenters = centerToConnectedCenters[currentCenter];
                    var maxX = centerToMaxX[currentCenter];
                    var maxY = centerToMaxY[currentCenter];

                    var location = terminal.Terminal.Translate(translation);

                    var trunk = new Trunk(locationToPoint[terminal.Terminal], currentCenter);

                    while (location.X <= maxX && location.Y <= maxY && context.Grid.IsEmpty(location))
                    {
                        if (locationToPoint.TryGetValue(location, out var point))
                        {
                            if (point.IsSteinerPoint)
                            {
                                trunk.Points.Add(point);
                            }
                            else
                            {
                                var matchedCenters = point.Centers.Intersect(nextCenters).ToHashSet();
                                if (matchedCenters.Count == 0)
                                {
                                    // The pumpjack terminal we ran into does not belong to the a pumpjack that the current
                                    // pumpjack should be connected to.
                                    break;
                                }

                                trunk.Points.Add(point);

                                currentCenter = matchedCenters.First();

                                trunk.Centers.Add(currentCenter);

                                nextCenters = centerToConnectedCenters[currentCenter];
                                maxX = centerToMaxX[currentCenter];
                                maxY = centerToMaxY[currentCenter];
                            }
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

    private static Dictionary<Location, HashSet<Location>> GetConnectedPumpjacks(Context context, Dictionary<Location, HashSet<FlutePoint>> centerToPoints)
    {
        // Determine which terminals should be connected to each other either directly or via only Steiner points.
        var centerToCenters = new Dictionary<Location, HashSet<Location>>();
        foreach (var center in context.CenterToTerminals.Keys)
        {
            var otherCenters = new HashSet<Location>();
            var visitedPoints = new HashSet<FlutePoint>();
            var queue = new Queue<FlutePoint>();
            foreach (var point in centerToPoints[center])
            {
                queue.Enqueue(point);
            }

            while (queue.Count > 0)
            {
                var point = queue.Dequeue();

                if (!visitedPoints.Add(point))
                {
                    continue;
                }

                if ((point.Centers.Contains(center) && point.Centers.Count > 1)
                    || (!point.Centers.Contains(center) && point.Centers.Count > 0))
                {
                    otherCenters.UnionWith(point.Centers);
                }
                else
                {
                    foreach (var neighbor in point.Neighbors)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            otherCenters.Remove(center);
            centerToCenters.Add(center, otherCenters);
        }

        return centerToCenters;
    }

    private static void EliminateOtherTerminals(Context context, Dictionary<Location, FlutePoint> locationToPoint, TerminalLocation selectedTerminal)
    {
        var terminalOptions = context.CenterToTerminals[selectedTerminal.Center];

        if (terminalOptions.Count == 1)
        {
            return;
        }

        foreach (var terminal in terminalOptions)
        {
            if (terminal == selectedTerminal)
            {
                continue;
            }

            locationToPoint[terminal.Terminal].IsEliminated = true;
        }

        terminalOptions.Clear();
        terminalOptions.Add(selectedTerminal);
    }

    private static Dictionary<Location, FlutePoint> GetLocationToFlutePoint(Context context)
    {
        var fluteTree = GetFluteTree(context);

        // VisualizeFLUTE(context, context.CenterToTerminals.SelectMany(p => p.Value).Select(l => new System.Drawing.Point(l.Terminal.X, l.Terminal.Y)).ToList(), fluteTree);

        // Map the FLUTE tree into a more useful object graph.
        var locationToPoint = new Dictionary<Location, FlutePoint>();

        FlutePoint GetOrAddPoint(Dictionary<Location, FlutePoint> locationToPoint, Branch branch)
        {
            var location = new Location(branch.X, branch.Y);
            if (!locationToPoint.TryGetValue(location, out var point))
            {
                point = new FlutePoint(location);
                locationToPoint.Add(location, point);
            }

            return point;
        }

        // Explore the branches.
        foreach (var branch in fluteTree.Branch)
        {
            var current = branch;
            while (true)
            {
                var next = fluteTree.Branch[current.N];

                var currentPoint = GetOrAddPoint(locationToPoint, current);
                var nextPoint = GetOrAddPoint(locationToPoint, next);

                currentPoint.Neighbors.Add(nextPoint);
                nextPoint.Neighbors.Add(currentPoint);

                if (current.N == next.N)
                {
                    break;
                }

                current = next;
            }
        }

        // Add in pumpjack information
        foreach ((var center, var terminals) in context.CenterToTerminals)
        {
            foreach (var terminal in terminals)
            {
                var point = locationToPoint[terminal.Terminal];
                point.Terminals.Add(terminal);
                point.Centers.Add(center);
            }
        }

        return locationToPoint;
    }

    private static void Visualize(Context context, Dictionary<Location, FlutePoint> locationToPoint, HashSet<Location> pipes)
    {
        var grid = new PipeGrid(context.Grid);

        foreach (var pipe in pipes)
        {
            if (locationToPoint.TryGetValue(pipe, out var point) && !point.IsSteinerPoint)
            {
                if (grid.IsEmpty(pipe))
                {
                    grid.AddEntity(pipe, new Terminal());
                }
            }
            else
            {
                if (grid.IsEmpty(pipe))
                {
                    grid.AddEntity(pipe, new Pipe());
                }
            }
        }

        Visualizer.Show(
            grid,
            locationToPoint
                .Values
                .Where(p => !p.IsEliminated)
                .Select(p => (IPoint)new Point(p.Location.X, p.Location.Y)),
            Array.Empty<IEdge>());
    }

    private static Tree GetFluteTree(Context context)
    {
        var centerPoints = context
            .CenterToTerminals
            .Keys
            .Select(l => new System.Drawing.Point(l.X, l.Y))
            .ToList();

        var terminalPoints = context
            .CenterToTerminals
            .Values
            .SelectMany(ts => ts.Select(t => new System.Drawing.Point(t.Terminal.X, t.Terminal.Y)))
            .ToList();

        var pumpjackPoints = context
            .Grid
            .LocationToEntity
            .Where(p => p.Value is PumpjackSide || p.Value is PumpjackCenter)
            .Select(p => p.Key)
            .Select(l => new System.Drawing.Point(l.X, l.Y))
            .ToList();

        return FLUTE.Execute(terminalPoints);
    }

    private static void VisualizeFLUTE(Context context, List<System.Drawing.Point> terminalPoints, Tree fluteTree)
    {
        var steinerPoints = fluteTree
            .Branch
            .Select(b => new System.Drawing.Point(b.X, b.Y))
            .Except(terminalPoints)
            .ToList();

        var edges = new HashSet<IEdge>();

        for (int i = 0; i < fluteTree.Branch.Length; i++)
        {
            var current = fluteTree.Branch[i];

            while (true)
            {
                var next = fluteTree.Branch[current.N];
                var edge = new Edge(e: 0, new Point(current.X, current.Y), new Point(next.X, next.Y));
                edges.Add(edge);

                if (current.N == next.N)
                {
                    break;
                }

                current = next;
            }
        }

        Visualizer.Show(context.Grid, steinerPoints.Concat(terminalPoints).Distinct().Select(x => (IPoint)new Point(x.X, x.Y)), edges);
    }
}
