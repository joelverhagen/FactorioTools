using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using DelaunatorSharp;
using PumpjackPipeOptimizer.Algorithms;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

/// <summary>
/// This is a clone of Teoxoy's implementation for Factorio Blueprint Editor.
/// https://github.com/teoxoy/factorio-blueprint-editor/blob/master/packages/editor/src/core/generators/pipe.ts
/// </summary>
internal static class AddPipes
{
    public static HashSet<Location> Execute(Context context)
    {
        // HACK: it appears FBE does not adjust the grid middle by the 2 cell buffer added to the side of the grid.
        // We'll apply this hack for now to reproduce FBE results.
        // var middle = context.Grid.Middle.Translate((-2, -2)); // 143
        // var middle = context.Grid.Middle; // 146
        // var middle = new Location(0, 0); // 142
        // var middle = new Location(0, context.Grid.Height - 1); // 146
        // var middle = new Location(context.Grid.Width - 1, 0); // 142
        // var middle = new Location(context.Grid.Width - 1, context.Grid.Height - 1); // 145

        // var middle = new Location(45, 10); // optimal
        
        var allLocations = Enumerable
            .Range(0, context.Grid.Height)
            .SelectMany(y => Enumerable.Range(0, context.Grid.Width).Select(x => new Location(x, y)));

        /*
        var metrics = allLocations
            .ToDictionary(
                x => x,
                p => new
                {
                    CenterManhattanDistance = context.CenterToTerminals.Keys.Sum(l => l.GetManhattanDistance(p)),
                    TerminalManhattanDistance = context.CenterToTerminals.Values.SelectMany(t => t).Sum(l => l.Terminal.GetManhattanDistance(p)),
                    CenterEuclideanDistance = context.CenterToTerminals.Keys.Sum(l => l.GetEuclideanDistance(p)),
                    TerminalEuclideanDistance = context.CenterToTerminals.Values.SelectMany(t => t).Sum(l => l.Terminal.GetEuclideanDistance(p)),
                    SumTerminalAligned = context.CenterToTerminals.Values.SelectMany(l => l).Count(l => l.Terminal.X == p.X || l.Terminal.Y == p.Y),
                    AnyTerminalAligned = context.CenterToTerminals.Values.Count(c => c.Any(l => l.Terminal.X == p.X || l.Terminal.Y == p.Y)),
                });

        Console.WriteLine();
        for (var y = 0; y < context.Grid.Height; y++)
        {
            for (var x = 0; x < context.Grid.Width; x++)
            {
                if (x > 0)
                {
                    Console.Write(",");
                }

                Console.Write(metrics[new Location(x, y)].Metric);
            }
            Console.WriteLine();
        }
        */

        /*
        var options = allLocations
            .AsParallel()
            .Select(middle =>
            {
                var output = DelaunayTriangulation(context, middle);
                Console.WriteLine($"Middle: {middle} -> pipes: " + output.Pipes.Count);
                return (output.Terminals, output.Pipes, Middle: middle);
            })
            .OrderBy(x => x.Pipes.Count)
            .ToList();

        var grid = new PipeGrid(context.Grid);
        foreach (var option in options)
        {
            grid.RemoveEntity(option.Middle);
            grid.AddEntity(option.Middle, new StringEntity(option.Pipes.Count.ToString()));
        }

        Console.WriteLine();
        grid.WriteTo(Console.Out, spacing: 1);
        
        (var terminals, var pipes, _) = options.First();
        */

        var middle = context.Grid.Middle; //.Translate((-2, -2));

        (var terminals, var pipes) = DelaunayTriangulation(context, middle);

        foreach (var terminal in terminals)
        {
            var otherTerminals = context.CenterToTerminals[terminal.Center];
            otherTerminals.Clear();
            otherTerminals.Add(terminal);

            if (!context.Grid.IsEntityType<Terminal>(terminal.Terminal))
            {
                context.Grid.AddEntity(terminal.Terminal, new Terminal());
            }
        }

        foreach (var pipe in pipes)
        {
            if (!context.Grid.IsEntityType<Terminal>(pipe))
            {
                context.Grid.AddEntity(pipe, new Pipe());
            }
        }

        // Console.WriteLine();
        // Console.WriteLine($"Pipes: " + pipes.Count);
        // context.Grid.WriteTo(Console.Out);

        return pipes;
    }

    private static (List<TerminalLocation> Terminals, HashSet<Location> Pipes) DelaunayTriangulation(Context context, Location middle)
    {
        var centerDistance = new Dictionary<(Location, Location), int>();
        var centers = context.CenterToTerminals.Keys.ToList();
        for (var i = 0; i < centers.Count; i++)
        {
            for (var j = 0; j < i; j++)
            {
                var a = centers[i];
                var b = centers[j];
                var distance = a.GetManhattanDistance(b);
                centerDistance.Add((a, b), distance);
                centerDistance.Add((b, a), distance);
            }
        }

        var minCenterDistance = centerDistance
            .GroupBy(x => x.Key.Item1)
            .ToDictionary(x => x.Key, x => x.Min(y => y.Value));

        // GENERATE LINES
        var middleMatters = new List<List<TerminalPair>>();

        var lines = PointsToLines(context.CenterToTerminals.Keys)
            .Select(line =>
            {
                var connections = context.CenterToTerminals[line.A]
                    .SelectMany(t => context.CenterToTerminals[line.B].Select(t2 => (A: t, B: t2)))
                    .Where(p => p.A.Terminal.X == p.B.Terminal.X || p.A.Terminal.Y == p.B.Terminal.Y)
                    .Select(p => new TerminalPair(p.A, p.B, MakeStraightLine(p.A.Terminal, p.B.Terminal)))
                    .Where(p => p.Line.All(l => context.Grid.IsEmpty(l)))
                    .ToList();

                return new PumpjackConnection(new Endpoints(line.A, line.B), connections);
            })
            .Where(x => x.Connections.Count > 0)
            .ToList();

        var columnHits = new int[context.Grid.Width];
        var rowHits = new int[context.Grid.Height];
        foreach (var line in lines)
        {
            foreach (var connection in line.Connections)
            {
                columnHits[connection.TerminalA.Terminal.X]++;
                columnHits[connection.TerminalB.Terminal.X]++;
                rowHits[connection.TerminalA.Terminal.Y]++;
                rowHits[connection.TerminalB.Terminal.Y]++;
            }
        }

        var columnWeight = new double[context.Grid.Width];
        var rowWeight = new double[context.Grid.Height];
        foreach (var line in lines)
        {
            foreach (var connection in line.Connections)
            {
                for (var i = 0; i < 4; i++)
                {
                    var weight = Math.Pow(2, -1 * i);

                    if (connection.TerminalA.Terminal.X + i < columnWeight.Length)
                    {
                        columnWeight[connection.TerminalA.Terminal.X + i] += weight;
                    }
                    
                    if (connection.TerminalB.Terminal.X + i < columnWeight.Length)
                    {
                        columnWeight[connection.TerminalB.Terminal.X + i] += weight;
                    }
                    
                    if (connection.TerminalA.Terminal.Y + i < rowWeight.Length)
                    {
                        rowWeight[connection.TerminalA.Terminal.Y + i] += weight;
                    }

                    if (connection.TerminalB.Terminal.Y + i < rowWeight.Length)
                    {
                        rowWeight[connection.TerminalB.Terminal.Y + i] += weight;
                    }

                    if (i != 0)
                    {
                        if (connection.TerminalA.Terminal.X - i >= 0)
                        {
                            columnWeight[connection.TerminalA.Terminal.X - i] += weight;
                        }

                        if (connection.TerminalB.Terminal.X - i >= 0)
                        {
                            columnWeight[connection.TerminalB.Terminal.X - i] += weight;
                        }

                        if (connection.TerminalA.Terminal.Y - i >= 0)
                        {
                            rowWeight[connection.TerminalA.Terminal.Y - i] += weight;
                        }

                        if (connection.TerminalB.Terminal.Y - i >= 0)
                        {
                            rowWeight[connection.TerminalB.Terminal.Y - i] += weight;
                        }

                    }
                }
            }
        }

        /*
        for (var i = 0; i < middleMatters.Count; i++)
        {
            for (var j = 0; j < middleMatters[i].Count; j++)
            {
                var t = middleMatters[i][j];
                context.Grid.RemoveEntity(t.TerminalA.Terminal);
                context.Grid.RemoveEntity(t.TerminalB.Terminal);
                context.Grid.AddEntity(t.TerminalA.Terminal, new StringEntity(j == 0 ? "+" : "-"));
                context.Grid.AddEntity(t.TerminalB.Terminal, new StringEntity(j == 0 ? "+" : "-"));
            }
        }

        Console.WriteLine();
        context.Grid.WriteTo(Console.Out);
        */

        // GENERATE GROUPS
        var groups = new List<Group>();
        var addedPumpjacks = new List<TerminalLocation>();
        while (lines.Count > 0)
        {
            lines = lines
                // .OrderByDescending(ent => LineContainsAnAddedPumpjack(addedPumpjacks, ent))
                .OrderByDescending(x => groups.Count == 0 ? 0 : groups.Min(g => Math.Min(g.Location.GetManhattanDistance(x.Endpoints.A), g.Location.GetManhattanDistance(x.Endpoints.B))))
                .ThenBy(x => Math.Min(x.Endpoints.A.X, x.Endpoints.B.X))
                .ThenBy(x => Math.Min(x.Endpoints.A.Y, x.Endpoints.B.Y))
                .ThenByDescending(ent => ent.Endpoints.A.GetManhattanDistance(true ? middle : context.Grid.Middle) + ent.Endpoints.B.GetManhattanDistance(true ? middle : context.Grid.Middle))
                .ThenBy(ent => ent.Connections.Count)
                .ThenBy(ent => ent.AverageDistance)
                .ToList();

            var line = lines.First();
            lines.RemoveAt(0);

            var addedA = addedPumpjacks.FirstOrDefault(x => x.Center == line.Endpoints.A);
            var addedB = addedPumpjacks.FirstOrDefault(x => x.Center == line.Endpoints.B);

            var sortedConnections = line.Connections
                // .OrderByDescending(x => x.IsHorizontal ? columnWeight[x.TerminalA.Terminal.X] : rowWeight[x.TerminalA.Terminal.Y])
                // .OrderBy(x => Math.Min(x.TerminalA.Terminal.X, x.TerminalB.Terminal.X))
                // .ThenBy(x => Math.Min(x.TerminalA.Terminal.Y, x.TerminalB.Terminal.Y))
                .OrderBy(x => groups.Count == 0 ? 0 : groups.Min(g => Math.Min(g.Location.GetManhattanDistance(x.TerminalA.Terminal), g.Location.GetManhattanDistance(x.TerminalB.Terminal))))
                .ThenBy(x => x.TerminalA.Terminal.GetManhattanDistance(true ? middle : context.Grid.Middle) + x.TerminalB.Terminal.GetManhattanDistance(true ? middle : context.Grid.Middle))
                .ThenBy(x => x.Line.Count)
                .ToList();

            foreach (var connection in sortedConnections)
            {
                if (addedA is null && addedB is null)
                {
                    groups.Add(new Group(
                        new List<TerminalLocation> { connection.TerminalA, connection.TerminalB },
                        new List<List<Location>> { connection.Line }));
                    addedPumpjacks.Add(connection.TerminalA);
                    addedPumpjacks.Add(connection.TerminalB);
                    break;
                }

                if (addedA is null && addedB is not null && addedB.Direction == connection.TerminalB.Direction)
                {
                    var group = groups.First(g => g.Entities.Contains(addedB));
                    group.Entities.Add(connection.TerminalA);
                    group.Paths.Add(connection.Line);
                    addedPumpjacks.Add(connection.TerminalA);
                    break;
                }

                if (addedA is not null && addedB is null && addedA.Direction == connection.TerminalA.Direction)
                {
                    var group = groups.First(g => g.Entities.Contains(addedA));
                    group.Entities.Add(connection.TerminalB);
                    group.Paths.Add(connection.Line);
                    addedPumpjacks.Add(connection.TerminalB);
                    break;
                }
            }
        }

        // if no LINES were generated, add 2 pumpjacks to a group here
        // this will only happen when only a few pumpjacks need to be connected
        if (groups.Count == 0)
        {
            var connection = PointsToLines(context.CenterToTerminals.Keys)
                .Select(line =>
                {
                    return context.CenterToTerminals[line.A]
                        .SelectMany(t => context.CenterToTerminals[line.B].Select(t2 => (A: t, B: t2)))
                        .Select(p =>
                        {
                            var result = Dijkstras.GetShortestPaths(context.Grid, p.A.Terminal, new HashSet<Location> { p.B.Terminal }, stopOnFirstGoal: true);
                            var path = result.GetStraightPaths(p.B.Terminal).First();
                            return new TerminalPair(p.A, p.B, path);
                        })
                        .OrderBy(x => x.Line.Count)
                        .First();
                })
                .OrderBy(x => x.Line.Count)
                .First();

            groups.Add(new Group(
                new List<TerminalLocation> { connection.TerminalA, connection.TerminalB },
                new List<List<Location>> { connection.Line }));
        }

        /*
        foreach (var group in groups)
        {
            foreach (var entity in group.Entities)
            {
                context.Grid.RemoveEntity(entity.Terminal);
                context.Grid.AddEntity(entity.Terminal, new Terminal());
            }

            foreach (var path in group.Paths)
            {
                foreach (var pipe in path)
                {
                    if (context.Grid.IsEmpty(pipe))
                    {
                        context.Grid.AddEntity(pipe, new Pipe());
                    }
                }
            }
        }

        Console.WriteLine();
        context.Grid.WriteTo(Console.Out);
        */

        // CONNECT GROUPS
        var maxTries = 3;
        var tries = 3;
        var aloneGroups = new List<Group>();
        Group? finalGroup = null;

        while (groups.Count > 0)
        {
            groups = groups
                .OrderBy(x => x.Paths.Sum(p => p.Count))
                .ToList();

            var groupsCopy = groups.ToList();

            var group = groups[0];
            groups.RemoveAt(0);

            if (groups.Count == 0)
            {
                if (aloneGroups.Count > 0 && tries > 0)
                {
                    groups.AddRange(aloneGroups);
                    groups.Add(group);
                    aloneGroups.Clear();
                    tries--;
                    continue;
                }

                finalGroup = group;
                break;
            }

            var locationToGroup = groupsCopy.ToDictionary(x => x.Location, x => x);
            var par = PointsToLines(locationToGroup.Keys)
                    .Where(l => l.A == group.Location || l.B == group.Location)
                    .Select(l => l.A == group.Location ? l.B : l.A)
                    .Select(l => locationToGroup[l])
                    .ToList();

            var connection = GetPathBetweenGroups(
                context,
                par,
                group,
                2 + maxTries - tries);

            if (connection is not null)
            {
                connection.FirstGroup.Entities.AddRange(group.Entities);
                connection.FirstGroup.Paths.AddRange(group.Paths);
                connection.FirstGroup.Paths.Add(connection.Lines[0]);
            }
            else
            {
                aloneGroups.Add(group);
            }
        }

        if (finalGroup is null)
        {
            throw new InvalidOperationException("The final group should be initalized at this point.");
        }

        var leftoverPumpjacks = context
            .CenterToTerminals
            .Keys
            .Except(addedPumpjacks.Select(x => x.Center))
            .OrderBy(l => l.GetManhattanDistance(true ? middle : context.Grid.Middle));

        foreach (var center in leftoverPumpjacks)
        {
            var terminalGroups = context
                .CenterToTerminals[center]
                .Select(t => new Group(new List<TerminalLocation> { t }, new List<List<Location>> { new List<Location> { t.Terminal } }))
                .ToList();

            var connection = GetPathBetweenGroups(
                context,
                terminalGroups,
                finalGroup);

            if (connection is null)
            {
                throw new InvalidOperationException("There should be at least one connection between a leftover pumpjack and the final group.");
            }

            finalGroup.Entities.Add(connection.FirstGroup.Entities.Single());
            finalGroup.Paths.Add(connection.Lines[0]);
        }

        var terminals = finalGroup.Entities.ToList();
        var pipes = finalGroup.Paths.SelectMany(l => l).ToHashSet();

        return (terminals, pipes);
    }

    private static List<Endpoints> PointsToLines(IEnumerable<Location> nodes)
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
        double lastSlope = 0;
        for (var i = 0; i < filteredNodes.Count; i++)
        {
            if (i == filteredNodes.Count - 1)
            {
                return Enumerable
                    .Range(1, filteredNodes.Count - 1)
                    .Select(i => new Endpoints(filteredNodes[i - 1], filteredNodes[i]))
                    .ToList();
            }

            var node = filteredNodes[i];
            var next = filteredNodes[i + 1];
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

    private static List<Location> MakeStraightLine(Location a, Location b)
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

    private static bool LineContainsAnAddedPumpjack(List<TerminalLocation> addedPumpjacks, PumpjackConnection ent)
    {
        return addedPumpjacks.Any(e =>
            (ent.Endpoints.A == e.Center || ent.Endpoints.B == e.Center)
            && ent.Connections.Any(t => t.TerminalA.Direction == e.Direction || t.TerminalB.Direction == e.Direction));
    }

    private static TwoConnectedGroups? GetPathBetweenGroups(Context context, List<Group> groups, Group group, int maxTurns = 2)
    {
        return groups
            .Select(g => ConnectTwoGroups(context, g, group, maxTurns))
            .Where(g => g.Lines.Count > 0)
            .OrderBy(g => g.MinDistance)
            .FirstOrDefault();
    }

    private static int CountTurns(List<Location> path)
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

    private static TwoConnectedGroups ConnectTwoGroups(Context context, Group a, Group b, int maxTurns = 2)
    {
        var aLocations = a.Paths.SelectMany(x => x).ToList();
        var bLocations = b.Paths.SelectMany(x => x).ToList();

        var lines = aLocations
            .SelectMany(al => bLocations.Where(bl => al.X == bl.X || al.Y == bl.Y).Select(bl => (A: al, B: bl)))
            .Select(p => new PathAndTurns(new Endpoints(p.A, p.B), MakeStraightLine(p.A, p.B), Turns: 0))
            .Where(l => l.Path.All(x => context.Grid.IsEmpty(x)))
            .ToList();

        List<Location> bLocationsOptimized;
        if (aLocations.Count == 1)
        {
            bLocationsOptimized = bLocations.OrderBy(b => aLocations[0].GetManhattanDistance(b)).Take(20).ToList();
        }
        else
        {
            bLocationsOptimized = bLocations;
        }

        lines.AddRange(PointsToLines(aLocations.Concat(bLocationsOptimized))
            .Where(p => !((aLocations.Contains(p.A) && aLocations.Contains(p.B)) || (bLocations.Contains(p.A) && bLocations.Contains(p.B))))
            .OrderBy(p => p.A.GetManhattanDistance(p.B))
            .Take(5)
            .Select(l =>
            {
                var result = Dijkstras.GetShortestPaths(context.Grid, l.A, new HashSet<Location> { l.B }, stopOnFirstGoal: true);
                var pathAndTurns = result
                    .GetStraightPaths(l.B)
                    .Select(p => new { Path = p, Turns = CountTurns(p) })
                    .OrderBy(x => x.Turns)
                    .First();
                return new PathAndTurns(l, pathAndTurns.Path, pathAndTurns.Turns);
            })
            .Where(l => l.Turns > 0 && l.Turns <= maxTurns)
            .ToList());

        return new TwoConnectedGroups(
            lines.OrderBy(x => x.Path.Count).Select(x => x.Path).ToList(),
            lines.Count > 0 ? lines.Min(x => x.Path.Count) : 0,
            a);
    }

    private record TwoConnectedGroups(List<List<Location>> Lines, int MinDistance, Group FirstGroup);

    private record PathAndTurns(Endpoints Endpoints, List<Location> Path, int Turns);

    private record Group(List<TerminalLocation> Entities, List<List<Location>> Paths)
    {
        public Location Location => new Location(
            (int)Math.Round(Entities.Average(e => e.Center.X), 0),
            (int)Math.Round(Entities.Average(e => e.Center.Y), 0));
    }

    private record PumpjackConnection(Endpoints Endpoints, List<TerminalPair> Connections)
    {
        public double AverageDistance => Connections.Count > 0 ? Connections.Average(x => x.Line.Count - 1) : 0;
    }
    private record Endpoints(Location A, Location B);

    private record TerminalPair(TerminalLocation TerminalA, TerminalLocation TerminalB, List<Location> Line)
    {
        public override string ToString()
        {
            return $"{TerminalA.Terminal} -> {TerminalB.Terminal} (length {Line.Count})";
        }

        public bool IsHorizontal => TerminalA.Terminal.X == TerminalB.Terminal.X;
    }
}
