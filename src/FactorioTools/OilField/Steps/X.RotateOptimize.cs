using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class RotateOptimize
{
    internal static HashSet<Location> Execute(Context context, HashSet<Location> pipes)
    {
        var intersections = GetIntersections(context, pipes);
        var goals = GetGoals(context, intersections);

        var existingPipeGrid = new ExistingPipeGrid(context.Grid);
        AddPipeEntities.Execute(existingPipeGrid, context.CenterToTerminals, pipes);

        Visualizer.Show(existingPipeGrid, intersections.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        foreach ((var center, var terminals) in context.CenterToTerminals)
        {
            var currentTerminal = terminals.Single();

            if (context.LocationToTerminals[currentTerminal.Terminal].Count > 1)
            {
                continue;
            }

            if (intersections.Contains(currentTerminal.Terminal))
            {
                continue;
            }

            goals.Remove(currentTerminal.Terminal);
            var exploredPaths = ExplorePaths(existingPipeGrid, currentTerminal.Terminal, goals);

            var goal = exploredPaths.ReachedGoals.Single();
            var newPipes = new HashSet<Location>(pipes);
            exploredPaths.RemovePathTo(goal, newPipes);

            var paths = new List<(TerminalLocation Terminal, int PathLength, List<Location>? Path)>
            {
                (currentTerminal, PathLength: pipes.Count - newPipes.Count + 1, Path: null)
            };

            foreach ((var direction, var translation) in InitializeContext.TerminalOffsets)
            {
                var terminalCandidate = center.Translate(translation);

                if (terminalCandidate == currentTerminal.Terminal
                    || context.Grid.IsEntityType<PumpjackSide>(terminalCandidate))
                {
                    continue;
                }

                var result = AStar.GetShortestPath(context.Grid, terminalCandidate, newPipes);
                if (result.ReachedGoal.HasValue)
                {
                    var path = result.Path;
                    paths.Add((new TerminalLocation(center, terminalCandidate, direction), path.Count, path));
                }
            }

            paths = paths.OrderBy(p => p.PathLength).ToList();

            if (paths[0].Path is not null)
            {
                var newPath = paths[0].Path!;
                var newTerminal = paths[0].Terminal;

                newPipes.UnionWith(newPath);

                if (newTerminal != currentTerminal)
                {
                    context.CenterToTerminals[center].Add(newTerminal);

                    if (!context.LocationToTerminals.TryGetValue(newTerminal.Terminal, out var locationTerminals))
                    {
                        locationTerminals = new List<TerminalLocation> { newTerminal };
                        context.LocationToTerminals.Add(newTerminal.Terminal, locationTerminals);
                    }
                    else
                    {
                        locationTerminals.Add(newTerminal);
                    }

                    EliminateOtherTerminals(context, newTerminal);
                }

                pipes = newPipes;

                intersections = GetIntersections(context, pipes);
                goals = GetGoals(context, intersections);

                /*
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone, context.CenterToTerminals, pipes);
                Visualizer.Show(clone, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
                */
            }
            else
            {
                // This goal was removed above so we add it back for subsequent iterations.
                goals.Add(currentTerminal.Terminal);
            }
        }

        return pipes;
    }

    private static HashSet<Location> GetGoals(Context context, HashSet<Location> intersections)
    {
        var goals = new HashSet<Location>(intersections);
        goals.UnionWith(context.LocationToTerminals.Keys);
        return goals;
    }

    private static ExploredPaths ExplorePaths(SquareGrid grid, Location start, HashSet<Location> goals)
    {
        var cameFrom = new Dictionary<Location, Location>();
        cameFrom[start] = start;

        var toExplore = new Stack<Location>();
        toExplore.Push(start);

        Span<Location> neighbors = stackalloc Location[4];

        var reachedGoals = new List<Location>();

        while (toExplore.Count > 0)
        {
            var current = toExplore.Pop();

            if (current != start && goals.Contains(current))
            {
                reachedGoals.Add(current);
                continue;
            }

            grid.GetNeighbors(neighbors, current);
            for (var i = 0; i < neighbors.Length; i++)
            {
                if (!neighbors[i].IsValid || cameFrom.ContainsKey(neighbors[i]))
                {
                    continue;
                }

                cameFrom.Add(neighbors[i], current);
                toExplore.Push(neighbors[i]);
            }
        }

        return new ExploredPaths(cameFrom, reachedGoals);
    }

    private static HashSet<Location> GetIntersections(Context context, HashSet<Location> pipes)
    {
        var intersections = new HashSet<Location>();
        foreach (var pipe in pipes)
        {
            var neighbors = 0;

            if (pipes.Contains(pipe.Translate((1, 0))))
            {
                neighbors++;
            }

            if (pipes.Contains(pipe.Translate((0, -1))))
            {
                neighbors++;
            }

            if (pipes.Contains(pipe.Translate((-1, 0))))
            {
                neighbors++;
            }

            if (pipes.Contains(pipe.Translate((0, 1))))
            {
                neighbors++;
            }

            if (neighbors > 2 || context.LocationToTerminals.ContainsKey(pipe) && neighbors > 1)
            {
                intersections.Add(pipe);
            }
        }

        return intersections;
    }

    private class ExploredPaths
    {
        public ExploredPaths(Dictionary<Location, Location> cameFrom, List<Location> reachedGoals)
        {
            CameFrom = cameFrom;
            ReachedGoals = reachedGoals;
        }

        public Dictionary<Location, Location> CameFrom { get; }
        public List<Location> ReachedGoals { get; }

        public void RemovePathTo(Location goal, HashSet<Location> removeFrom)
        {
            var current = goal;

            while (true)
            {
                if (!CameFrom.TryGetValue(current, out var previous))
                {
                    throw new InvalidOperationException("No path to this goal was found.");
                }

                if (current == previous)
                {
                    break;
                }

                removeFrom.Remove(previous);
                current = previous;
            }
        }
    }
}