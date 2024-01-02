using System;
using System.Collections.Generic;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Algorithms;

public static class Dijkstras
{
    public static DijkstrasResult GetShortestPaths(SharedInstances sharedInstances, SquareGrid grid, Location start, LocationSet goals, bool stopOnFirstGoal)
    {
        var cameFrom = new Dictionary<Location, LocationSet>();
        cameFrom[start] = new LocationSet();
        var remainingGoals = new LocationSet(goals);

#if USE_SHARED_INSTANCES
        var priorityQueue = sharedInstances.LocationPriorityQueue;
        var costSoFar = sharedInstances.LocationToDouble;
        var inQueue = sharedInstances.LocationSetB;
#else
        var priorityQueue = new PriorityQueue<Location, double>();
        var costSoFar = new Dictionary<Location, double>();
        var inQueue = new LocationSet();
#endif

        var reachedGoals = new LocationSet();
        costSoFar[start] = 0;

        try
        {
            priorityQueue.Enqueue(start, 0);
            inQueue.Add(start);

            Span<Location> neighbors = stackalloc Location[4];

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
                            cameFrom[neighbor] = new LocationSet { current };
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

        return new DijkstrasResult(grid, cameFrom, reachedGoals);
    }

}
