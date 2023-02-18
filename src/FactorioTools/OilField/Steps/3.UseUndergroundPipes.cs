using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class UseUndergroundPipes
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

    public static void Execute(Context context, HashSet<Location> pipes)
    {
        ConvertInOneDirection(context, pipes, (X: 0, Y: 1));
        ConvertInOneDirection(context, pipes, (X: 1, Y: 0));
    }

    private static void ConvertInOneDirection(
        Context context,
        HashSet<Location> pipesAndTerminals,
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
        var candidates = new HashSet<Location>();
#endif

        try
        {
            var backward = (X: forward.X * -1, Y: forward.Y * -1);
            var right = (X: forward.Y, Y: forward.X);
            var left = (X: right.X * -1, Y: right.Y * -1);

            foreach (var goal in pipesAndTerminals)
            {
                if ((context.Grid.IsEntityType<Pipe>(goal.Translate(forward)) || context.Grid.IsEntityType<Pipe>(goal.Translate(backward)))
                    && !context.Grid.IsEntityType<Pipe>(goal.Translate(right))
                    && !context.Grid.IsEntityType<Pipe>(goal.Translate(left)))
                {
                    if (context.Grid.IsEntityType<Terminal>(goal))
                    {
                        var terminals = context.LocationToTerminals[goal];
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

            var sorted = sort(candidates).ToList();
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

                AddRunAndClear(context.Grid, forwardDirection, backwardDirection, currentRun);

                currentRun.Add(current);
            }

            AddRunAndClear(context.Grid, forwardDirection, backwardDirection, currentRun);
        }
        finally
        {
#if USE_SHARED_INSTANCES
            candidates.Clear();
#endif
        }
    }

    private static void AddRunAndClear(SquareGrid grid, Direction forwardDirection, Direction backwardDirection, List<Location> currentRun)
    {
        if (currentRun.Count >= MinUnderground)
        {
            // Convert pipes to underground pipes
            for (var j = 0; j < currentRun.Count; j++)
            {
                var l = currentRun[j];
                grid.RemoveEntity(l);
                if (j == 0)
                {
                    grid.AddEntity(l, new UndergroundPipe(backwardDirection));
                }

                if (j == currentRun.Count - 1)
                {
                    grid.AddEntity(l, new UndergroundPipe(forwardDirection));
                }
            }
        }

        currentRun.Clear();
    }

    public static Direction GetTerminalDirection(Location center, Location terminal)
    {
        var deltaX = terminal.X - center.X;
        var deltaY = terminal.Y - center.Y;
        return (deltaX, deltaY) switch
        {
            (1, -2) => Direction.Up,
            (2, -1) => Direction.Right,
            (-2, 1) => Direction.Left,
            (-1, 2) => Direction.Down,
            _ => throw new NotImplementedException(),
        };
    }
}
