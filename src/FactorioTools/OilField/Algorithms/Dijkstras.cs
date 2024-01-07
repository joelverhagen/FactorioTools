using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public static class Dijkstras
{
    public static DijkstrasResult GetShortestPaths(Context context, SquareGrid grid, Location start, ILocationSet goals, bool stopOnFirstGoal)
    {
        var cameFrom = new Dictionary<Location, ILocationSet>();
        cameFrom[start] = context.GetLocationSet();
        var remainingGoals = context.GetLocationSet(goals);

#if USE_SHARED_INSTANCES
        var priorityQueue = context.SharedInstances.LocationPriorityQueue;
        var costSoFar = context.SharedInstances.LocationToDouble;
        var inQueue = context.SharedInstances.LocationSetB;
#else
        var priorityQueue = new PriorityQueue<Location, double>();
        var costSoFar = new Dictionary<Location, double>();
        var inQueue = context.GetLocationSet();
#endif

        var reachedGoals = context.GetLocationSet();
        costSoFar[start] = 0;

        try
        {
            priorityQueue.Enqueue(start, 0);
            inQueue.Add(start);

#if USE_STACKALLOC
            Span<Location> neighbors = stackalloc Location[4];
#else
            Span<Location> neighbors = new Location[4];
#endif

            while (priorityQueue.Count > 0)
            {
                var current = priorityQueue.Dequeue();
                inQueue.Remove(current);
                var currentCost = costSoFar[current];

                if (remainingGoals.Remove(current))
                {
                    reachedGoals.Add(current);

                    if (stopOnFirstGoal || remainingGoals.Count == 0)
                    {
                        break;
                    }
                }

                grid.GetNeighbors(neighbors, current);
                for (int i = 0; i < neighbors.Length; i++)
                {
                    var neighbor = neighbors[i];
                    if (!neighbor.IsValid)
                    {
                        continue;
                    }

                    var alternateCost = currentCost + SquareGrid.NeighborCost;
                    bool previousExists;
                    if (!(previousExists = costSoFar.TryGetValue(neighbor, out var neighborCost)) || alternateCost <= neighborCost)
                    {
                        if (!previousExists || alternateCost < neighborCost)
                        {
                            costSoFar[neighbor] = alternateCost;
                            cameFrom[neighbor] = context.GetLocationSet(current);
                        }
                        else
                        {
                            cameFrom[neighbor].Add(current);
                        }

                        if (!inQueue.Contains(neighbor))
                        {
                            priorityQueue.Enqueue(neighbor, alternateCost);
                            inQueue.Add(neighbor);
                        }
                    }
                }
            }
        }
        finally
        {
#if USE_SHARED_INSTANCES
            priorityQueue.Clear();
            costSoFar.Clear();
            inQueue.Clear();
#endif
        }

        return new DijkstrasResult(cameFrom, reachedGoals);
    }

}
