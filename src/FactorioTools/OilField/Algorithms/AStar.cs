using System;
using System.Collections.Generic;
using System.Numerics;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

/// <summary>
/// Source: https://www.redblobgames.com/pathfinding/a-star/implementation.html
/// </summary>
public static class AStar
{
    public static AStarResult GetShortestPath(SharedInstances sharedInstances, SquareGrid grid, Location start, LocationSet goals, bool preferNoTurns = true, int xWeight = 1, int yWeight = 1, List<Location>? outputList = null)
    {
        if (goals.Contains(start))
        {
            if (outputList is not null)
            {
                outputList.Add(start);
            }
            else
            {
                outputList = new List<Location> { start };
            }

            return new AStarResult(start, outputList);
        }

#if USE_VECTORS
        var useVector = Vector.IsHardwareAccelerated && goals.Count >= Vector<int>.Count;
#endif

#if USE_SHARED_INSTANCES
        var goalsArray = sharedInstances.GetArray(ref sharedInstances.LocationArray, goals.Count);
#else
        var goalsArray = new Location[goals.Count];
#endif
        goals.CopyTo(goalsArray);

#if USE_VECTORS
        int[]? xs = null;
        int[]? ys = null;
        if (useVector)
        {
#if USE_SHARED_INSTANCES
            xs = sharedInstances.GetArray(ref sharedInstances.IntArrayX, goals.Count);
            ys = sharedInstances.GetArray(ref sharedInstances.IntArrayY, goals.Count);
#else
            xs = new int[goals.Count];
            ys = new int[goals.Count];
#endif
            for (var i = 0; i < goals.Count; i++)
            {
                xs[i] = goalsArray[i].X;
                ys[i] = goalsArray[i].Y;
            }
        }
#endif

#if USE_SHARED_INSTANCES
        var cameFrom = sharedInstances.LocationToLocation;
        var costSoFar = sharedInstances.LocationToDouble;
        var frontier = sharedInstances.LocationPriorityQueue;
#else
        var cameFrom = new Dictionary<Location, Location>();
        var costSoFar = new Dictionary<Location, double>();
        var frontier = new PriorityQueue<Location, double>();
#endif

        try
        {
            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            Location? reachedGoal = null;
#if USE_STACKALLOC
            Span<Location> neighbors = stackalloc Location[4];
#else
            Span<Location> neighbors = new Location[4];
#endif

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

#if USE_VECTORS
                        if (useVector)
                        {
                            priority = newCost + Heuristic(next, xs!, ys!, goals.Count, xWeight, yWeight);
                        }
                        else
#endif
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

            if (!reachedGoal.HasValue)
            {
                outputList = null;
            }
            else if (outputList is not null)
            {
                AddPath(cameFrom, reachedGoal.Value, outputList);
            }
            else
            {
                outputList = GetPath(cameFrom, start, reachedGoal.Value);
            }

            return new AStarResult(reachedGoal, outputList);
        }
        finally
        {
#if USE_SHARED_INSTANCES
            cameFrom.Clear();
            costSoFar.Clear();
            frontier.Clear();
#endif
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

#if USE_VECTORS
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
#endif
}
