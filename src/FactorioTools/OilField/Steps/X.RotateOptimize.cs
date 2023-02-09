using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using Microsoft.Extensions.ObjectPool;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class RotateOptimize
{
#if USE_OBJECT_POOLING
    public static readonly ObjectPool<Queue<Location>> QueuePool = ObjectPool.Create<Queue<Location>>();
#endif
#if DEBUG && USE_OBJECT_POOLING
    public static int QueuePoolCount;
#endif

    internal static HashSet<Location> Execute(Context parentContext, HashSet<Location> pipes)
    {
        var context = new ChildContext(parentContext, pipes);

        // Visualizer.Show(existingPipeGrid, intersections.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        var modified = true;

        while (modified)
        {
            var changedTerminal = false;
            foreach (var terminals in context.CenterToTerminals.Values)
            {
                var currentTerminal = terminals.Single();

                if (context.LocationToTerminals[currentTerminal.Terminal].Count > 1
                    || context.Intersections.Contains(currentTerminal.Terminal))
                {
                    continue;
                }

                changedTerminal |= UseBestTerminal(context, currentTerminal);
            }

            var shortenedPath = false;
            foreach (var intersection in context.Intersections.ToList())
            {
                if (!context.Intersections.Contains(intersection))
                {
                    continue;
                }

                context.Goals.Remove(intersection);
                var exploredPaths = ExplorePaths(context, intersection);
                context.Goals.Add(intersection);

                foreach (var goal in exploredPaths.ReachedGoals)
                {
                    shortenedPath |= UseShortestPath(context, exploredPaths, intersection, goal);
                }
            }

            modified = changedTerminal || shortenedPath;
        }

        return context.Pipes;
    }

    private static bool UseBestTerminal(ChildContext context, TerminalLocation originalTerminal)
    {
        context.Goals.Remove(originalTerminal.Terminal);
        var exploredPaths = ExplorePaths(context, originalTerminal.Terminal);

        /*
        if (exploredPaths.ReachedGoals.Count == 0)
        {
            var clone = new PipeGrid(context.ExistingPipeGrid);
            AddPipeEntities.Execute(clone, context.CenterToTerminals, context.Pipes);
            Visualizer.Show(clone, context.Goals.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
        }
        */

        var originalGoal = exploredPaths.ReachedGoals.Single();

        var originalPath = exploredPaths.GetPath(originalGoal);
        for (var i = 1; i < originalPath.Count; i++)
        {
            context.Pipes.Remove(originalPath[i]);
        }

        var paths = new List<(TerminalLocation Terminal, List<Location> Path)>
        {
            (originalTerminal, originalPath)
        };

        for (var i = 0; i < InitializeContext.TerminalOffsets.Count; i++)
        {
            (var direction, var translation) = InitializeContext.TerminalOffsets[i];

            var terminalCandidate = originalTerminal.Center.Translate(translation);
            if (context.Grid.IsEntityType<PumpjackSide>(terminalCandidate))
            {
                continue;
            }

            var result = AStar.GetShortestPath(context.ParentContext.SharedInstances, context.Grid, terminalCandidate, context.Pipes);
            if (result.ReachedGoal.HasValue)
            {
                var path = result.Path;
                paths.Add((new TerminalLocation(originalTerminal.Center, terminalCandidate, direction), path));
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

            return true;
        }
        else
        {
            context.Goals.Add(originalTerminal.Terminal);
            return false;
        }
    }

    private static bool UseShortestPath(
        ChildContext context,
        ExploredPaths exploredPaths,
        Location start,
        Location originalGoal)
    {
        var originalPath = exploredPaths.GetPath(originalGoal);
        for (var i = 1; i < originalPath.Count; i++)
        {
            context.Pipes.Remove(originalPath[i]);
        }

        /*
        var clone = new PipeGrid(context.Grid);
        AddPipeEntities.Execute(clone, context.CenterToTerminals, context.Pipes);
        Visualizer.Show(clone, originalPath.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
        */

        var connectionPoints = ExplorePipes(context, originalGoal);
        var result = AStar.GetShortestPath(context.ParentContext.SharedInstances, context.Grid, start, connectionPoints);

        if (result.Path.Count > originalPath.Count
            || (result.Path.Count == originalPath.Count && CountTurns(result.Path) >= CountTurns(originalPath)))
        {
            context.Pipes.UnionWith(originalPath);
            return false;
        }

        context.Pipes.UnionWith(result.Path);
        context.UpdateIntersectionsAndGoals();

        /*
        var clone = new PipeGrid(context.Grid);
        AddPipeEntities.Execute(clone, context.CenterToTerminals, context.Pipes);
        Visualizer.Show(clone, originalPath.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
        */

        return true;
    }

    private static HashSet<Location> ExplorePipes(ChildContext context, Location start)
    {
        var toExplore = GetQueue(context);
        try
        {
            toExplore.Enqueue(start);
            var pipes = new HashSet<Location>(context.Pipes.Count) { start };

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
        finally
        {
            ReturnQueue(toExplore);
        }
    }

    private static ExploredPaths ExplorePaths(ChildContext context, Location start)
    {
        var toExplore = GetQueue(context);
        try
        {
            toExplore.Enqueue(start);
            var cameFrom = new Dictionary<Location, Location>();
            cameFrom[start] = start;


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
        finally
        {
            ReturnQueue(toExplore);
        }
    }

    private static Queue<Location> GetQueue(ChildContext context)
    {
#if USE_OBJECT_POOLING
        return QueuePool.Get();
#elif USE_SHARED_INSTANCES
        return context.ParentContext.SharedInstances.LocationQueue;
#else
        return new Queue<Location>();
#endif

#if DEBUG && USE_OBJECT_POOLING
        Interlocked.Increment(ref QueuePoolCount);
#endif
    }

    private static void ReturnQueue(Queue<Location> toExplore)
    {
#if USE_OBJECT_POOLING
        toExplore.Clear();
        QueuePool.Return(toExplore);
#elif USE_SHARED_INSTANCES
        toExplore.Clear();
#endif

#if DEBUG && USE_OBJECT_POOLING
            Interlocked.Decrement(ref QueuePoolCount);
#endif
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

                if (Pipes.Contains(pipe.Translate(1, 0)))
                {
                    neighbors++;
                }

                if (Pipes.Contains(pipe.Translate(0, -1)))
                {
                    neighbors++;
                }

                if (Pipes.Contains(pipe.Translate(-1, 0)))
                {
                    neighbors++;
                }

                if (Pipes.Contains(pipe.Translate(0, 1)))
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