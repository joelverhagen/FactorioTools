using PumpjackPipeOptimizer.Algorithms;
using PumpjackPipeOptimizer.Grid;
using PumpjackPipeOptimizer.Steps;

namespace PumpjackPipeOptimizer;

internal class RotateOptimize
{
    internal static HashSet<Location> Execute(Context context, HashSet<Location> pipes)
    {
        var intersections = new HashSet<Location>();
        var allTerminals = context
            .CenterToTerminals
            .Values
            .SelectMany(ts => ts)
            .Select(t => t.Terminal)
            .ToHashSet();

        foreach (var pipe in pipes)
        {
            var neighbors = 0;
            foreach (var direction in SquareGrid.Directions)
            {
                var neighbor = pipe.Translate(direction);
                if (pipes.Contains(neighbor))
                {
                    neighbors++;
                }
            }

            if (neighbors > 2 || (allTerminals.Contains(pipe) && neighbors > 1))
            {
                intersections.Add(pipe);
            }
        }

        var existingPipeGrid = new ExistingPipeGrid(context.Grid);
        foreach (var pipe in pipes)
        {
            if (allTerminals.Contains(pipe))
            {
                existingPipeGrid.AddEntity(pipe, new Terminal());
            }
            else
            {
                existingPipeGrid.AddEntity(pipe, new Pipe());
            }
        }

        // Visualizer.Show(existingPipeGrid, intersections.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());

        foreach ((var center, var terminals) in context.CenterToTerminals)
        {
            var currentTerminal = terminals.Single();

            if (intersections.Contains(currentTerminal.Terminal))
            {
                continue;
            }

            var goals = new HashSet<Location>(intersections);
            goals.UnionWith(allTerminals);
            goals.Remove(currentTerminal.Terminal);

            var originalPathResult = Dijkstras.GetShortestPaths(existingPipeGrid, currentTerminal.Terminal, goals, stopOnFirstGoal: true);
            var originalPathGoal = originalPathResult.ReachedGoals.Single();
            var originalPath = originalPathResult.GetStraightPaths(originalPathGoal).Single();

            var newPipes = new HashSet<Location>(pipes);
            newPipes.ExceptWith(originalPath);
            newPipes.Add(originalPathGoal);

            var paths = new List<(TerminalLocation Terminal, List<Location> Path)>
            {
                (currentTerminal, originalPath)
            };

            foreach ((var direction, var translation) in InitializeContext.TerminalOffsets)
            {
                var terminalCandidate = center.Translate(translation);

                if (terminalCandidate == currentTerminal.Terminal
                    || context.Grid.IsEntityType<PumpjackSide>(terminalCandidate))
                {
                    continue;
                }

                var result = Dijkstras.GetShortestPaths(context.Grid, terminalCandidate, newPipes, stopOnFirstGoal: true);
                var reachedGoal = result.ReachedGoals.Single();
                var path = result.GetStraightPaths(reachedGoal).First();

                paths.Add((new TerminalLocation(center, terminalCandidate, direction), path));
            }

            paths = paths.OrderBy(p => p.Path.Count).ToList();

            if (paths[0].Path.Count < originalPath.Count)
            {
                newPipes.UnionWith(paths[0].Path);
                context.CenterToTerminals[center].Clear();
                context.CenterToTerminals[center].Add(paths[0].Terminal);

                pipes = newPipes;

                /*
                var clone = new PipeGrid(context.Grid);
                AddPipes.AddGridEntities(clone, context.CenterToTerminals, pipes);
                Visualizer.Show(clone, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
                */
            }
        }

        return pipes;
    }
}