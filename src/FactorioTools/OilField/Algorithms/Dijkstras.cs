using Knapcode.FactorioTools.OilField.Grid;
using Microsoft.Extensions.ObjectPool;

namespace Knapcode.FactorioTools.OilField.Algorithms;

internal static class Dijkstras
{
    public static readonly ObjectPool<Dictionary<Location, HashSet<Location>>> LocationToPreviousPool = ObjectPool.Create<Dictionary<Location, HashSet<Location>>>();
    public static readonly ObjectPool<HashSet<Location>> LocationHashSetPool = ObjectPool.Create<HashSet<Location>>();
    private static readonly ObjectPool<Dictionary<Location, double>> LocationToCostPool = ObjectPool.Create<Dictionary<Location, double>>();
    private static readonly ObjectPool<PriorityQueue<Location, double>> PriorityQueuePool = ObjectPool.Create<PriorityQueue<Location, double>>();

    public static DijkstrasResult GetShortestPaths(SquareGrid grid, Location start, HashSet<Location> goals, bool stopOnFirstGoal)
    {
        var locationToCost = LocationToCostPool.Get();
        var locationToPrevious = LocationToPreviousPool.Get();
        var remainingGoals = LocationHashSetPool.Get();
        var reachedGoals = LocationHashSetPool.Get();
        var priorityQueue = PriorityQueuePool.Get();
        var inQueue = LocationHashSetPool.Get();

        try
        {
            locationToPrevious[start] = LocationHashSetPool.Get();
            locationToCost[start] = 0;
            remainingGoals.UnionWith(goals);

            priorityQueue.Enqueue(start, 0);
            inQueue.Add(start);

            Span<Location> neighbors = stackalloc Location[4];

            while (priorityQueue.Count > 0)
            {
                var current = priorityQueue.Dequeue();
                inQueue.Remove(current);
                var currentCost = locationToCost[current];

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

                    var alternateCost = currentCost + grid.GetNeighborCost(current, neighbor);
                    bool previousExists;
                    if (!(previousExists = locationToCost.TryGetValue(neighbor, out var neighborCost)) || alternateCost <= neighborCost)
                    {
                        if (!previousExists || alternateCost < neighborCost)
                        {
                            locationToCost[neighbor] = alternateCost;
                            var previous = LocationHashSetPool.Get();
                            previous.Add(current);
                            locationToPrevious[neighbor] = previous;
                        }
                        else
                        {
                            locationToPrevious[neighbor].Add(current);
                        }

                        if (!inQueue.Contains(neighbor))
                        {
                            priorityQueue.Enqueue(neighbor, alternateCost);
                            inQueue.Add(neighbor);
                        }
                    }
                }
            }

            return new DijkstrasResult(grid, locationToPrevious, reachedGoals);
        }
        finally
        {
            locationToCost.Clear();
            LocationToCostPool.Return(locationToCost);

            remainingGoals.Clear();
            LocationHashSetPool.Return(remainingGoals);

            priorityQueue.Clear();
            PriorityQueuePool.Return(priorityQueue);

            inQueue.Clear();
            LocationHashSetPool.Return(inQueue);
        }

    }

}
