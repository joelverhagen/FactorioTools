using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class RotateOptimize
{
    internal static HashSet<Location> Execute(Context context, HashSet<Location> pipes)
    {
        var intersections = GetIntersections(context, pipes);

        var existingPipeGrid = new ExistingPipeGrid(context.Grid);
        AddPipeEntities.Execute(existingPipeGrid, context.CenterToTerminals, pipes);

        // Visualizer.Show(existingPipeGrid, intersections.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());

        foreach ((var center, var terminals) in context.CenterToTerminals)
        {
            var currentTerminal = terminals.Single();

            if (intersections.Contains(currentTerminal.Terminal))
            {
                continue;
            }

            var newPipes = new HashSet<Location>(pipes);

            var goals = new HashSet<Location>(intersections);
            goals.UnionWith(context.LocationToTerminals.Keys);

            var paths = new List<(TerminalLocation Terminal, List<Location> Path)>();

            List<Location> originalPath;
            if (context.LocationToTerminals[currentTerminal.Terminal].Count == 1)
            {
                goals.Remove(currentTerminal.Terminal);

                var originalPathResult = AStar.GetShortestPath(existingPipeGrid, currentTerminal.Terminal, goals);
                var originalPathGoal = originalPathResult.ReachedGoal!.Value;
                originalPath = originalPathResult.Path;

                newPipes.ExceptWith(originalPath);
                newPipes.Add(originalPathGoal);
            }
            else
            {
                originalPath = new List<Location> { currentTerminal.Terminal };
            }

            paths.Add((currentTerminal, originalPath));

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
                    paths.Add((new TerminalLocation(center, terminalCandidate, direction), path));
                }
            }

            paths = paths.OrderBy(p => p.Path.Count).ToList();

            if (paths[0].Path.Count < originalPath.Count)
            {
                var newPath = paths[0].Path;
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

                /*
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone, context.CenterToTerminals, pipes);
                Visualizer.Show(clone, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
                */
            }
        }

        return pipes;
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
}