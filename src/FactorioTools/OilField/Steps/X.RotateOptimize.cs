using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class RotateOptimize
{
    private static readonly IReadOnlyDictionary<Direction, IReadOnlyList<(Direction Direction, (int DeltaX, int DeltaY))>> DirectionToTerminalOffsets = InitializeContext.TerminalOffsets
        .ToDictionary(x => x.Direction, x => (IReadOnlyList<(Direction Direction, (int DeltaX, int DeltaY))>)new[] { x });

    internal static HashSet<Location> Execute(Context parentContext, HashSet<Location> pipes)
    {
        var context = new ChildContext(parentContext, pipes);        

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

            context.Goals.Remove(currentTerminal.Terminal);
            var exploredPaths = ExplorePaths(context, currentTerminal.Terminal);

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

            if (context.Intersections.Contains(currentTerminal.Terminal))
            {
                /*
                if (exploredPaths.ReachedGoals.Count < 1)
                {
                    throw new NotImplementedException();
                    var clone = new PipeGrid(context.ExistingPipeGrid);
                    AddPipeEntities.Execute(clone, context.CenterToTerminals, context.Pipes);
                    Visualizer.Show(clone, context.Goals.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
                }
                */

                foreach (var goal in exploredPaths.ReachedGoals)
                {
                    var existingPath = exploredPaths.GetPath(goal);
                    context.Pipes.ExceptWith(existingPath);
                    context.Pipes.Add(goal);

                    var disconnectedPipes = ExplorePipes(context, goal);
                    UseBestTerminal(context, currentTerminal, exploredPaths, goal, DirectionToTerminalOffsets[currentTerminal.Direction], disconnectedPipes);
                }
            }
            else
            {
                /*
                if (exploredPaths.ReachedGoals.Count == 0)
                {
                    throw new NotImplementedException();
                    var clone = new PipeGrid(context.ExistingPipeGrid);
                    AddPipeEntities.Execute(clone, context.CenterToTerminals, context.Pipes);
                    Visualizer.Show(clone, context.Goals.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
                }
                */

                var goal = exploredPaths.ReachedGoals.Single();
                UseBestTerminal(context, currentTerminal, exploredPaths, goal, InitializeContext.TerminalOffsets, context.Pipes);
            }
        }

        return context.Pipes;
    }

    private static void UseBestTerminal(
        ChildContext context,
        TerminalLocation originalTerminal,
        ExploredPaths exploredPaths,
        Location originalGoal,
        IReadOnlyList<(Direction Direction, (int DeltaX, int DeltaY))> terminalOffsets,
        HashSet<Location> connectionPoints)
    {
        var originalPath = exploredPaths.GetPath(originalGoal);
        context.Pipes.ExceptWith(originalPath);
        context.Pipes.Add(originalGoal);

        var paths = new List<(TerminalLocation Terminal, List<Location> Path)>
        {
            (originalTerminal, originalPath)
        };

        for (var i = 0; i < terminalOffsets.Count; i++)
        {
            (var direction, var translation) = terminalOffsets[i];

            var terminalCandidate = originalTerminal.Center.Translate(translation);
            if (context.Grid.IsEntityType<PumpjackSide>(terminalCandidate))
            {
                continue;
            }

            var result = AStar.GetShortestPath(context.Grid, terminalCandidate, connectionPoints);

            if (result.ReachedGoal.HasValue)
            {
                var path = result.Path;
                paths.Add((new TerminalLocation(originalTerminal.Center, terminalCandidate, direction), path));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        paths.Sort((a, b) =>
        {
            var pathCountCompare = a.Path.Count.CompareTo(b.Path.Count);
            if (pathCountCompare != 0)
            {
                return pathCountCompare;
            }

            return CountTurns(a.Path).CompareTo(CountTurns(b.Path));
        });

        context.Pipes.UnionWith(paths[0].Path);

        if (paths[0].Path != originalPath)
        {
            var newPath = paths[0].Path!;
            var newTerminal = paths[0].Terminal;

            if (newTerminal != originalTerminal)
            {
                context.CenterToTerminals[originalTerminal.Center].Add(newTerminal);

                if (!context.LocationToTerminals.TryGetValue(newTerminal.Terminal, out var locationTerminals))
                {
                    locationTerminals = new List<TerminalLocation> { newTerminal };
                    context.LocationToTerminals.Add(newTerminal.Terminal, locationTerminals);
                }
                else
                {
                    locationTerminals.Add(newTerminal);
                }

                EliminateOtherTerminals(context.ParentContext, newTerminal);
            }

            context.UpdateIntersectionsAndGoals();

            /*
            if (paths[0].Terminal.Direction == originalTerminal.Direction)
            {
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone, context.CenterToTerminals, context.Pipes);
                Visualizer.Show(clone, originalPath.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
            }
            */

            /*
            var clone = new PipeGrid(context.Grid);
            AddPipeEntities.Execute(clone, context.CenterToTerminals, context.Pipes);
            Visualizer.Show(clone, originalPath.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
            */
        }
        else
        {
            context.Goals.Add(originalTerminal.Terminal);
        }
    }

    private static HashSet<Location> ExplorePipes(ChildContext context, Location start)
    {
        var toExplore = new Queue<Location>();
        toExplore.Enqueue(start);
        var pipes = new HashSet<Location> { start };

        Span<Location> neighbors = stackalloc Location[4];

        while (toExplore.Count > 0)
        {
            var current = toExplore.Dequeue();

            context.ExistingPipeGrid.GetNeighbors(neighbors, current);
            for (var i = 0; i < neighbors.Length; i++)
            {
                if (neighbors[i].IsValid && pipes.Add(neighbors[i]))
                {
                    toExplore.Enqueue(neighbors[i]);
                }
            }
        }

        return pipes;
    }

    private static ExploredPaths ExplorePaths(ChildContext context, Location start)
    {
        var cameFrom = new Dictionary<Location, Location>();
        cameFrom[start] = start;

        var toExplore = new Queue<Location>();
        toExplore.Enqueue(start);

        Span<Location> neighbors = stackalloc Location[4];

        var reachedGoals = new List<Location>();

        while (toExplore.Count > 0)
        {
            var current = toExplore.Dequeue();

            if (current != start && context.Goals.Contains(current))
            {
                reachedGoals.Add(current);
                continue;
            }

            context.ExistingPipeGrid.GetNeighbors(neighbors, current);
            for (var i = 0; i < neighbors.Length; i++)
            {
                if (!neighbors[i].IsValid || cameFrom.ContainsKey(neighbors[i]))
                {
                    continue;
                }

                cameFrom.Add(neighbors[i], current);
                toExplore.Enqueue(neighbors[i]);
            }
        }

        return new ExploredPaths(start, cameFrom, reachedGoals);
    }

    private class ChildContext
    {
        public ChildContext(Context parentContext, HashSet<Location> pipes)
        {
            ParentContext = parentContext;
            Pipes = pipes;
            Intersections = new HashSet<Location>(pipes.Count);
            Goals = new HashSet<Location>(pipes.Count);
            ExistingPipeGrid = new ExistingPipeGrid(parentContext.Grid, pipes);

            UpdateIntersectionsAndGoals();
        }

        public Context ParentContext { get; }
        public SquareGrid Grid => ParentContext.Grid;
        public Dictionary<Location, List<TerminalLocation>> LocationToTerminals => ParentContext.LocationToTerminals;
        public IReadOnlyDictionary<Location, List<TerminalLocation>> CenterToTerminals => ParentContext.CenterToTerminals;
        public HashSet<Location> Pipes { get; }
        public HashSet<Location> Intersections { get; }
        public HashSet<Location> Goals { get; }
        public ExistingPipeGrid ExistingPipeGrid { get; }

        public void UpdateIntersectionsAndGoals()
        {
            Intersections.Clear();

            Goals.Clear();
            Goals.UnionWith(LocationToTerminals.Keys);

            foreach (var pipe in Pipes)
            {
                var neighbors = 0;

                if (Pipes.Contains(pipe.Translate((1, 0))))
                {
                    neighbors++;
                }

                if (Pipes.Contains(pipe.Translate((0, -1))))
                {
                    neighbors++;
                }

                if (Pipes.Contains(pipe.Translate((-1, 0))))
                {
                    neighbors++;
                }

                if (Pipes.Contains(pipe.Translate((0, 1))))
                {
                    neighbors++;
                }

                if (neighbors > 2 || LocationToTerminals.ContainsKey(pipe) && neighbors > 1)
                {
                    Intersections.Add(pipe);
                    Goals.Add(pipe);
                }
            }
        }
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