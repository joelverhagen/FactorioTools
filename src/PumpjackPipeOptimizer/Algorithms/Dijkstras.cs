using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Algorithms;

internal static class Dijkstras
{
    public static DijkstrasResult GetShortestPaths(SquareGrid grid, Location start, HashSet<Location> goals, bool stopOnFirstGoal)
    {
        var locationToCost = new Dictionary<Location, double>();
        locationToCost[start] = 0;
        var locationToPrevious = new Dictionary<Location, HashSet<Location>>();
        locationToPrevious[start] = new HashSet<Location>();
        var remainingGoals = new HashSet<Location>(goals);
        var reachedGoals = new HashSet<Location>();

        var priorityQueue = new PriorityQueue<Location, double>();
        var inQueue = new HashSet<Location>();
        priorityQueue.Enqueue(start, 0);
        inQueue.Add(start);

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

            var neighbors = grid.GetNeighbors(current);
            foreach (var neighbor in neighbors)
            {
                var alternateCost = currentCost + grid.GetNeighborCost(current, neighbor);
                bool previousExists;
                if (!(previousExists = locationToCost.TryGetValue(neighbor, out var neighborCost)) || alternateCost <= neighborCost)
                {
                    if (!previousExists || alternateCost < neighborCost)
                    {
                        locationToCost[neighbor] = alternateCost;
                        locationToPrevious[neighbor] = new HashSet<Location> { current };
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

        return new DijkstrasResult(grid, locationToCost, locationToPrevious, reachedGoals);
    }

}
