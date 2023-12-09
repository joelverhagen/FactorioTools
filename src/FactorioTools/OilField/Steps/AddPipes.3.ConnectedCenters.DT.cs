using System.Data;
using Knapcode.FactorioTools.OilField.Algorithms;

namespace Knapcode.FactorioTools.OilField.Steps;

public static partial class AddPipes
{
    private static Dictionary<Location, HashSet<Location>> GetConnectedPumpjacksWithDelaunay(List<Location> centers)
    {
        var delaunator = GetDelauntator(centers);
        var dlGraph = centers.ToDictionary(c => c, c => new HashSet<Location>());

        for (var e = 0; e < delaunator.Triangles.Length; e++)
        {
            if (e > delaunator.Halfedges[e])
            {
                var p = centers[delaunator.Triangles[e]];
                var q = centers[delaunator.Triangles[e % 3 == 2 ? e - 2 : e + 1]];

                dlGraph[p].Add(q);
                dlGraph[q].Add(p);
            }
        }

        return dlGraph;
    }

    private static Dictionary<Location, HashSet<Location>> GetConnectedPumpjacksWithDelaunayMst(Context context, List<Location> centers)
    {
        var delaunator = GetDelauntator(centers);
        var dlGraph = centers.ToDictionary(c => c, c => new Dictionary<Location, int>());

        for (var e = 0; e < delaunator.Triangles.Length; e++)
        {
            if (e > delaunator.Halfedges[e])
            {
                var p = centers[delaunator.Triangles[e]];
                var q = centers[delaunator.Triangles[e % 3 == 2 ? e - 2 : e + 1]];

                var cost = p.GetEuclideanDistanceSquared(q);
                dlGraph[p][q] = cost;
                dlGraph[q][p] = cost;
            }
        }

        var closestToMiddle = centers.MinBy(context.Grid.Middle.GetEuclideanDistanceSquared);
        var mst = Prims.GetMinimumSpanningTree(context.SharedInstances, dlGraph, closestToMiddle, digraph: false);

        return mst;
    }

    private static Delaunator GetDelauntator(List<Location> centers)
    {
        var points = centers.Select(p => (IPoint)new Point(p.X, p.Y)).ToArray();
        var delaunator = new Delaunator(points);
        return delaunator;
    }
}
