using System;
using System.Collections.Generic;
using System.Linq;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public static class PlanUndergroundPipes
{
    /// <summary>
    /// The ends each occupy one spot. Any less than 3 would not be worth it since the number of underground pipes
    /// (start and end) would be equal to the number of normal pipes.
    /// </summary>
    private const int MinUnderground = 3;

    /// <summary>
    /// The maximum length of an underground pipe, including the ends. Not including the ends, this would be 9,
    /// according to https://wiki.factorio.com/Pipe_to_ground.
    /// </summary>
    private const int MaxUnderground = 11;

    public static Dictionary<Location, Direction> Execute(Context context, LocationSet pipes)
    {
        // Track underground pipes and their directions
        var locationToDirection = new Dictionary<Location, Direction>();

        ConvertInOneDirection(context, pipes, locationToDirection, (X: 0, Y: 1));
        ConvertInOneDirection(context, pipes, locationToDirection, (X: 1, Y: 0));

        Validate.UndergroundPipesArePipes(context, pipes, locationToDirection);

        return locationToDirection;
    }

    private static void ConvertInOneDirection(
        Context context,
        LocationSet pipes,
        Dictionary<Location, Direction> locationToDirection,
        (int X, int Y) forward)
    {
        (var forwardDirection, var backwardDirection) = forward switch
        {
            (1, 0) => (Direction.Right, Direction.Left),
            (0, 1) => (Direction.Down, Direction.Up),
            _ => throw new NotImplementedException()
        };
        Func<IEnumerable<Location>, IOrderedEnumerable<Location>> sort = forward switch
        {
            (1, 0) => x => x.OrderBy(l => l.Y).ThenBy(l => l.X),
            (0, 1) => x => x.OrderBy(l => l.X).ThenBy(l => l.Y),
            _ => throw new NotImplementedException()
        };

        // Find candidates for underground pipes. These are pipes that have other pipes before and after them in
        // axis they are going and no pipes next to them.

#if USE_SHARED_INSTANCES
        var candidates = context.SharedInstances.LocationSetA;
#else
        var candidates = new LocationSet();
#endif

        try
        {
            var backward = (X: forward.X * -1, Y: forward.Y * -1);
            var right = (X: forward.Y, Y: forward.X);
            var left = (X: right.X * -1, Y: right.Y * -1);

            foreach (var goal in pipes.EnumerateItems())
            {
                if ((pipes.Contains(goal.Translate(forward)) || pipes.Contains(goal.Translate(backward)))
                    && !pipes.Contains(goal.Translate(right))
                    && !pipes.Contains(goal.Translate(left)))
                {
                    if (context.LocationToTerminals.TryGetValue(goal, out var terminals))
                    {
                        if (terminals.Count > 1)
                        {
                            continue;
                        }

                        var direction = terminals.Single().Direction;
                        if (direction != forwardDirection && direction != backwardDirection)
                        {
                            continue;
                        }
                    }

                    candidates.Add(goal);
                }
            }

            if (candidates.Count == 0)
            {
                return;
            }

            var sorted = sort(candidates.EnumerateItems()).ToList();
            var currentRun = new List<Location> { sorted[0] };
            for (var i = 1; i < sorted.Count; i++)
            {
                var previous = currentRun[currentRun.Count - 1];
                var current = sorted[i];

                if (previous.X + forward.X == current.X
                    && previous.Y + forward.Y == current.Y
                    && currentRun.Count < MaxUnderground)
                {
                    currentRun.Add(current);
                    continue;
                }

                AddRunAndClear(pipes, locationToDirection, forwardDirection, backwardDirection, currentRun);

                currentRun.Add(current);
            }

            AddRunAndClear(pipes, locationToDirection, forwardDirection, backwardDirection, currentRun);
        }
        finally
        {
#if USE_SHARED_INSTANCES
            candidates.Clear();
#endif
        }
    }

    private static void AddRunAndClear(
        LocationSet pipes,
        Dictionary<Location, Direction> locationToDirection,
        Direction forwardDirection,
        Direction backwardDirection,
        List<Location> currentRun)
    {
        if (currentRun.Count >= MinUnderground)
        {
            // Convert pipes to underground pipes
            locationToDirection.Add(currentRun[0], backwardDirection);
            locationToDirection.Add(currentRun[currentRun.Count - 1], forwardDirection);

            for (var j = 1; j < currentRun.Count - 1; j++)
            {
                pipes.Remove(currentRun[j]);
            }
        }

        currentRun.Clear();
    }
}
