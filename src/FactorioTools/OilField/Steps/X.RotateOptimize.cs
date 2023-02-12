using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
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

#if USE_SHARED_INSTANCES
        var originalPath = context.ParentContext.SharedInstances.LocationListA;
#else
        var originalPath = new List<Location>();
#endif

        try
        {
            exploredPaths.AddPath(originalGoal, originalPath);

            for (var i = 1; i < originalPath.Count; i++)
            {
                context.Pipes.Remove(originalPath[i]);
            }

            var minTerminal = originalTerminal;
            var minPath = originalPath;
            var minPathTurns = CountTurns(minPath);
            var changedPath = false;

            for (var i = 0; i < InitializeContext.TerminalOffsets.Count; i++)
            {
                (var direction, var translation) = InitializeContext.TerminalOffsets[i];

                var terminalCandidate = originalTerminal.Center.Translate(translation);
                if (context.Grid.IsEntityType<PumpjackSide>(terminalCandidate))
                {
                    continue;
                }

#if USE_SHARED_INSTANCES
                var newPath = minPath == context.ParentContext.SharedInstances.LocationListA ? context.ParentContext.SharedInstances.LocationListB : context.ParentContext.SharedInstances.LocationListA;
#else
            var newPath = new List<Location>();
#endif
                var result = AStar.GetShortestPath(context.ParentContext.SharedInstances, context.Grid, terminalCandidate, context.Pipes, outputList: newPath);
                if (result.ReachedGoal.HasValue)
                {
                    var terminal = new TerminalLocation(originalTerminal.Center, terminalCandidate, direction);
                    var pathTurns = CountTurns(newPath);

                    if (newPath.Count < minPath.Count
                        || (newPath.Count == minPath.Count && pathTurns < minPathTurns))
                    {
                        minPath.Clear();

                        minTerminal = terminal;
                        minPath = newPath;
                        minPathTurns = pathTurns;
                        changedPath = true;
                    }
                    else
                    {
                        newPath.Clear();
                    }
                }
                else
                {
                    newPath.Clear();
                }
            }

            context.Pipes.UnionWith(minPath);

            if (changedPath)
            {
                if (minTerminal != originalTerminal)
                {
                    context.CenterToTerminals[originalTerminal.Center].Add(minTerminal);

                    if (!context.LocationToTerminals.TryGetValue(minTerminal.Terminal, out var locationTerminals))
                    {
                        locationTerminals = new List<TerminalLocation> { minTerminal };
                        context.LocationToTerminals.Add(minTerminal.Terminal, locationTerminals);
                    }
                    else
                    {
                        locationTerminals.Add(minTerminal);
                    }

                    EliminateOtherTerminals(context.ParentContext, minTerminal);
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
        finally
        {
#if USE_SHARED_INSTANCES
            context.ParentContext.SharedInstances.LocationListA.Clear();
            context.ParentContext.SharedInstances.LocationListB.Clear();
#endif
        }
    }

    private static bool UseShortestPath(
        ChildContext context,
        ExploredPaths exploredPaths,
        Location start,
        Location originalGoal)
    {
#if USE_SHARED_INSTANCES
        var originalPath = context.ParentContext.SharedInstances.LocationListA;
#else
        var originalPath = new List<Location>();
#endif
        exploredPaths.AddPath(originalGoal, originalPath);
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

#if USE_SHARED_INSTANCES
        var result = AStar.GetShortestPath(context.ParentContext.SharedInstances, context.Grid, start, connectionPoints, outputList: context.ParentContext.SharedInstances.LocationListB);
#else
        var result = AStar.GetShortestPath(context.ParentContext.SharedInstances, context.Grid, start, connectionPoints);
#endif

        try
        {
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
        finally
        {
#if USE_SHARED_INSTANCES
            context.ParentContext.SharedInstances.LocationListA.Clear();
            context.ParentContext.SharedInstances.LocationListB.Clear();
#endif
        }
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

        public void AddPath(Location goal, List<Location> outputList)
        {
            Helpers.AddPath(CameFrom, goal, outputList);
        }
    }
}