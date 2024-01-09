using System.Collections.Generic;
using System.Data;
using DelaunatorSharp;

namespace Knapcode.FactorioTools.OilField;

public static partial class AddPipes
{
    private static ILocationDictionary<ILocationSet> GetConnectedPumpjacksWithDelaunay(Context context, List<Location> centers)
    {
        var delaunator = GetDelauntator(centers);
        var dlGraph = centers.ToDictionary(context, c => c, c => context.GetLocationSet(allowEnumerate: true));

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

    private static ILocationDictionary<ILocationSet> GetConnectedPumpjacksWithDelaunayMst(Context context, List<Location> centers)
    {
        var delaunator = GetDelauntator(centers);
        var dlGraph = centers.ToDictionary(context, c => c, c => context.GetLocationDictionary<int>());

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

        var closestToMiddle = centers.MinBy(context.Grid.Middle.GetEuclideanDistanceSquared)!;
        var mst = Prims.GetMinimumSpanningTree(context, dlGraph, closestToMiddle, digraph: false);

        return mst;
    }

    private static Delaunator GetDelauntator(List<Location> centers)
    {
        var points = new IPoint[centers.Count];
        for (var i = 0; i < centers.Count; i++)
        {
            var center = centers[i];
            points[i] = new Point(center.X, center.Y);
        }
        var delaunator = new Delaunator(points);
        return delaunator;
    }
}
