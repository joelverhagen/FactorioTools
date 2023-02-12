using System.Collections;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class Helpers
{
    public record PoweredEntity(Location Center, int Width, int Height);

    public static Dictionary<Location, BitArray> GetCandidateToCovered(Context context, List<PoweredEntity> entities, int providerWidth, int providerHeight, int supplyWidth, int supplyHeight)
    {
        var candidateToCovered = new Dictionary<Location, BitArray>();

        /*
        providerWidth = 1;
        providerHeight = 1;
        supplyWidth = 7;
        supplyHeight = 7;
        */

        for (int i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];

            // entity = new PoweredEntity(new Location(3, 3), 4, 4);

            var minX = Math.Max((providerWidth - 1) / 2, entity.Center.X - ((entity.Width - 1) / 2) - (supplyWidth / 2));
            var minY = Math.Max((providerHeight - 1) / 2, entity.Center.Y - ((entity.Height - 1) / 2) - (supplyHeight / 2));
            var maxX = Math.Min(context.Grid.Width - (providerWidth / 2) - 1, entity.Center.X + (entity.Width / 2) + ((supplyWidth - 1) / 2));
            var maxY = Math.Min(context.Grid.Height - (providerHeight / 2) - 1, entity.Center.Y + (entity.Height / 2) + ((supplyHeight - 1) / 2));

            // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var candidate = new Location(x, y);
                    var fits = DoesProviderFit(context.Grid, providerWidth, providerHeight, candidate);

                    if (!fits)
                    {
                        continue;
                    }

                    if (!candidateToCovered.TryGetValue(candidate, out var covered))
                    {
                        covered = new BitArray(entities.Count);
                        covered[i] = true;
                        candidateToCovered.Add(candidate, covered);
                    }
                    else
                    {
                        covered[i] = true;
                    }
                }
            }
        }

        return candidateToCovered;
    }

    public static Dictionary<Location, HashSet<Location>> GetProviderCenterToCoveredCenters(
        SquareGrid grid,
        int providerWidth,
        int providerHeight,
        int supplyWidth,
        int supplyHeight,
        IEnumerable<Location> providerCenters)
    {
        var poleCenterToCoveredCenters = new Dictionary<Location, HashSet<Location>>();

        const int minPoweredEntityWidth = 3;
        const int minPoweredEntityHeight = 3;

        foreach (var center in providerCenters)
        {
            var coveredCenters = new HashSet<Location>();

            var minX = Math.Max(minPoweredEntityWidth - 1, center.X - ((providerWidth - 1) / 2) - (supplyWidth / 2));
            var minY = Math.Max(minPoweredEntityHeight - 1, center.Y - ((providerHeight - 1) / 2) - (supplyHeight / 2));
            var maxX = Math.Min(grid.Width - minPoweredEntityWidth + 1, center.X + (providerWidth / 2) + ((supplyWidth - 1) / 2));
            var maxY = Math.Min(grid.Height - minPoweredEntityHeight + 1, center.Y + (providerHeight / 2) + ((supplyHeight - 1) / 2));

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var location = new Location(x, y);

                    var entity = grid[location];
                    switch (entity)
                    {
                        case PumpjackCenter:
                            coveredCenters.Add(location);
                            break;
                        case PumpjackSide pumpjackSide:
                            coveredCenters.Add(grid.EntityToLocation[pumpjackSide.Center]);
                            break;
                        case BeaconCenter:
                            coveredCenters.Add(location);
                            break;
                        case BeaconSide beaconSide:
                            coveredCenters.Add(grid.EntityToLocation[beaconSide.Center]);
                            break;
                    }
                }
            }

            poleCenterToCoveredCenters.Add(center, coveredCenters);
        }

        return poleCenterToCoveredCenters;
    }

    /// <summary>
    /// Checks if the provider fits at the provided center location. This does NOT account for grid bounds.
    /// </summary>
    public static bool DoesProviderFit(SquareGrid grid, int providerWidth, int providerHeight, Location center)
    {
        var fits = true;

        (var offsetX, var offsetY) = GetProviderCenterOffset(providerWidth, providerHeight);
        for (var w = 0; w < providerWidth && fits; w++)
        {
            for (var h = 0; h < providerHeight && fits; h++)
            {
                var location = center.Translate(offsetX + w, offsetY + h);
                fits = grid.IsEmpty(location);
            }
        }

        return fits;
    }

    public static bool IsProviderInBounds(SquareGrid grid, int providerWidth, int providerHeight, Location center)
    {
        return center.X - ((providerWidth - 1) / 2) > 0
            && center.Y - ((providerHeight - 1) / 2) > 0
            && center.X + (providerWidth / 2) < grid.Width
            && center.Y + (providerHeight / 2) < grid.Height;
    }

    public static (int OffsetX, int OffsetY) GetProviderCenterOffset(int providerWidth, int providerHeight)
    {
        var offsetX = (providerWidth - 1) / 2 * -1;
        var offsetY = (providerHeight - 1) / 2 * -1;
        return (offsetX, offsetY);
    }

    public static void EliminateOtherTerminals(Context context, TerminalLocation selectedTerminal)
    {
        var terminalOptions = context.CenterToTerminals[selectedTerminal.Center];

        if (terminalOptions.Count == 1)
        {
            return;
        }

        for (var i = 0; i < terminalOptions.Count; i++)
        {
            var otherTerminal = terminalOptions[i];
            if (otherTerminal == selectedTerminal)
            {
                continue;
            }

            var terminals = context.LocationToTerminals[otherTerminal.Terminal];

            if (terminals.Count == 1)
            {
                context.LocationToTerminals.Remove(otherTerminal.Terminal);
            }
            else
            {
                terminals.Remove(otherTerminal);
            }
        }

        terminalOptions.Clear();
        terminalOptions.Add(selectedTerminal);
    }

    public static List<Location> GetPath(Dictionary<Location, Location> cameFrom, Location start, Location reachedGoal)
    {
        var sizeEstimate = 2 * start.GetManhattanDistance(reachedGoal);
        var path = new List<Location>(sizeEstimate);
        AddPath(cameFrom, reachedGoal, path);
        return path;
    }

    public static void AddPath(Dictionary<Location, Location> cameFrom, Location reachedGoal, List<Location> outputList)
    {
        var current = reachedGoal;
        while (true)
        {
            var next = cameFrom[current];
            outputList.Add(current);
            if (next == current)
            {
                break;
            }

            current = next;
        }
    }

    public static int CountTrue(this BitArray array)
    {
        var count = 0;
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i])
            {
                count++;
            }
        }

        return count;
    }

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
            (var min, var max) = a.Y < b.Y ? (a.Y, b.Y) : (b.Y, a.Y);
            var line = new List<Location>(max - min + 1);
            for (var y = min; y <= max; y++)
            {
                line.Add(new Location(a.X, y));
            }

            return line;
        }

        if (a.Y == b.Y)
        {
            (var min, var max) = a.X < b.X ? (a.X, b.X) : (b.X, a.X);
            var line = new List<Location>(max - min + 1);
            for (var x = min; x <= max; x++)
            {
                line.Add(new Location(x, a.Y));
            }

            return line;
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


        var points = filteredNodes.Select<Location, DelaunatorSharp.IPoint>(x => new DelaunatorSharp.Point(x.X, x.Y)).ToArray();
        var delaunator = new DelaunatorSharp.Delaunator(points);

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
