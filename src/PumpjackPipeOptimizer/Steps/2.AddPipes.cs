using System.Data;
using DelaunatorSharp;
using PumpjackPipeOptimizer.Algorithms;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

/// <summary>
/// This is a clone of Teoxoy's implementation for Factorio Blueprint Editor.
/// https://github.com/teoxoy/factorio-blueprint-editor/blob/master/packages/editor/src/core/generators/pipe.ts
/// </summary>
internal static class AddPipes
{
    public static HashSet<Location> Execute(Context context)
    {
        var centersByMiddleDistance = context
            .CenterToTerminals
            .Keys
            .OrderBy(c => c.GetManhattanDistance(context.Grid.Middle))
            .ToList();

        // Start the pipe with the shortest path between the two pumpjacks closest to the center.
        var centerToTerminalLocations = context
            .CenterToTerminals
            .ToDictionary(x => x.Key, x => x.Value.Select(t => t.Terminal).ToHashSet());
        var firstShortestPath = AddShortestPath(
            context,
            centerToTerminalLocations,
            centersByMiddleDistance[0],
            centerToTerminalLocations[centersByMiddleDistance[1]]);
        var goals = new HashSet<Location>(firstShortestPath);
        centerToTerminalLocations[centersByMiddleDistance[1]].IntersectWith(new[] { firstShortestPath[0] });

        // Then proceed with the pumpjacks furthest from the middle. This is to allow long runs of pipes early on
        // to be connected to by later pumpjacks.
        for (var i = centersByMiddleDistance.Count - 1; i >= 2; i--)
        {
            AddShortestPath(
                context,
                centerToTerminalLocations,
                centersByMiddleDistance[i],
                goals);
        }

        // Prune the terminals now that we have a selection
        foreach ((var center, var terminals) in context.CenterToTerminals)
        {
            var selectedTerminalLocation = centerToTerminalLocations[center].Single();
            var selectedTerminal = terminals.Single(t => t.Terminal == selectedTerminalLocation);
            terminals.Clear();
            terminals.Add(selectedTerminal);
        }

        // Add the known entities to the grid, to allow easier clean-up steps.
        var selectedTerminals = new HashSet<Location>();
        foreach ((_, var terminals) in context.CenterToTerminals)
        {
            var location = terminals.Single();
            selectedTerminals.Add(location.Terminal);
            if (context.Grid.IsEmpty(location.Terminal))
            {
                context.Grid.AddEntity(location.Terminal, new Terminal());
            }
        }

        foreach (var location in goals)
        {
            if (context.Grid.IsEmpty(location))
            {
                context.Grid.AddEntity(location, new Pipe());
            }
        }

        return goals;
    }

    private static List<Location> AddShortestPath(
        Context context,
        Dictionary<Location, HashSet<Location>> cetnerToTerminalLocations,
        Location startPumpjack,
        HashSet<Location> goals)
    {
        List<(Location Terminal, List<Location> Path)> shortestPaths = new();
        var shortestPathLength = 0;
        var terminals = cetnerToTerminalLocations[startPumpjack];
        foreach (var start in terminals)
        {
            var result = Dijkstras.GetShortestPaths(context.Grid, start, goals, stopOnFirstGoal: true);

            var goal = result.ReachedGoals.Single();
            var paths = result.GetStraightPaths(goal);

            var pathLength = paths[0].Count;

            if (shortestPathLength == 0 || pathLength < shortestPathLength)
            {
                shortestPaths.Clear();
                shortestPaths.AddRange(paths.Select(x => (start, x)));
                shortestPathLength = pathLength;
            }
            else if (pathLength == shortestPathLength)
            {
                shortestPaths.AddRange(paths.Select(x => (start, x)));
            }
        }

        // If there are several shortest paths with the same length, prefer ones that are straight lines (now turns).
        // This allows us to make more effective use of underground pipes.
        var shortestPath = shortestPaths
            .OrderByDescending(p => p.Path[0].X == p.Path.Last().X || p.Path[0].Y == p.Path.Last().Y)
            .First();

        terminals.IntersectWith(new[] { shortestPath.Terminal });

        foreach (var location in shortestPath.Path)
        {
            goals.Add(location);
        }

        return shortestPath.Path;
    }
}
