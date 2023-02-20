namespace Knapcode.FactorioTools.OilField.Algorithms;

internal static class Prims
{
    public static Dictionary<Location, HashSet<Location>> GetMinimumSpanningTree(
        SharedInstances sharedInstances,
        Dictionary<Location, Dictionary<Location, int>> graph,
        Location firstNode,
        bool digraph)
    {
#if USE_SHARED_INSTANCES
        var visited = sharedInstances.LocationSetA;
#else
        var visited = new HashSet<Location>();
#endif
        var priority = new PriorityQueue<(Location NodeA, Location NodeB), int>();
        var mst = new Dictionary<Location, HashSet<Location>>();

        try
        {
            visited.Add(firstNode);
            foreach ((var otherNode, var cost) in graph[firstNode])
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
                    nodes = new HashSet<Location> { nodeB };
                    mst.Add(nodeA, nodes);
                }
                else
                {
                    nodes.Add(nodeB);
                }

                foreach ((var neighbor, var cost) in graph[nodeB])
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
                    foreach (var neighbor in mst[center])
                    {
                        if (!mst.TryGetValue(neighbor, out var otherNeighbors))
                        {
                            otherNeighbors = new HashSet<Location> { center };
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
