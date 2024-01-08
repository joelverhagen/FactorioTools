using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField
{
    public static class BreadthFirstFinder
    {
        public static List<Location>? GetShortestPath(Context context, Location start, Location goal)
        {
#if USE_SHARED_INSTANCES
            var toExplore = context.SharedInstances.LocationQueue;
            var parents = context.SharedInstances.LocationToLocation;
            var visited = context.SharedInstances.LocationSetA;
#else
            var toExplore = new Queue<Location>();
            var parents = context.GetLocationDictionary<Location>();
            var visited = context.GetLocationSet();
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

                    context.Grid.GetNeighbors(neighbors, current);
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
