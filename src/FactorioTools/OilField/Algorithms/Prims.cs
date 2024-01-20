using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public static class Prims
{
    public static ILocationDictionary<ILocationSet> GetMinimumSpanningTree(
        Context context,
        ILocationDictionary<ILocationDictionary<int>> graph,
        Location firstNode,
        bool digraph)
    {
        var priority = new PriorityQueue<Tuple<Location, Location>, int>();
        var mst = context.GetLocationDictionary<ILocationSet>();

#if !USE_SHARED_INSTANCES
        var visited = context.GetLocationSet();
#else
        var visited = context.SharedInstances.LocationSetA;
        try
        {
#endif
        visited.Add(firstNode);
            foreach ((var otherNode, var cost) in graph[firstNode].EnumeratePairs())
            {
                priority.Enqueue(Tuple.Create(firstNode, otherNode), cost);
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
                        priority.Enqueue(Tuple.Create(nodeB, neighbor), cost);
                    }
                }
            }

            if (!digraph)
            {
                // Make the MST bidirectional (a graph, not a digraph).
                var keys = mst.Keys.ToTableArray();
                for (var i = 0; i < keys.Count; i++)
                {
                    var center = keys[i];
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
#if USE_SHARED_INSTANCES
        }
        finally
        {
            visited.Clear();
        }
#endif
    }
}
