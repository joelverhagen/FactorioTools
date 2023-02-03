using DelaunatorSharp;

namespace PumpjackPipeOptimizer.Steps;

internal static class Helpers
{
    public static bool AreLocationsCollinear(List<Location> locations)
    {
        double lastSlope = 0;
        for (var i = 0; i < locations.Count; i++)
        {
            if (i == locations.Count - 1)
            {
                return true;
            }

            var node = locations[i];
            var next = locations[i + 1];
            double dX = Math.Abs(node.X - next.X);
            double dY = Math.Abs(node.Y - next.Y);
            if (i == 0)
            {
                lastSlope = dY / dX;
            }
            else if (lastSlope != dY / dX)
            {
                break;
            }
        }

        return false;
    }

    public static int CountTurns(List<Location> path)
    {
        var previousDirection = -1;
        var turns = 0;
        for (var i = 1; i < path.Count; i++)
        {
            var currentDirection = path[i].X == path[i - 1].X ? 0 : 1;
            if (previousDirection != -1)
            {
                if (previousDirection != currentDirection)
                {
                    turns++;
                }
            }

            previousDirection = currentDirection;
        }

        return turns;
    }

    public static List<Location> MakeStraightLine(Location a, Location b)
    {
        if (a.X == b.X)
        {
            return Enumerable
                .Range(Math.Min(a.Y, b.Y), Math.Abs(a.Y - b.Y) + 1)
                .Select(y => new Location(a.X, y))
                .ToList();
        }

        if (a.Y == b.Y)
        {
            return Enumerable
                .Range(Math.Min(a.X, b.X), Math.Abs(a.X - b.X) + 1)
                .Select(x => new Location(x, a.Y))
                .ToList();
        }

        throw new ArgumentException("The two points must be one the same line either horizontally or vertically.");
    }

    /// <summary>
    /// Source: https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts#L62
    /// </summary>
    public static List<Endpoints> PointsToLines(IEnumerable<Location> nodes)
    {
        var filteredNodes = nodes
            .Distinct()
            .OrderBy(x => x.X)
            .ThenBy(x => x.Y)
            .ToList();

        if (filteredNodes.Count == 1)
        {
            return new List<Endpoints> { new Endpoints(filteredNodes[0], filteredNodes[0]) };
        }
        else if (filteredNodes.Count == 2)
        {
            return new List<Endpoints> { new Endpoints(filteredNodes[0], filteredNodes[1]) };
        }

        // Check that nodes are not collinear
        if (AreLocationsCollinear(filteredNodes))
        {
            return Enumerable
            .Range(1, filteredNodes.Count - 1)
                .Select(i => new Endpoints(filteredNodes[i - 1], filteredNodes[i]))
                .ToList();
        }


        var points = filteredNodes.Select<Location, IPoint>(x => new Point(x.X, x.Y)).ToArray();
        var delaunator = new Delaunator(points);

        var lines = new List<Endpoints>();
        for (var e = 0; e < delaunator.Triangles.Length; e++)
        {
            if (e > delaunator.Halfedges[e])
            {
                var p = filteredNodes[delaunator.Triangles[e]];
                var q = filteredNodes[delaunator.Triangles[e % 3 == 2 ? e - 2 : e + 1]];
                lines.Add(new Endpoints(p, q));
            }
        }

        return lines;
    }

    public record Endpoints(Location A, Location B);
}
