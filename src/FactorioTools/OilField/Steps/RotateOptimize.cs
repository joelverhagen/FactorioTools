using System;
using System.Collections.Generic;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

public static class RotateOptimize
{
    public static void Execute(Context parentContext, ILocationSet pipes)
    {
        if (parentContext.LocationToTerminals.Count < 2)
        {
            return;
        }

        var context = new ChildContext(parentContext, pipes);

        // VisualizeIntersections(context);

        var modified = true;
        var previousPipeCount = int.MaxValue;

        // Some oil fields have multiple optimal configurations with the same pipe count. Allow up to 5 of these to be
        // attempted before settling on one.
        var allowedSamePipeCounts = 5;

        while (modified && (context.Pipes.Count < previousPipeCount || allowedSamePipeCounts > 0))
        {
            var changedTerminal = false;
            foreach (var terminals in context.CenterToTerminals.Values)
            {
                if (terminals.Count != 1)
                {
                    throw new FactorioToolsException("There should be a single terminal at this point.");
                }

                var currentTerminal = terminals[0];

                if (context.LocationToTerminals[currentTerminal.Terminal].Count > 1
                    || context.Intersections.Contains(currentTerminal.Terminal))
                {
                    continue;
                }

                if (UseBestTerminal(context, currentTerminal))
                {
                    // VisualizeIntersections(context);
                    changedTerminal = true;
                }
            }

            // VisualizeIntersections(context);

            var shortenedPath = false;
            var intersections = context.Intersections.EnumerateItems().ToTableList();
            for (var i = 0; i < intersections.Count; i++)
            {
                var intersection = intersections[i];
                if (!context.Intersections.Contains(intersection))
                {
                    continue;
                }

                context.Goals.Remove(intersection);
                var exploredPaths = ExplorePaths(context, intersection);
                context.Goals.Add(intersection);

                for (var j = 0; j < exploredPaths.ReachedGoals.Count; j++)
                {
                    if (UseShortestPath(context, exploredPaths, intersection, exploredPaths.ReachedGoals[j]))
                    {
                        // VisualizeIntersections(context);
                        shortenedPath = true;
                    }
                }
            }

            modified = changedTerminal || shortenedPath;
            if (previousPipeCount == context.Pipes.Count)
            {
                allowedSamePipeCounts--;
            }
            else
            {
                previousPipeCount = context.Pipes.Count;
            }

            // VisualizeIntersections(context);
        }
    }

#if ENABLE_VISUALIZER
    private static void VisualizeIntersections(ChildContext context)
    {
        var clone = new PipeGrid(context.ExistingPipeGrid);
        AddPipeEntities.Execute(context.ParentContext, clone, context.Pipes, allowMultipleTerminals: true);
        Visualizer.Show(clone, context.Intersections.ToDelaunatorPoints().EnumerateItems(), Array.Empty<DelaunatorSharp.IEdge>());
    }
#endif

    private static bool UseBestTerminal(ChildContext context, TerminalLocation originalTerminal)
    {
        context.Goals.Remove(originalTerminal.Terminal);
        var exploredPaths = ExplorePaths(context, originalTerminal.Terminal);

        /*
        if (exploredPaths.ReachedGoals.Count == 0)
        {
            var clone = new PipeGrid(context.ExistingPipeGrid);
            AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, context.Pipes);
            Visualizer.Show(clone, context.Goals.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
        }
        */

        if (exploredPaths.ReachedGoals.Count != 1)
        {
            throw new FactorioToolsException("Only a single goal should have been reached.");
        }

        var originalGoal = exploredPaths.ReachedGoals[0];

#if !USE_SHARED_INSTANCES
        var originalPath = TableList.New<Location>();
#else
        var originalPath = context.ParentContext.SharedInstances.LocationListA;
        try
        {
#endif
            exploredPaths.AddPath(originalGoal, originalPath);

            for (var i = 1; i < originalPath.Count; i++)
            {
                context.Pipes.Remove(originalPath[i]);
            }

            var minTerminal = originalTerminal;
            var minPath = originalPath;
            var minPathTurns = CountTurns(minPath);
            var changedPath = false;

            for (var i = 0; i < TerminalOffsets.Count; i++)
            {
                (var direction, var translation) = TerminalOffsets[i];

                var terminalCandidate = originalTerminal.Center.Translate(translation);
                if (!context.Grid.IsEmpty(terminalCandidate) && !context.Grid.IsEntityType<Pipe>(terminalCandidate))
                {
                    continue;
                }

#if USE_SHARED_INSTANCES
                var newPath = minPath == context.ParentContext.SharedInstances.LocationListA ? context.ParentContext.SharedInstances.LocationListB : context.ParentContext.SharedInstances.LocationListA;
#else
                var newPath = TableList.New<Location>();
#endif
                var result = AStar.GetShortestPath(context.ParentContext, context.Grid, terminalCandidate, context.Pipes, outputList: newPath);
                if (result.Success)
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

            context.Pipes.UnionWith(minPath.EnumerateItems());

            if (changedPath)
            {
                if (minTerminal != originalTerminal)
                {
                    context.CenterToTerminals[originalTerminal.Center].Add(minTerminal);

                    if (!context.LocationToTerminals.TryGetValue(minTerminal.Terminal, out var locationTerminals))
                    {
                        locationTerminals = TableList.New(minTerminal);
                        context.LocationToTerminals.Add(minTerminal.Terminal, locationTerminals);
                    }
                    else
                    {
                        locationTerminals.Add(minTerminal);
                    }

                    EliminateOtherTerminals(context.ParentContext, minTerminal);
                }

                // Console.WriteLine($"New best terminal: {minTerminal} -> {minPath.Last()}");

                context.UpdateIntersectionsAndGoals();

                /*
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, context.Pipes);
                Visualizer.Show(clone, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
                */

                return true;
            }
            else
            {
                context.Goals.Add(originalTerminal.Terminal);
                return false;
            }
#if USE_SHARED_INSTANCES
        }
        finally
        {
            context.ParentContext.SharedInstances.LocationListA.Clear();
            context.ParentContext.SharedInstances.LocationListB.Clear();
        }
#endif
    }

    private static bool UseShortestPath(
        ChildContext context,
        ExploredPaths exploredPaths,
        Location start,
        Location originalGoal)
    {
#if !USE_SHARED_INSTANCES
        var originalPath = TableList.New<Location>();
        var connectionPoints = context.ParentContext.GetLocationSet(context.Pipes.Count, allowEnumerate: true);
#else
        var originalPath = context.ParentContext.SharedInstances.LocationListA;
        var connectionPoints = context.ParentContext.SharedInstances.LocationSetA;
        try
        {
#endif
            exploredPaths.AddPath(originalGoal, originalPath);

            for (var i = 1; i < originalPath.Count; i++)
            {
                // Does the path contain an intersection as an intermediate point? This can happen if a previous call
                // of this method with the same exploration changed the intersections.
                if (i < originalPath.Count - 1 && context.Intersections.Contains(originalPath[i]))
                {
                    // Add the path back.
                    for (var j = i - 1; j > 0; j--)
                    {
                        context.Pipes.Add(originalPath[j]);
                    }

                    return false;
                }

                context.Pipes.Remove(originalPath[i]);
            }

            /*
            var clone = new PipeGrid(context.Grid);
            AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, context.Pipes);
            Visualizer.Show(clone, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
            */

            ExplorePipes(context, originalGoal, connectionPoints);

#if !USE_SHARED_INSTANCES
            var result = AStar.GetShortestPath(context.ParentContext, context.Grid, start, connectionPoints);
#else
            var result = AStar.GetShortestPath(context.ParentContext, context.Grid, start, connectionPoints, outputList: context.ParentContext.SharedInstances.LocationListB);
            try
            {
#endif
                if (result.Path.Count > originalPath.Count
                    || (result.Path.Count == originalPath.Count && CountTurns(result.Path) >= CountTurns(originalPath)))
                {
                    context.Pipes.UnionWith(originalPath.EnumerateItems());

                    return false;
                }

                context.Pipes.UnionWith(result.Path.EnumerateItems());
                context.UpdateIntersectionsAndGoals();

                /*
                var clone2 = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone2, new(), context.CenterToTerminals, context.Pipes);
                Visualizer.Show(clone2, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
                */

                // Console.WriteLine($"Shortened path: {result.Path[0]} -> {result.Path.Last()}");

                return true;
#if USE_SHARED_INSTANCES
            }
            finally
            {
                result.Path.Clear();
            }
        }
        finally
        {
            originalPath.Clear();
            connectionPoints.Clear();
        }
#endif
    }

    private static void ExplorePipes(ChildContext context, Location start, ILocationSet pipes)
    {
#if !USE_SHARED_INSTANCES
        var toExplore = new Queue<Location>();
#else
        var toExplore = context.ParentContext.SharedInstances.LocationQueue;
        try
        {
#endif
            toExplore.Enqueue(start);
            pipes.Add(start);

#if USE_STACKALLOC && LOCATION_AS_STRUCT
            Span<Location> neighbors = stackalloc Location[4];
#else
            Span<Location> neighbors = new Location[4];
#endif

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
#if USE_SHARED_INSTANCES
        }
        finally
        {
            toExplore.Clear();
        }
#endif
    }

    private static ExploredPaths ExplorePaths(ChildContext context, Location start)
    {
#if !USE_SHARED_INSTANCES
        var toExplore = new Queue<Location>();
#else
        var toExplore = context.ParentContext.SharedInstances.LocationQueue;
        try
        {
#endif
            toExplore.Enqueue(start);
            var cameFrom = context.ParentContext.GetLocationDictionary<Location>();
            cameFrom[start] = start;

#if USE_STACKALLOC && LOCATION_AS_STRUCT
            Span<Location> neighbors = stackalloc Location[4];
#else
            Span<Location> neighbors = new Location[4];
#endif

            var reachedGoals = TableList.New<Location>();

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
#if USE_SHARED_INSTANCES
        }
        finally
        {
            toExplore.Clear();
        }
#endif
    }

    private class ChildContext
    {
        public ChildContext(Context parentContext, ILocationSet pipes)
        {
            ParentContext = parentContext;
            Pipes = pipes;
            Intersections = parentContext.GetLocationSet(pipes.Count, allowEnumerate: true);
            Goals = parentContext.GetLocationSet(pipes.Count);
            ExistingPipeGrid = new ExistingPipeGrid(parentContext.Grid, pipes);

            UpdateIntersectionsAndGoals();
        }

        public Context ParentContext { get; }
        public SquareGrid Grid => ParentContext.Grid;
        public ILocationDictionary<ITableList<TerminalLocation>> LocationToTerminals => ParentContext.LocationToTerminals;
        public ILocationDictionary<ITableList<TerminalLocation>> CenterToTerminals => ParentContext.CenterToTerminals;
        public ILocationSet Pipes { get; }
        public ILocationSet Intersections { get; }
        public ILocationSet Goals { get; }
        public ExistingPipeGrid ExistingPipeGrid { get; }

        public void UpdateIntersectionsAndGoals()
        {
            Intersections.Clear();

            Goals.Clear();
            Goals.UnionWith(LocationToTerminals.Keys);

            foreach (var pipe in Pipes.EnumerateItems())
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
        public ExploredPaths(Location start, ILocationDictionary<Location> cameFrom, ITableList<Location> reachedGoals)
        {
            Start = start;
            CameFrom = cameFrom;
            ReachedGoals = reachedGoals;
        }

        public Location Start { get; }
        public ILocationDictionary<Location> CameFrom { get; }
        public ITableList<Location> ReachedGoals { get; }

        public void AddPath(Location goal, ITableList<Location> outputList)
        {
            Helpers.AddPath(CameFrom, goal, outputList);
        }
    }
}