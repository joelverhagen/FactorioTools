using System;
using System.Collections.Generic;
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

    public static ILocationDictionary<Direction> Execute(Context context, ILocationSet pipes)
    {
        // Track underground pipes and their directions
        var locationToDirection = context.GetLocationDictionary<Direction>();

        ConvertInOneDirection(context, pipes, locationToDirection, new Location(0, 1));
        ConvertInOneDirection(context, pipes, locationToDirection, new Location(1, 0));

        Validate.UndergroundPipesArePipes(context, pipes, locationToDirection);

        return locationToDirection;
    }

    private static void ConvertInOneDirection(
        Context context,
        ILocationSet pipes,
        ILocationDictionary<Direction> locationToDirection,
        Location forward)
    {
        Direction forwardDirection;
        Direction backwardDirection;
        Comparison<Location> sort;

        if (forward.X == 1 && forward.Y == 0)
        {
            forwardDirection = Direction.Right;
            backwardDirection = Direction.Left;
            sort = static (a, b) =>
            {
                var c = a.Y.CompareTo(b.Y);
                if (c != 0)
                {
                    return c;
                }

                return a.X.CompareTo(b.X);
            };
        }
        else if (forward.X == 0 && forward.Y == 1)
        {
            forwardDirection = Direction.Down;
            backwardDirection = Direction.Up;
            sort = static (a, b) =>
            {
                var c = a.X.CompareTo(b.X);
                if (c != 0)
                {
                    return c;
                }

                return a.Y.CompareTo(b.Y);
            };
        }
        else
        {
            throw new NotImplementedException();
        }

        // Find candidates for underground pipes. These are pipes that have other pipes before and after them in
        // axis they are going and no pipes next to them.

#if !USE_SHARED_INSTANCES
        var candidates = context.GetLocationSet(allowEnumerate: true);
#else
        var candidates = context.SharedInstances.LocationSetA;
        try
        {
#endif
            var backward = new Location(forward.X * -1, forward.Y * -1);
            var right = new Location(forward.Y, forward.X);
            var left = new Location(right.X * -1, right.Y * -1);

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

                        var direction = terminals[0].Direction;
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

            var sorted = candidates.EnumerateItems().ToTableArray();
            sorted.Sort(sort);

            var currentRun = TableArray.New(sorted[0]);
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
#if USE_SHARED_INSTANCES
        }
        finally
        {
            candidates.Clear();
        }
#endif
    }

    private static void AddRunAndClear(
        ILocationSet pipes,
        ILocationDictionary<Direction> locationToDirection,
        Direction forwardDirection,
        Direction backwardDirection,
        ITableArray<Location> currentRun)
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
