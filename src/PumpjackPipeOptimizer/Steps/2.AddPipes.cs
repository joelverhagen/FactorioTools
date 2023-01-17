using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using DelaunatorSharp;
using Knapcode.FluteSharp;
using PumpjackPipeOptimizer.Algorithms;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

/// <summary>
/// This is a clone of Teoxoy's implementation for Factorio Blueprint Editor.
/// https://github.com/teoxoy/factorio-blueprint-editor/blob/master/packages/editor/src/core/generators/pipe.ts
/// </summary>
internal static class AddPipes
{
    private static readonly Lazy<FLUTE> LazyFlute = new Lazy<FLUTE>(() =>
    {
        var d = 9;
        var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        using var powvStream = File.OpenRead(Path.Combine(assemblyDir, $"POWV{d}.dat"));
        using var postStream = File.OpenRead(Path.Combine(assemblyDir, $"POST{d}.dat"));
        var lookUpTable = new LookUpTable(d, powvStream, postStream);
        return new FLUTE(lookUpTable);
    });

    private static FLUTE FLUTE => LazyFlute.Value;

    private class FlutePoint
    {
        public FlutePoint(Location location)
        {
            Location = location;
        }

        public Location Location { get; }
        public HashSet<Location> Centers { get; } = new HashSet<Location>();
        public HashSet<TerminalLocation> Terminals { get; } = new HashSet<TerminalLocation>();
        public HashSet<FlutePoint> Neighbors { get; } = new HashSet<FlutePoint>();

        public override string ToString()
        {
            return Location.ToString();
        }
    }

    private static IReadOnlyList<(int DeltaX, int DeltaY)> NeighborTranslations = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) };

    public static HashSet<Location> Execute(Context context)
    {
        var locationToTerminals = context
            .CenterToTerminals
            .Values
            .SelectMany(ts => ts)
            .GroupBy(ts => ts.Terminal)
            .ToDictionary(g => g.Key, g => g.ToHashSet());

        var fluteTree = GetFluteTree(context);

        VisualizeFLUTE(context, context.CenterToTerminals.SelectMany(p => p.Value).Select(l => new System.Drawing.Point(l.Terminal.X, l.Terminal.Y)).ToList(), fluteTree);

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

        // Start at Steiner point closest to the middle that is in an empty spot.
        var emptyPoints = locationToPoint
            .Values
            .Where(p => context.Grid.IsEmpty(p.Location))
            .ToList();
        var steinerPoints = emptyPoints
            .Where(p => p.Terminals.Count == 0)
            .OrderBy(p => p.Location.GetManhattanDistance(context.Grid.Middle))
            .ThenBy(p => p.Location.X) // PERF-IDEA: remove this sort. It's just for determinism.
            .ThenBy(p => p.Location.Y) // PERF-IDEA: remove this sort. It's just for determinism.
            .ToList();
        var pipes = new HashSet<Location>();
        var pipeToPipeGroup = new Dictionary<Location, HashSet<Location>>();
        var pipeGroups = new HashSet<HashSet<Location>>();

        void MergePipeGroup(HashSet<Location> pipeGroup, HashSet<Location> otherPipeGroup)
        {
            if (!ReferenceEquals(pipeGroup, otherPipeGroup))
            {
                // This is an existing pipe and a different group. Merge the two groups.
                foreach (var otherPipe in otherPipeGroup)
                {
                    pipeToPipeGroup![otherPipe] = pipeGroup;
                }

                pipeGroup.UnionWith(otherPipeGroup);
                pipeGroups!.Remove(otherPipeGroup);
            }
        }

        void AddPipe(Location pipe, HashSet<Location> pipeGroup)
        {
            pipes!.Add(pipe);

            if (!pipeToPipeGroup!.TryGetValue(pipe, out var otherPipeGroup))
            {
                // This is a new pipe. Add it to the current group.
                pipeGroup.Add(pipe);
                pipeToPipeGroup.Add(pipe, pipeGroup);
            }
            else
            {
                MergePipeGroup(pipeGroup, otherPipeGroup);
            }

            // Join adjacent groups.
            foreach (var translation in NeighborTranslations)
            {
                var neighbor = pipe.Translate(translation);
                if (pipeToPipeGroup.TryGetValue(neighbor, out otherPipeGroup))
                {
                    MergePipeGroup(pipeGroup, otherPipeGroup);
                }
            }
        }

        // Connect all Steiner points to their closest point.
        var steinerPointGoals = emptyPoints.Select(p => p.Location).ToHashSet();
        foreach (var point in steinerPoints)
        {
            if (pipes.Contains(point.Location))
            {
                continue;
            }

            pipes.Add(point.Location);

            if (!pipeToPipeGroup.TryGetValue(point.Location, out var pipeGroup))
            {
                pipeGroup = new HashSet<Location> { point.Location };
                pipeToPipeGroup.Add(point.Location, pipeGroup);
                pipeGroups.Add(pipeGroup);
            }

            steinerPointGoals.Remove(point.Location);

            var result = Dijkstras.GetShortestPaths(context.Grid, point.Location, steinerPointGoals, stopOnFirstGoal: true);
            var reachedGoal = result.ReachedGoals.Single();

            var isTerminal = context.CenterToTerminals.Any(p => p.Value.Any(t => t.Terminal == reachedGoal));

            if (locationToPoint.TryGetValue(reachedGoal, out var reachedPoint) && reachedPoint.Terminals.Count > 0)
            {
                // The reached point is a terminal for one or more pumpjacks. This means we must eliminate the other
                // terminals on the related pumpjacks.
                foreach (var matchedTerminal in reachedPoint.Terminals)
                {
                    var otherTerminals = context.CenterToTerminals[matchedTerminal.Center];
                    foreach (var otherTerminal in otherTerminals)
                    {
                        if (otherTerminal == matchedTerminal)
                        {
                            continue;
                        }

                        steinerPointGoals.Remove(otherTerminal.Terminal);
                    }

                    otherTerminals.Clear();
                    otherTerminals.Add(matchedTerminal);
                }
            }

            // IDEA: try the other path options
            var path = result.GetStraightPaths(reachedGoal).First();

            foreach (var pipe in path)
            {
                AddPipe(pipe, pipeGroup);
                steinerPointGoals.Add(pipe);
            }
            
            Visualize(context, locationToPoint, pipes);
        };

        Visualize(context, locationToPoint, pipes);

        throw new NotImplementedException();

        // Connect unconnected pumpjacks to the closest pipe
        var unconnectedCenters = context
            .CenterToTerminals
            .Where(p => p.Value.Count > 1)
            .Select(p => p.Key)
            .OrderBy(c => c.GetManhattanDistance(context.Grid.Middle))
            .ThenBy(c => c.X) // PERF-IDEA: remove this sort. It's just for determinism.
            .ThenBy(c => c.Y) // PERF-IDEA: remove this sort. It's just for determinism.
            .ToList();

        // IDEA: re-order the unconnected centers as we going based the overall shortest path to the existing pipes.
        foreach (var center in unconnectedCenters)
        {
            var terminals = context.CenterToTerminals[center];

            var closestTerminal = terminals
                .Select(t =>
                {
                    var result = Dijkstras.GetShortestPaths(context.Grid, t.Terminal, pipes, stopOnFirstGoal: true);
                    var reachedGoal = result.ReachedGoals.Single();

                    // IDEA: try the other path options
                    var path = result.GetStraightPaths(reachedGoal).First();
                    
                    return new { Terminal = t, ReachedGoal = reachedGoal, Path = path };
                })
                .OrderBy(t => t.Path.Count)
                .First();

            terminals.Clear();
            terminals.Add(closestTerminal.Terminal);

            var pipeGroup = pipeToPipeGroup[closestTerminal.ReachedGoal];
            foreach (var pipe in closestTerminal.Path)
            {
                AddPipe(pipe, pipeGroup);
                pipes.Add(pipe);
            }
        }

        // Connect pipe groups using the FLUTE tree neighbors.
        var finalPipeGroup = pipeGroups
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.Min(l => l.X)) // PERF-IDEA: remove this sort. It's just for determinism.
            .ThenBy(g => g.Min(l => l.Y)) // PERF-IDEA: remove this sort. It's just for determinism.
            .First();
        pipeGroups.Remove(finalPipeGroup);

        while (pipeGroups.Count > 0)
        {
            // Find a point the has a neighbor outside of the current group. Perform a breadth-first search starting from
            // an arbitrary location.
            var start = finalPipeGroup
                .Select(l => locationToPoint.TryGetValue(l, out var p) ? p : null!)
                .Where(p => p is not null)
                .OrderBy(p => p.Location.X) // PERF-IDEA: remove this sort. It's just for determinism.
                .ThenBy(p => p.Location.Y) // PERF-IDEA: remove this sort. It's just for determinism.
                .First();
            var queue = new Queue<FlutePoint>();
            queue.Enqueue(start);

            var pointToInGroup = new Dictionary<FlutePoint, FlutePoint> { { start, start } };
            (FlutePoint Start, FlutePoint Goal)? edge = null;

            while (edge is null && queue.Count > 0)
            {
                var point = queue.Dequeue();

                foreach (var neighbor in point.Neighbors)
                {
                    // Only enqueue the neighbor if we haven't explored it already.
                    if (pointToInGroup.ContainsKey(neighbor))
                    {
                        continue;
                    }

                    if (!pipes.Contains(neighbor.Location))
                    {
                        // This neighbor is either an eliminated terminal or a Steiner point that is at a non-empty spot
                        // on the grid (e.g. inside of a pumpjack). Use the last in-group point as the parent.
                        pointToInGroup[neighbor] = pointToInGroup[point];
                    }
                    else if (finalPipeGroup.Contains(neighbor.Location))
                    {
                        pointToInGroup[neighbor] = neighbor;
                    }
                    else
                    {
                        edge = (pointToInGroup[point], neighbor);
                        break;
                    }
;
                    queue.Enqueue(neighbor);
                }
            }

            if (!edge.HasValue)
            {
                throw new InvalidOperationException("No edge could be found between the remaining groups.");
            }

            var goals = new HashSet<Location>();
            foreach (var group in pipeGroups)
            {
                goals.UnionWith(group);
            }

            var result = Dijkstras.GetShortestPaths(context.Grid, edge.Value.Start.Location, goals, stopOnFirstGoal: true);
            var reachedGoal = result.ReachedGoals.Single();

            var otherPipeGroup = pipeToPipeGroup[reachedGoal];
            MergePipeGroup(finalPipeGroup, otherPipeGroup);

            // IDEA: try the other path options
            var path = result.GetStraightPaths(reachedGoal).First();

            foreach (var pipe in path)
            {
                AddPipe(pipe, finalPipeGroup);
            }
        }

        foreach (var pipe in pipes)
        {
            if (locationToPoint.TryGetValue(pipe, out var point) && point.Terminals.Count > 0)
            {
                context.Grid.AddEntity(pipe, new Terminal());
            }
            else
            {
                context.Grid.AddEntity(pipe, new Pipe());
            }
        }

        Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());

        throw new NotImplementedException();

        return pipes;
    }

    private static void Visualize(Context context, Dictionary<Location, FlutePoint> locationToPoint, HashSet<Location> pipes)
    {
        var grid = new PipeGrid(context.Grid);

        foreach (var pipe in pipes)
        {
            if (locationToPoint.TryGetValue(pipe, out var point) && point.Terminals.Count > 0)
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

        Visualizer.Show(grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());
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
