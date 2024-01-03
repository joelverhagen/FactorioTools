using System.Data;
using Knapcode.FluteSharp;
using System.Collections.Generic;
using System.Linq;

namespace Knapcode.FactorioTools.OilField;

public static partial class AddPipes
{
    private static Dictionary<Location, LocationSet> GetConnectedPumpjacksWithFLUTE(Context context)
    {
        var locationToPoint = GetLocationToFlutePoint(context);

        // Determine which terminals should be connected to each other either directly or via only Steiner points.
        var centerToCenters = new Dictionary<Location, LocationSet>();
        foreach (var (center, terminals) in context.CenterToTerminals)
        {
            var otherCenters = new LocationSet();
            var visitedPoints = new LocationSet();
            var queue = new Queue<FlutePoint>();
            foreach (var terminal in terminals)
            {
                queue.Enqueue(locationToPoint[terminal.Terminal]);
            }

            while (queue.Count > 0)
            {
                var point = queue.Dequeue();

                if (!visitedPoints.Add(point.Location))
                {
                    continue;
                }

                if (!point.Centers.Contains(center) && point.Centers.Count > 0)
                {
                    otherCenters.UnionWith(point.Centers);
                }
                else
                {
                    otherCenters.UnionWith(point.Centers);

                    foreach (var neighbor in point.Neighbors.EnumerateItems())
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            otherCenters.Remove(center);
            centerToCenters.Add(center, otherCenters);
        }

        return centerToCenters;
    }

    private class FlutePoint
    {
        public FlutePoint(Location location)
        {
            Location = location;
        }

        public bool IsEliminated { get; set; }
        public bool IsSteinerPoint => Centers.Count == 0;
        public Location Location { get; }
        public LocationSet Centers { get; } = new LocationSet();
        public List<TerminalLocation> Terminals { get; } = new List<TerminalLocation>();
#if USE_HASHSETS
        public HashSet<FlutePoint> Neighbors { get; } = new HashSet<FlutePoint>();
#else
        public Dictionary<FlutePoint, bool> Neighbors { get; } = new Dictionary<FlutePoint, bool>();
#endif

#if ENABLE_GRID_TOSTRING
        public override string ToString()
        {
            return Location.ToString();
        }
#endif
    }

    private static Dictionary<Location, FlutePoint> GetLocationToFlutePoint(Context context)
    {
        var fluteTree = GetFluteTree(context);

        // VisualizeFLUTE(context, context.CenterToTerminals.SelectMany(p => p.Value).Select(l => (IPoint)new Point(l.Terminal.X, l.Terminal.Y)), fluteTree);

        // Map the FLUTE tree into a more useful object graph.
        var locationToPoint = new Dictionary<Location, FlutePoint>();

        FlutePoint GetOrAddPoint(Dictionary<Location, FlutePoint> locationToPoint, Branch branch)
        {
            var location = new Location(branch.X, branch.Y);
            if (!locationToPoint.TryGetValue(location, out var point))
            {
                point = new FlutePoint(location);
                locationToPoint.Add(location, point);
            }

            return point;
        }

        // Explore the branches.
        foreach (var branch in fluteTree.Branch)
        {
            var current = branch;
            while (true)
            {
                var next = fluteTree.Branch[current.N];

                var currentPoint = GetOrAddPoint(locationToPoint, current);
                var nextPoint = GetOrAddPoint(locationToPoint, next);

                currentPoint.Neighbors.Add(nextPoint);
                nextPoint.Neighbors.Add(currentPoint);

                if (current.N == next.N)
                {
                    break;
                }

                current = next;
            }
        }

        // Remove self from neighbors
        foreach (var point in locationToPoint.Values)
        {
            point.Neighbors.Remove(point);
        }

        // Add in pumpjack information
        foreach ((var center, var terminals) in context.CenterToTerminals)
        {
            foreach (var terminal in terminals)
            {
                var point = locationToPoint[terminal.Terminal];
                point.Terminals.Add(terminal);
                point.Centers.Add(center);
            }
        }

        return locationToPoint;
    }

    private static Tree GetFluteTree(Context context)
    {
        /*
        var centerPoints = context
            .CenterToTerminals
            .Keys
            .Select(l => new System.Drawing.Point(l.X, l.Y))
            .ToList();
        */

        var terminalPoints = context
            .CenterToTerminals
            .Values
            .SelectMany(ts => ts.Select(t => new FluteSharp.Point(t.Terminal.X, t.Terminal.Y)))
            .ToList();

        /*
        var pumpjackPoints = context
            .Grid
            .LocationToEntity
            .Where(p => p.Value is PumpjackSide || p.Value is PumpjackCenter)
            .Select(p => p.Key)
            .Select(l => new System.Drawing.Point(l.X, l.Y))
            .ToList();
        */

        InitializeFLUTE.Execute(lutD: 6);

        return InitializeFLUTE.FLUTE!.Execute(terminalPoints);
    }

#if ENABLE_VISUALIZER
    private static void VisualizeFLUTE(Context context, IEnumerable<DelaunatorSharp.IPoint> terminalPoints, Tree fluteTree)
    {
        var steinerPoints = fluteTree
            .Branch
            .Select(b => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(b.X, b.Y))
            .Except(terminalPoints)
            .ToList();

        var edges = new HashSet<DelaunatorSharp.IEdge>();

        for (int i = 0; i < fluteTree.Branch.Length; i++)
        {
            var current = fluteTree.Branch[i];

            while (true)
            {
                var next = fluteTree.Branch[current.N];
                var edge = new DelaunatorSharp.Edge(e: 0, new DelaunatorSharp.Point(current.X, current.Y), new DelaunatorSharp.Point(next.X, next.Y));
                edges.Add(edge);

                if (current.N == next.N)
                {
                    break;
                }

                current = next;
            }
        }

        Visualizer.Show(context.Grid, steinerPoints.Concat(terminalPoints).Distinct().Select(x => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(x.X, x.Y)), edges);
    }
#endif
}
