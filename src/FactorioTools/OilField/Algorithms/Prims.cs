using System.Collections.Generic;
using System.Linq;

namespace Knapcode.FactorioTools.OilField;

public static class Prims
{
    public static ILocationDictionary<ILocationSet> GetMinimumSpanningTree(
        Context context,
        ILocationDictionary<ILocationDictionary<int>> graph,
        Location firstNode,
        bool digraph)
    {
#if USE_SHARED_INSTANCES
        var visited = context.SharedInstances.LocationSetA;
#else
        var visited = context.GetLocationSet();
#endif
        var priority = new PriorityQueue<(Location NodeA, Location NodeB), int>();
        var mst = context.GetLocationDictionary<ILocationSet>();

        try
        {
            visited.Add(firstNode);
            foreach ((var otherNode, var cost) in graph[firstNode].EnumeratePairs())
            {
                priority.Enqueue((firstNode, otherNode), cost);
            }

            while (priority.Count > 0)
            {
                (var nodeA, var nodeB) = priority.Dequeue();
                if (!visited.Add(nodeB))
                {
                    continue;
                }

                if (!mst.TryGetValue(nodeA, out var nodes))
                {
                    nodes = context.GetLocationSet(nodeB, allowEnumerate: true);
                    mst.Add(nodeA, nodes);
                }
                else
                {
                    nodes.Add(nodeB);
                }

                foreach ((var neighbor, var cost) in graph[nodeB].EnumeratePairs())
                {
                    if (!visited.Contains(neighbor))
                    {
                        priority.Enqueue((nodeB, neighbor), cost);
                    }
                }
            }

            if (!digraph)
            {
                // Make the MST bidirectional (a graph, not a digraph).
                foreach (var center in mst.Keys.ToList())
                {
                    foreach (var neighbor in mst[center].EnumerateItems())
                    {
                        if (!mst.TryGetValue(neighbor, out var otherNeighbors))
                        {
                            otherNeighbors = context.GetLocationSet(center, allowEnumerate: true);
                            mst.Add(neighbor, otherNeighbors);
                        }
                        else
                        {
                            otherNeighbors.Add(center);
                        }
                    }
                }
            }

            return mst;
        }
        finally
        {
#if USE_SHARED_INSTANCES
            visited.Clear();
#endif
        }
    }
}
