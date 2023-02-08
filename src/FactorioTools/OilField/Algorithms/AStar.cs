using System.Buffers;
using System.Numerics;
using Knapcode.FactorioTools.OilField.Grid;
using Microsoft.Extensions.ObjectPool;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

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

        var useVector = Vector.IsHardwareAccelerated && goals.Count >= Vector<int>.Count;
        var goalsArray = ArrayPool<Location>.Shared.Rent(goals.Count);
        goals.CopyTo(goalsArray);

        int[]? xs = null;
        int[]? ys = null;
        if (useVector)
        {
            xs = ArrayPool<int>.Shared.Rent(goals.Count);
            ys = ArrayPool<int>.Shared.Rent(goals.Count);
            for (var i = 0; i < goals.Count; i++)
            {
                xs[i] = goalsArray[i].X;
                ys[i] = goalsArray[i].Y;
            }

            ArrayPool<Location>.Shared.Return(goalsArray);
        }

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
                var currentCost = costSoFar[current];

                grid.GetNeighbors(neighbors, current);
                for (int i = 0; i < neighbors.Length; i++)
                {
                    Location next = neighbors[i];
                    if (!next.IsValid)
                    {
                        continue;
                    }

                    double newCost = currentCost + SquareGrid.NeighborCost;

                    if (!costSoFar.TryGetValue(next, out var thisCostSoFar) || newCost < thisCostSoFar)
                    {
                        costSoFar[next] = newCost;
                        double priority;
                        if (useVector)
                        {
                            priority = newCost + Heuristic(next, xs!, ys!, goals.Count, xWeight, yWeight);
                        }
                        else
                        {
                            priority = newCost + Heuristic(next, goalsArray, goals.Count, xWeight, yWeight);
                        }

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

            var path = reachedGoal.HasValue ? GetPath(cameFrom, start, reachedGoal.Value) : null;
            return new AStarResult(reachedGoal, path);
        }
        finally
        {
            cameFrom.Clear();
            CameFromPool.Return(cameFrom);

            costSoFar.Clear();
            CostSoFarPool.Return(costSoFar);

            frontier.Clear();
            FrontierPool.Return(frontier);

            if (useVector)
            {
                ArrayPool<int>.Shared.Return(xs!);
                ArrayPool<int>.Shared.Return(ys!);
            }
            else
            {
                ArrayPool<Location>.Shared.Return(goalsArray);
            }

        }
    }

    private static bool IsTurn(Location a, Location b, Location c)
    {
        var directionA = a.X == b.X ? 0 : 1;
        var directionB = b.X == c.X ? 0 : 1;
        return directionA != directionB;
    }

    private static int Heuristic(Location current, Location[] goals, int goalsCount, int xWeight, int yWeight)
    {
        var min = int.MaxValue;
        for (int i = 0; i < goalsCount; i++)
        {
            var val = xWeight * Math.Abs(goals[i].X - current.X) + yWeight * Math.Abs(goals[i].Y - current.Y);
            if (val < min)
            {
                min = val;
            }
        }

        return min;
    }

    private static int Heuristic(Location current, int[] xs, int[] ys, int goalsCount, int xWeight, int yWeight)
    {
        var remaining = goalsCount % Vector<int>.Count;

        var xWeightVector = new Vector<int>(xWeight);
        var yWeightVector = new Vector<int>(yWeight);
        var xCurrentVector = new Vector<int>(current.X);
        var yCurrentVector = new Vector<int>(current.Y);

        var minVector = new Vector<int>(int.MaxValue);

        for (var i = 0; i < goalsCount - remaining; i += Vector<int>.Count)
        {
            var xVector = new Vector<int>(xs, i);
            var yVector = new Vector<int>(ys, i);

            var xDeltaVector = Vector.Abs(xVector - xCurrentVector) * xWeightVector;
            var yDeltaVector = Vector.Abs(yVector - yCurrentVector) * yWeightVector;

            var manhattanDistanceVector = xDeltaVector + yDeltaVector;

            minVector = Vector.Min(minVector, manhattanDistanceVector);
        }

        var min = int.MaxValue;

        for (var i = 0; i < Vector<int>.Count; i++)
        {
            if (minVector[i] < min)
            {
                min = minVector[i];
            }
        }

        for (var i = goalsCount - remaining - 1; i < goalsCount; i++)
        {
            var val = xWeight * Math.Abs(xs[i] - current.X) + yWeight * Math.Abs(ys[i] - current.Y);
            if (val < min)
            {
                min = val;
            }
        }

        return min;
    }
}
