using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField
{
    public static class BreadthFirstFinder
    {
        public static List<Location>? GetShortestPath(SharedInstances sharedInstances, SquareGrid grid, Location start, Location goal)
        {
#if USE_SHARED_INSTANCES
            var toExplore = sharedInstances.LocationQueue;
            var parents = sharedInstances.LocationToLocation;
            var visited = sharedInstances.LocationSetA;
#else
            var toExplore = new Queue<Location>();
            var parents = new Dictionary<Location, Location>();
            var visited = new LocationSet();
#endif
            try
            {
                toExplore.Enqueue(start);

#if USE_STACKALLOC
                Span<Location> neighbors = stackalloc Location[4];
#else
                Span<Location> neighbors = new Location[4];
#endif

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
#if USE_SHARED_INSTANCES
                toExplore.Clear();
                parents.Clear();
                visited.Clear();
#endif
            }
        }
    }
}
