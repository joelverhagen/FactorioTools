using Knapcode.FluteSharp;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public static class AddPipesConnectedCentersFLUTE
{
    public static ILocationDictionary<ILocationSet> Execute(Context context)
    {
        var locationToPoint = GetLocationToFlutePoint(context);

        // Determine which terminals should be connected to each other either directly or via only Steiner points.
        var centerToCenters = context.GetLocationDictionary<ILocationSet>();
        foreach (var (center, terminals) in context.CenterToTerminals.EnumeratePairs())
        {
            var otherCenters = context.GetLocationSet(allowEnumerate: true);
            var visitedPoints = context.GetLocationSet();
            var queue = new Queue<FlutePoint>();
            for (var i = 0; i < terminals.Count; i++)
            {
                queue.Enqueue(locationToPoint[terminals[i].Terminal]);
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
                        queue.Enqueue(locationToPoint[neighbor]);
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
        public FlutePoint(Context context, Location location)
        {
            Location = location;
            Centers = context.GetLocationSet(allowEnumerate: true);
            Neighbors = context.GetLocationSet(allowEnumerate: true);
        }

        public bool IsEliminated { get; set; }
        public bool IsSteinerPoint => Centers.Count == 0;
        public Location Location { get; }
        public ILocationSet Centers { get; }
        public ITableArray<TerminalLocation> Terminals { get; } = TableArray.New<TerminalLocation>();
        public ILocationSet Neighbors { get; }

#if ENABLE_GRID_TOSTRING
        public override string ToString()
        {
            return Location.ToString();
        }
#endif
    }

    private static ILocationDictionary<FlutePoint> GetLocationToFlutePoint(Context context)
    {
        var fluteTree = GetFluteTree(context);

        // VisualizeFLUTE(context, context.CenterToTerminals.SelectMany(p => p.Value).Select(l => (IPoint)new Point(l.Terminal.X, l.Terminal.Y)), fluteTree);

        // Map the FLUTE tree into a more useful object graph.
        var locationToPoint = context.GetLocationDictionary<FlutePoint>();

        FlutePoint GetOrAddPoint(ILocationDictionary<FlutePoint> locationToPoint, Branch branch)
        {
            var location = new Location(branch.X, branch.Y);
            if (!locationToPoint.TryGetValue(location, out var point))
            {
                point = new FlutePoint(context, location);
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

                currentPoint.Neighbors.Add(nextPoint.Location);
                nextPoint.Neighbors.Add(currentPoint.Location);

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
            point.Neighbors.Remove(point.Location);
        }

        // Add in pumpjack information
        foreach ((var center, var terminals) in context.CenterToTerminals.EnumeratePairs())
        {
            for (var i = 0; i < terminals.Count; i++)
            {
                var terminal = terminals[i];
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

        var terminalPoints = new List<Point>();
        foreach (var terminals in context.CenterToTerminals.Values)
        {
            for (var i = 0; i < terminals.Count; i++)
            {
                var terminal = terminals[i];
                terminalPoints.Add(new Point(terminal.Terminal.X, terminal.Terminal.Y));
            }
        }

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
    private static void VisualizeFLUTE(Context context, IReadOnlyCollection<DelaunatorSharp.IPoint> terminalPoints, Tree fluteTree)
    {
        /*
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
        */
    }
#endif
}
