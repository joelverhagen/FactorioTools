using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class RotateOptimize
{
    internal static HashSet<Location> Execute(Context context, HashSet<Location> pipes)
    {
        var intersections = GetIntersections(context.LocationToTerminals, pipes);
        var goals = GetGoals(context.LocationToTerminals, intersections);

        var existingPipeGrid = new ExistingPipeGrid(context.Grid, pipes);

        // Visualizer.Show(existingPipeGrid, intersections.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        foreach ((var center, var terminals) in context.CenterToTerminals)
        {
            var currentTerminal = terminals.Single();

            // Locations that are used by two pumpjack terminals are not eligible for this optimization flow. A better
            // pipe configuration can only be detected if a new path is shorter to other pipes. When a terminal is
            // shared with another pumpjack, the current path length (1, start = goal) will always be less than or equal
            // to the alternatives.
            if (context.LocationToTerminals[currentTerminal.Terminal].Count > 1)
            {
                continue;
            }

            /*
            if (goals.Contains(new Location(39, 25)))
            {
            }
            */

            goals.Remove(currentTerminal.Terminal);
            var exploredPaths = ExplorePaths(existingPipeGrid, currentTerminal.Terminal, goals);

            /*
            if (exploredPaths.ReachedGoals.Count == 0)
            {
                Visualizer.Show(existingPipeGrid, goals.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
            }
            */

            /*
            if (currentTerminal.Terminal == new Location(16, 30))
            {
                Visualizer.Show(existingPipeGrid, goals.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
            }
            */

            if (intersections.Contains(currentTerminal.Terminal))
            {
                goals.Add(currentTerminal.Terminal);

                /*
                foreach (var goal in exploredPaths.ReachedGoals)
                {

                }
                */
            }
            else
            {
                var goal = exploredPaths.ReachedGoals.Single();
                var existingPath = exploredPaths.GetPath(goal);
                pipes.ExceptWith(existingPath);
                pipes.Add(goal);

                var paths = new List<(TerminalLocation Terminal, List<Location> Path)>
                {
                    (currentTerminal, existingPath)
                };

                for (var i = 0; i < InitializeContext.TerminalOffsets.Count; i++)
                {
                    (var direction, var translation) = InitializeContext.TerminalOffsets[i];
                    if (currentTerminal.Direction == direction)
                    {
                        continue;
                    }

                    var terminalCandidate = center.Translate(translation);
                    if (context.Grid.IsEntityType<PumpjackSide>(terminalCandidate))
                    {
                        continue;
                    }

                    var result = AStar.GetShortestPath(context.Grid, terminalCandidate, pipes);
                    if (result.ReachedGoal.HasValue)
                    {
                        var path = result.Path;
                        paths.Add((new TerminalLocation(center, terminalCandidate, direction), path));
                    }
                }

                paths = paths
                    .OrderBy(p => p.Path.Count)
                    .ThenBy(p => CountTurns(p.Path))
                    .ToList();

                pipes.UnionWith(paths[0].Path);

                if (paths[0].Path != existingPath)
                {
                    var newPath = paths[0].Path!;
                    var newTerminal = paths[0].Terminal;

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

                    intersections = GetIntersections(context.LocationToTerminals, pipes);
                    goals = GetGoals(context.LocationToTerminals, intersections);

                    /*
                    var clone = new PipeGrid(context.Grid);
                    AddPipeEntities.Execute(clone, context.CenterToTerminals, pipes);
                    Visualizer.Show(clone, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
                    */
                }
                else
                {
                    goals.Add(currentTerminal.Terminal);
                }
            }
        }

        return pipes;
    }

    private static HashSet<Location> GetGoals(Dictionary<Location, List<TerminalLocation>> locationToTerminals, HashSet<Location> intersections)
    {
        var goals = new HashSet<Location>(intersections);
        goals.UnionWith(locationToTerminals.Keys);
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

        return new ExploredPaths(start, cameFrom, reachedGoals);
    }

    private static HashSet<Location> GetIntersections(Dictionary<Location, List<TerminalLocation>> locationToTerminals, HashSet<Location> pipes)
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

            if (neighbors > 2 || locationToTerminals.ContainsKey(pipe) && neighbors > 1)
            {
                intersections.Add(pipe);
            }
        }

        return intersections;
    }

    private class ExploredPaths
    {
        public ExploredPaths(Location start, Dictionary<Location, Location> cameFrom, List<Location> reachedGoals)
        {
            Start = start;
            CameFrom = cameFrom;
            ReachedGoals = reachedGoals;
        }

        public Location Start { get; }
        public Dictionary<Location, Location> CameFrom { get; }
        public List<Location> ReachedGoals { get; }

        public List<Location> GetPath(Location goal)
        {
            return Helpers.GetPath(CameFrom, Start, goal);
        }
    }
}