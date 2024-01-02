using System;
using System.Collections.Generic;
using System.Linq;
using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

public static class RotateOptimize
{
    public static void Execute(Context parentContext, LocationSet pipes)
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
                var currentTerminal = terminals.Single();

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
            foreach (var intersection in context.Intersections.EnumerateItems().ToList())
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
                    if (UseShortestPath(context, exploredPaths, intersection, goal))
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

#if DEBUG
    private static void VisualizeIntersections(ChildContext context)
    {
        var clone = new PipeGrid(context.ExistingPipeGrid);
        AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, context.Pipes, allowMultipleTerminals: true);
        Visualizer.Show(clone, context.Intersections.EnumerateItems().Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
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

        var originalGoal = exploredPaths.ReachedGoals.Single();

#if NO_SHARED_INSTANCES
        var originalPath = new List<Location>();
#else
        var originalPath = context.ParentContext.SharedInstances.LocationListA;
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

            for (var i = 0; i < TerminalOffsets.Count; i++)
            {
                (var direction, var translation) = TerminalOffsets[i];

                var terminalCandidate = originalTerminal.Center.Translate(translation);
                if (!context.Grid.IsEmpty(terminalCandidate) && !context.Grid.IsEntityType<Pipe>(terminalCandidate))
                {
                    continue;
                }

#if NO_SHARED_INSTANCES
                var newPath = new List<Location>();
#else
                var newPath = minPath == context.ParentContext.SharedInstances.LocationListA ? context.ParentContext.SharedInstances.LocationListB : context.ParentContext.SharedInstances.LocationListA;
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
        }
        finally
        {
#if !NO_SHARED_INSTANCES
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
#if NO_SHARED_INSTANCES
        var originalPath = new List<Location>();
        var connectionPoints = new LocationSet(context.Pipes.Count);
#else
        var originalPath = context.ParentContext.SharedInstances.LocationListA;
        var connectionPoints = context.ParentContext.SharedInstances.LocationSetA;
#endif

        try
        {

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

#if NO_SHARED_INSTANCES
            var result = AStar.GetShortestPath(context.ParentContext.SharedInstances, context.Grid, start, connectionPoints);
#else
            var result = AStar.GetShortestPath(context.ParentContext.SharedInstances, context.Grid, start, connectionPoints, outputList: context.ParentContext.SharedInstances.LocationListB);
#endif

            /*
            if (result.ReachedGoal is null)
            {
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, context.Pipes);
                Visualizer.Show(clone, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
            }
            */

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
                var clone2 = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone2, new(), context.CenterToTerminals, context.Pipes);
                Visualizer.Show(clone2, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
                */

                // Console.WriteLine($"Shortened path: {result.Path[0]} -> {result.Path.Last()}");

                return true;
            }
            finally
            {
#if !NO_SHARED_INSTANCES
                result.Path.Clear();
#endif
            }
        }
        finally
        {
#if !NO_SHARED_INSTANCES
            originalPath.Clear();
            connectionPoints.Clear();
#endif
        }
    }

    private static void ExplorePipes(ChildContext context, Location start, LocationSet pipes)
    {
        var toExplore = GetQueue(context);
        try
        {
            toExplore.Enqueue(start);
            pipes.Add(start);

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
#if NO_SHARED_INSTANCES
        return new Queue<Location>();
#else
        return context.ParentContext.SharedInstances.LocationQueue;
#endif
    }

    private static void ReturnQueue(Queue<Location> toExplore)
    {
#if !NO_SHARED_INSTANCES
        toExplore.Clear();
#endif
    }

    private class ChildContext
    {
        public ChildContext(Context parentContext, LocationSet pipes)
        {
            ParentContext = parentContext;
            Pipes = pipes;
            Intersections = new LocationSet(pipes.Count);
            Goals = new LocationSet(pipes.Count);
            ExistingPipeGrid = new ExistingPipeGrid(parentContext.Grid, pipes);

            UpdateIntersectionsAndGoals();
        }

        public Context ParentContext { get; }
        public SquareGrid Grid => ParentContext.Grid;
        public Dictionary<Location, List<TerminalLocation>> LocationToTerminals => ParentContext.LocationToTerminals;
        public IReadOnlyDictionary<Location, List<TerminalLocation>> CenterToTerminals => ParentContext.CenterToTerminals;
        public LocationSet Pipes { get; }
        public LocationSet Intersections { get; }
        public LocationSet Goals { get; }
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