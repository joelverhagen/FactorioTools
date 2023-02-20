using System.Data;
using DelaunatorSharp;
using Knapcode.FluteSharp;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddPipes
{
    private static Dictionary<Location, HashSet<Location>> GetConnectedPumpjacksWithFLUTE(Context context)
    {
        var locationToPoint = GetLocationToFlutePoint(context);

        var centerToPoints = context
            .CenterToTerminals
            .ToDictionary(p => p.Key, p => p.Value.Select(t => locationToPoint[t.Terminal]).ToHashSet());

        // Determine which terminals should be connected to each other either directly or via only Steiner points.
        var centerToCenters = new Dictionary<Location, HashSet<Location>>();
        foreach (var center in context.CenterToTerminals.Keys)
        {
            var otherCenters = new HashSet<Location>();
            var visitedPoints = new HashSet<FlutePoint>();
            var queue = new Queue<FlutePoint>();
            foreach (var point in centerToPoints[center])
            {
                queue.Enqueue(point);
            }

            while (queue.Count > 0)
            {
                var point = queue.Dequeue();

                if (!visitedPoints.Add(point))
                {
                    continue;
                }

                if ((point.Centers.Contains(center) && point.Centers.Count > 1)
                    || (!point.Centers.Contains(center) && point.Centers.Count > 0))
                {
                    otherCenters.UnionWith(point.Centers);
                }
                else
                {
                    foreach (var neighbor in point.Neighbors)
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
        public HashSet<Location> Centers { get; } = new HashSet<Location>();
        public HashSet<TerminalLocation> Terminals { get; } = new HashSet<TerminalLocation>();
        public HashSet<FlutePoint> Neighbors { get; } = new HashSet<FlutePoint>();

        public override string ToString()
        {
            return Location.ToString();
        }
    }

    private static Dictionary<Location, FlutePoint> GetLocationToFlutePoint(Context context)
    {
        var fluteTree = GetFluteTree(context);

        // VisualizeFLUTE(context, context.CenterToTerminals.SelectMany(p => p.Value).Select(l => new System.Drawing.Point(l.Terminal.X, l.Terminal.Y)).ToList(), fluteTree);

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
            .SelectMany(ts => ts.Select(t => new System.Drawing.Point(t.Terminal.X, t.Terminal.Y)))
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

#if DEBUG
    private static void VisualizeFLUTE(Context context, List<System.Drawing.Point> terminalPoints, Tree fluteTree)
    {
        var steinerPoints = fluteTree
            .Branch
            .Select(b => new System.Drawing.Point(b.X, b.Y))
            .Except(terminalPoints)
            .ToList();

        var edges = new HashSet<IEdge>();

        for (int i = 0; i < fluteTree.Branch.Length; i++)
        {
            var current = fluteTree.Branch[i];

            while (true)
            {
                var next = fluteTree.Branch[current.N];
                var edge = new Edge(e: 0, new Point(current.X, current.Y), new Point(next.X, next.Y));
                edges.Add(edge);

                if (current.N == next.N)
                {
                    break;
                }

                current = next;
            }
        }

        Visualizer.Show(context.Grid, steinerPoints.Concat(terminalPoints).Distinct().Select(x => (IPoint)new Point(x.X, x.Y)), edges);
    }
#endif
}
