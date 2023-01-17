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

        public bool IsEliminated { get; set; }
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
        var locationToPoint = GetLocationToFlutePoint(context);

        var start = locationToPoint
            .Values
            .Where(p => p.Terminals.Count > 0)
            .OrderBy(p => p.Location.X)
            .ThenBy(p => p.Location.Y)
            .First();
        var centerInGroup = new HashSet<Location> { start.Centers.First() };
        var pointsInGroup = new HashSet<FlutePoint> { start };
        var pipes = new HashSet<Location>();

        // Calculate how many FLUTE points are on each row and column. This allows us to prefer points that can be on a
        // long line of pipes.
        var emptyPoints = locationToPoint.Values.Where(p => context.Grid.IsEmpty(p.Location));
        var groupedByX = emptyPoints.ToLookup(p => p.Location.X);
        var groupedByY = emptyPoints.ToLookup(p => p.Location.Y);

        while (centerInGroup.Count < context.CenterToTerminals.Count)
        {
            // Perform a breadth-first search to find the next FLUTE tree point that is not yet connected.
            var queue = new Queue<FlutePoint>();
            queue.Enqueue(start);

            var pointToParent = new Dictionary<FlutePoint, FlutePoint> { { start, start } };
            FlutePoint? goal = null;

            while (goal is null && queue.Count > 0)
            {
                var point = queue.Dequeue();

                // Prefer available, distance Steiner points, then terminals with multiple pumpjacks, then terminals on "crowded" rows or columns.
                var neighborToDistance = point.Neighbors.ToDictionary(p => p, p => p.Location.GetManhattanDistance(point.Location));
                var neighbors = point
                    .Neighbors
                    .OrderByDescending(p => p.Location.X == point.Location.X || p.Location.Y == point.Location.Y ? p.Location.GetManhattanDistance(point.Location) : 0)
                    .ThenByDescending(p => p.Centers.Count == 0 && context.Grid.IsEmpty(p.Location) ? p.Location.GetManhattanDistance(point.Location) : 0)
                    .ThenByDescending(p => p.Centers.Count)
                    .ThenByDescending(p => groupedByX[p.Location.X].Count(c => !c.IsEliminated) + groupedByY[p.Location.Y].Count(c => !c.IsEliminated))
                    .ThenBy(p => p.Location.X)
                    .ThenBy(p => p.Location.Y);

                foreach (var neighbor in neighbors)
                {
                    if (pointToParent.ContainsKey(neighbor))
                    {
                        continue;
                    }

                    pointToParent.Add(neighbor, point);

                    if (pointsInGroup.Contains(neighbor))
                    {
                        // This neighbor is alreay in the group. Continue exploring.
                    }
                    else if (neighbor.Centers.Count == 0)
                    {
                        // This neighbor is a Steiner point.
                        if (context.Grid.IsEmpty(neighbor.Location))
                        {
                            // This neighbor is an EMPTY Steiner point. Use it as the goal for path finding.
                            goal = neighbor;
                            break;
                        }
                        else
                        {
                            // This is an NON-EMPTY Steiner point (e.g. it lies within a pumpjack and can't be a spot for
                            // pipes). Continue exploring.
                        }
                    }
                    else
                    {
                        // This neighbor is a pumpjack terminal.
                        if (neighbor.IsEliminated)
                        {
                            // This is a terminal that has been eliminated (another terminal for the same pumpjack was
                            // selected instead). Continue exploring.
                        }
                        else
                        {
                            var newCenters = new HashSet<Location>(neighbor.Centers);
                            newCenters.ExceptWith(centerInGroup);

                            if (newCenters.Count > 0)
                            {
                                // This is a terminal for a pumpjack we have NOT REACHED YET. Use it as the goal for path finding.
                                goal = neighbor;
                                break;
                            }
                            else
                            {
                                // This is a terminal for a pumpjack we have ALREADY REACHED. Continue exploring.
                            }
                        }
                    }

                    queue.Enqueue(neighbor);
                }
            }

            if (goal is null)
            {
                throw new InvalidOperationException("No connection could be found between the current terminals and the remaining terminals.");
            }

            FlutePoint parent = goal;
            List<Location> path;
            while (true)
            {
                parent = pointToParent[parent];
                if (!context.Grid.IsEmpty(parent.Location))
                {
                    continue;
                }

                if (parent.Centers.Count > 0)
                {
                    // This parent is a terminal. Use all terminal candidates as the start options.
                    var startCenter = parent
                        .Centers
                        .Intersect(centerInGroup)
                        .First();
                    var shortestPath = context
                        .CenterToTerminals[startCenter]
                        .Select(t =>
                        {
                            var result = Dijkstras.GetShortestPaths(context.Grid, t.Terminal, new HashSet<Location> { goal.Location }, stopOnFirstGoal: true);
                            var path = result.GetStraightPaths(goal.Location).First();
                            return new { Terminal = t, Path = path };
                        })
                        .OrderBy(t => t.Path.Count)
                        .First();

                    // We've selected a path from a terminal to the goal. Eliminate other terminal options for this pumpjack.
                    centerInGroup.Add(shortestPath.Terminal.Center);
                    EliminateOtherTerminals(context, locationToPoint, shortestPath.Terminal);

                    path = shortestPath.Path;
                }
                else
                {
                    // The parent is a Steiner point.
                    var result = Dijkstras.GetShortestPaths(context.Grid, parent.Location, new HashSet<Location> { goal.Location }, stopOnFirstGoal: true);
                    path = result.GetStraightPaths(goal.Location).First();
                }

                // If the goal is a terminal, eliminate the other terminal options from the pumpjack.
                foreach (var goalTerminal in goal.Terminals)
                {
                    centerInGroup.Add(goalTerminal.Center);
                    EliminateOtherTerminals(context, locationToPoint, goalTerminal);
                }

                foreach (var pipe in path)
                {
                    pipes.Add(pipe);
                }

                // Visualize(context, locationToPoint, pipes);


                // Start the next iteration at the goal we found
                pointsInGroup.Add(goal);
                start = goal;
                break;
            }


            /*
            var startCenter = edge
                .Value
                .Start
                .Centers
                .Intersect(centerInGroup)
                .First();
            var goal = edge.Value.Goal.Location;
            var shortestPath = context
                .CenterToTerminals[startCenter]
                .Select(t =>
                {
                    var result = Dijkstras.GetShortestPaths(context.Grid, t.Terminal, new HashSet<Location> { goal }, stopOnFirstGoal: true);
                    var path = result.GetStraightPaths(goal).First();
                    return new { Terminal = t, Path = path };
                })
                .OrderBy(t => t.Path.Count)
                .First();
            */
            // 
        }

        return pipes;

        throw new NotImplementedException();
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
