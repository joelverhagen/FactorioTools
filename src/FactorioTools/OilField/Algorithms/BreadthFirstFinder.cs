using System;
using System.Collections.Generic;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Algorithms
{
    public static class BreadthFirstFinder
    {
        public static List<Location>? GetShortestPath(SharedInstances sharedInstances, SquareGrid grid, Location start, Location goal)
        {
#if NO_SHARED_INSTANCES
            var toExplore = new Queue<Location>();
            var parents = new Dictionary<Location, Location>();
            var visited = new LocationSet();
#else
            var toExplore = sharedInstances.LocationQueue;
            var parents = sharedInstances.LocationToLocation;
            var visited = sharedInstances.LocationSetA;
#endif
            try
            {
                toExplore.Enqueue(start);

                Span<Location> neighbors = stackalloc Location[4];

                while (toExplore.Count > 0)
                {
                    var current = toExplore.Dequeue();
                    if (!visited.Add(current))
                    {
                        continue;
                    }

                    if (current == goal)
                    {
                        var output = new List<Location> { current };
                        while (parents.TryGetValue(current, out var parent))
                        {
                            output.Add(parent);
                            current = parent;
                        }

                        output.Reverse();
                        return output;
                    }

                    grid.GetNeighbors(neighbors, current);
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        Location next = neighbors[i];
                        if (!next.IsValid
                            || visited.Contains(next)
                            || !parents.TryAdd(next, current))
                        {
                            continue;
                        }

                        toExplore.Enqueue(next);
                    }
                }

                return null;
            }
            finally
            {
#if !NO_SHARED_INSTANCES
                toExplore.Clear();
                parents.Clear();
                visited.Clear();
#endif
            }
        }
    }
}
