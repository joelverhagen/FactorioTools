using System;

namespace Knapcode.FactorioTools.OilField;

public static class Dijkstras
{
    public static DijkstrasResult GetShortestPaths(Context context, SquareGrid grid, Location start, ILocationSet goals, bool stopOnFirstGoal, bool allowGoalEnumerate)
    {
        var cameFrom = context.GetLocationDictionary<ILocationSet>();
        cameFrom[start] = context.GetLocationSet();
        var remainingGoals = context.GetLocationSet(goals);
        var reachedGoals = context.GetLocationSet(allowEnumerate: allowGoalEnumerate);

#if !USE_SHARED_INSTANCES
        var priorityQueue = new System.Collections.Generic.PriorityQueue<Location, double>();
        var costSoFar = context.GetLocationDictionary<double>();
        var inQueue = context.GetLocationSet();
#else
        var priorityQueue = context.SharedInstances.LocationPriorityQueue;
        var costSoFar = context.SharedInstances.LocationToDouble;
        var inQueue = context.SharedInstances.LocationSetB;
        try
        {
#endif
            costSoFar[start] = 0;

            priorityQueue.Enqueue(start, 0);
            inQueue.Add(start);

#if RENT_NEIGHBORS
            Location[] neighbors = context.SharedInstances.GetNeighborArray();
#else
#if USE_STACKALLOC && LOCATION_AS_STRUCT
            Span<Location> neighbors = stackalloc Location[4];
#else
            Span<Location> neighbors = new Location[4];
#endif
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

#if RENT_NEIGHBORS
            context.SharedInstances.ReturnNeighborArray(neighbors);
#endif

#if USE_SHARED_INSTANCES
        }
        finally
        {
            priorityQueue.Clear();
            costSoFar.Clear();
            inQueue.Clear();
        }
#endif

        return new DijkstrasResult(cameFrom, reachedGoals);
    }

}
