using System.Data;
using DelaunatorSharp;
using PumpjackPipeOptimizer.Algorithms;

namespace PumpjackPipeOptimizer.Steps;

internal static partial class PlanPipes
{
    private static Dictionary<Location, HashSet<Location>> GetConnectedPumpjacksWithDelaunay(List<Location> centers)
    {
        var dlGraph = GetDelaunayGraph(centers);
        return dlGraph.ToDictionary(p => p.Key, p => p.Value.Keys.ToHashSet());
    }

    private static Dictionary<Location, HashSet<Location>> GetConnectedPumpjacksWithDelaunayMst(Context context, List<Location> centers)
    {
        var dlGraph2 = GetDelaunayGraph(centers);
        var closestToMiddle = centers.MinBy(context.Grid.Middle.GetEuclideanDistance);
        var mst = Prims.GetMinimumSpanningTree(dlGraph2, closestToMiddle, digraph: false);


        // Visualizer.Show(context.Grid, points, delaunator.GetEdges());
        // Visualizer.Show(context.Grid, points, mst.SelectMany(p => p.Value.Select(o => (IEdge)new Edge(0, new Point(o.X, o.Y), new Point(p.Key.X, p.Key.Y)))));

        return mst;
    }

    private static Dictionary<Location, Dictionary<Location, double>> GetDelaunayGraph(List<Location> centers)
    {
        var points = centers.Select(p => (IPoint)new Point(p.X, p.Y)).ToArray();
        var delaunator = new Delaunator(points);
        var graph = centers.ToDictionary(c => c, c => new Dictionary<Location, double>());

        for (var e = 0; e < delaunator.Triangles.Length; e++)
        {
            if (e > delaunator.Halfedges[e])
            {
                var p = centers[delaunator.Triangles[e]];
                var q = centers[delaunator.Triangles[e % 3 == 2 ? e - 2 : e + 1]];

                var cost = p.GetEuclideanDistance(q);
                graph[p][q] = cost;
                graph[q][p] = cost;
            }
        }

        return graph;
    }
}
