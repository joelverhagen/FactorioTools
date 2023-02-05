using Knapcode.FactorioTools.OilField.Grid;
using Microsoft.Extensions.ObjectPool;

namespace Knapcode.FactorioTools.OilField.Algorithms;

/// <summary>
/// Source: https://www.redblobgames.com/pathfinding/a-star/implementation.html
/// </summary>
internal static class AStar
{
    private static readonly ObjectPool<Dictionary<Location, Location>> CameFromPool = ObjectPool.Create<Dictionary<Location, Location>>();
    private static readonly ObjectPool<Dictionary<Location, double>> CostSoFarPool = ObjectPool.Create<Dictionary<Location, double>>();
    private static readonly ObjectPool<PriorityQueue<Location, double>> FrontierPool = ObjectPool.Create<PriorityQueue<Location, double>>();

    public static AStarResult GetShortestPath(SquareGrid grid, Location start, HashSet<Location> goals, bool preferNoTurns = true, int xWeight = 1, int yWeight = 1)
    {
        if (goals.Contains(start))
        {
            return new AStarResult(start, new List<Location> { start });
        }

        var goalsList = goals.ToList();

        var cameFrom = CameFromPool.Get();
        var costSoFar = CostSoFarPool.Get();
        var frontier = FrontierPool.Get();

        try
        {
            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            Location? reachedGoal = null;
            Span<Location> neighbors = stackalloc Location[4];

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (goals.Contains(current))
                {
                    reachedGoal = current;
                    break;
                }

                var previous = cameFrom[current];

                grid.GetNeighbors(neighbors, current);
                for (int i = 0; i < neighbors.Length; i++)
                {
                    Location next = neighbors[i];
                    if (!next.IsValid)
                    {
                        continue;
                    }

                    double newCost = costSoFar[current] + grid.GetNeighborCost(current, next);

                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = newCost + Heuristic(next, goalsList, xWeight, yWeight);

                        // Prefer paths without turns.
                        if (preferNoTurns && previous != current && IsTurn(previous, current, next))
                        {
                            priority += 0.0001;
                        }

                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }

            return new AStarResult(reachedGoal, GetPath(cameFrom, start, reachedGoal));
        }
        finally
        {
            cameFrom.Clear();
            CameFromPool.Return(cameFrom);

            costSoFar.Clear();
            CostSoFarPool.Return(costSoFar);

            frontier.Clear();
            FrontierPool.Return(frontier);
        }
    }

    private static bool IsTurn(Location a, Location b, Location c)
    {
        var directionA = a.X == b.X ? 0 : 1;
        var directionB = b.X == c.X ? 0 : 1;
        return directionA != directionB;
    }

    private static double Heuristic(Location current, List<Location> goals, int xWeight, int yWeight)
    {
        var min = double.MaxValue;
        for (int i = 0; i < goals.Count; i++)
        {
            Location goal = goals[i];
            var val = xWeight * Math.Abs(goal.X - current.X) + yWeight * Math.Abs(goal.Y - current.Y);
            if (val < min)
            {
                min = val;
            }
        }

        return min;
    }

    private static List<Location>? GetPath(Dictionary<Location, Location> cameFrom, Location start, Location? reachedGoal)
    {
        if (!reachedGoal.HasValue)
        {
            return null;
        }

        var current = reachedGoal.Value;
        var sizeEstimate = 2 * start.GetManhattanDistance(current);
        var path = new List<Location>(sizeEstimate);
        while (true)
        {
            var next = cameFrom[current];
            path.Add(current);
            if (next == current)
            {
                break;
            }

            current = next;
        }

        return path;
    }
}
