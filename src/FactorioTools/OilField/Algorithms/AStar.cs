using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Algorithms;

/// <summary>
/// Source: https://www.redblobgames.com/pathfinding/a-star/implementation.html
/// </summary>
internal static class AStar
{
    public static AStarResult GetShortestPath(SquareGrid grid, Location start, HashSet<Location> goals, bool preferNoTurns = true, int xWeight = 1, int yWeight = 1)
    {
        var goalsList = goals.ToList();

        var sizeEstimate = 2 * start.GetManhattanDistance(goalsList[0]);
        var cameFrom = new Dictionary<Location, Location>(sizeEstimate);
        var costSoFar = new Dictionary<Location, double>(sizeEstimate);

        var frontier = new PriorityQueue<Location, double>();
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;

        Location? reachedGoal = null;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (goals.Contains(current))
            {
                reachedGoal = current;
                break;
            }

            var previous = cameFrom[current];

            List<Location> neighbors = grid.GetNeighbors(current);
            for (int i = 0; i < neighbors.Count; i++)
            {
                Location next = neighbors[i];
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

        return new AStarResult(start, reachedGoal, goals, cameFrom, costSoFar);
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
}
