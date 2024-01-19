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
    public static AStarResult GetShortestPath(
        Context context,
        SquareGrid grid,
        Location start,
        ILocationSet goals,
        bool preferNoTurns = true,
        int xWeight = 1,
        int yWeight = 1,
        ITableList<Location>? outputList = null)
    {
        if (goals.Contains(start))
        {
            if (outputList is not null)
            {
                outputList.Add(start);
            }
            else
            {
                outputList = TableList.New(start);
            }

            return new AStarResult(success: true, start, outputList);
        }

#if USE_SHARED_INSTANCES
        var goalsArray = context.SharedInstances.GetArray(ref context.SharedInstances.LocationArray, goals.Count);
#else
        var goalsArray = new Location[goals.Count];
#endif
        goals.CopyTo(goalsArray);

#if USE_VECTORS
        var useVector = Vector.IsHardwareAccelerated && goals.Count >= Vector<int>.Count;

        int[]? xs = null;
        int[]? ys = null;
        if (useVector)
        {
#if USE_SHARED_INSTANCES
            xs = context.SharedInstances.GetArray(ref context.SharedInstances.IntArrayX, goals.Count);
            ys = context.SharedInstances.GetArray(ref context.SharedInstances.IntArrayY, goals.Count);
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

#if !USE_SHARED_INSTANCES
        var cameFrom = context.GetLocationDictionary<Location>();
        var costSoFar = context.GetLocationDictionary<double>();
        var frontier = new PriorityQueue<Location, double>();
#else
        var cameFrom = context.SharedInstances.LocationToLocation;
        var costSoFar = context.SharedInstances.LocationToDouble;
        var frontier = context.SharedInstances.LocationPriorityQueue;
        try
        {
#endif
        frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            Location reachedGoal = Location.Invalid;
            bool success = false;
#if RENT_NEIGHBORS
            Location[] neighbors = context.SharedInstances.GetNeighborArray();
#else
#if USE_STACKALLOC && LOCATION_AS_STRUCT
            Span<Location> neighbors = stackalloc Location[4];
#else
            Span<Location> neighbors = new Location[4];
#endif
#endif

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (goals.Contains(current))
                {
                    reachedGoal = current;
                    success = true;
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

#if RENT_NEIGHBORS
            context.SharedInstances.ReturnNeighborArray(neighbors);
#endif

            if (!success)
            {
                outputList = null;
            }
            else if (outputList is not null)
            {
                AddPath(cameFrom, reachedGoal, outputList);
            }
            else
            {
                outputList = GetPath(cameFrom, start, reachedGoal);
            }

            return new AStarResult(success, reachedGoal, outputList);
#if USE_SHARED_INSTANCES
        }
        finally
        {
            cameFrom.Clear();
            costSoFar.Clear();
            frontier.Clear();
        }
#endif
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
