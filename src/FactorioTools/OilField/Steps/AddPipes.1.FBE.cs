using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

/// <summary>
/// This "FBE" implementation is copied from Teoxoy's Factorio Blueprint Editor (FBE).
/// Source:
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/pipe.ts
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts
/// 
/// Teoxoy came up with the idea to use Delaunay triangulation for this problem. Awesome!
/// </summary>
public static partial class AddPipes
{
    public static HashSet<Location> ExecuteWithFbe(Context context)
    {
        // HACK: it appears FBE does not adjust the grid middle by the 2 cell buffer added to the side of the grid.
        // We'll apply this hack for now to reproduce FBE results.
        var middle = context.Grid.Middle.Translate(-2, -2);

        (var terminals, var pipes) = DelaunayTriangulation(context, middle);

        foreach (var terminal in terminals)
        {
            EliminateOtherTerminals(context, terminal);
        }

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

        // GENERATE GROUPS
        var groups = new List<Group>();
        var addedPumpjacks = new List<TerminalLocation>();
        while (lines.Count > 0)
        {
            var line = lines
                .MinBy(ent => (
                    !LineContainsAnAddedPumpjack(addedPumpjacks, ent),
                    -(ent.Endpoints.A.GetManhattanDistance(middle) + ent.Endpoints.B.GetManhattanDistance(middle)),
                    ent.Connections.Count,
                    ent.AverageDistance
                ))!;
            lines.Remove(line);

            var addedA = addedPumpjacks.FirstOrDefault(x => x.Center == line.Endpoints.A);
            var addedB = addedPumpjacks.FirstOrDefault(x => x.Center == line.Endpoints.B);

            var sortedConnections = line.Connections
                .OrderBy(x => x.TerminalA.Terminal.GetManhattanDistance(true ? middle : context.Grid.Middle))
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
                    group.Add(connection.TerminalA);
                    group.Paths.Add(connection.Line);
                    addedPumpjacks.Add(connection.TerminalA);
                    break;
                }

                if (addedA is not null && addedB is null && addedA.Direction == connection.TerminalA.Direction)
                {
                    var group = groups.First(g => g.Entities.Contains(addedA));
                    group.Add(connection.TerminalB);
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
                            var result = AStar.GetShortestPath(context.SharedInstances, context.Grid, p.A.Terminal, new HashSet<Location> { p.B.Terminal });
                            return new TerminalPair(p.A, p.B, result.Path);
                        })
                        .MinBy(x => x.Line.Count)!;
                })
                .MinBy(x => x.Line.Count)!;

            groups.Add(new Group(
                new List<TerminalLocation> { connection.TerminalA, connection.TerminalB },
                new List<List<Location>> { connection.Line }));
        }

        // CONNECT GROUPS
        const int maxTries = 10;
        var tries = maxTries;
        var aloneGroups = new List<Group>();
        Group? finalGroup = null;

        while (groups.Count > 0)
        {
            var group = groups.MinBy(x => x.Paths.Sum(p => p.Count))!;
            groups.Remove(group);

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

            var locationToGroup = groups.ToDictionary(x => x.Location, x => x);
            locationToGroup.Add(group.Location, group);

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
                connection.FirstGroup.AddRange(group.Entities);
                connection.FirstGroup.Paths.AddRange(group.Paths);
                connection.FirstGroup.Paths.Add(connection.Lines[0]);
            }
            else
            {
                aloneGroups.Add(group);
            }

            // VisualizeGroups(context, addedPumpjacks, new[] { group });
        }

        if (finalGroup is null)
        {
            throw new FactorioToolsException("The final group should be initialized at this point.");
        }

        if (aloneGroups.Count > 0)
        {
            throw new FactorioToolsException("There should be no more alone groups at this point.");
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

            var maxTurns = 2;
            while (true)
            {
                var connection = GetPathBetweenGroups(
                    context,
                    terminalGroups,
                    finalGroup,
                    maxTurns);

                if (connection is null)
                {
                    // VisualizeGroups(context, addedPumpjacks, new[] { finalGroup });

                    if (maxTurns > 4)
                    {
                        throw new FactorioToolsException("There should be at least one connection between a leftover pumpjack and the final group. Max turns: " + maxTurns);
                    }

                    maxTurns++;
                    continue;
                }

                finalGroup.Add(connection.FirstGroup.Entities.Single());
                finalGroup.Paths.Add(connection.Lines[0]);
                break;
            }

        }

        var terminals = finalGroup.Entities.ToList();
        var pipes = finalGroup.Paths.SelectMany(l => l).ToHashSet();

        return (terminals, pipes);
    }

#if DEBUG
    private static void VisualizeGroups(Context context, List<TerminalLocation> addedPumpjacks, IEnumerable<Group> groups)
    {
        var clone = new PipeGrid(context.Grid);
        AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, groups.SelectMany(x => x.Paths.SelectMany(l => l)).ToHashSet(), undergroundPipes: null, allowMultipleTerminals: true);
        Visualizer.Show(clone, addedPumpjacks.Select(x => (IPoint)new Point(x.Center.X, x.Center.Y)), Array.Empty<IEdge>());
    }
#endif

    private static bool LineContainsAnAddedPumpjack(List<TerminalLocation> addedPumpjacks, PumpjackConnection ent)
    {
        return addedPumpjacks.Any(e =>
            (ent.Endpoints.A == e.Center || ent.Endpoints.B == e.Center)
            && ent.Connections.Any(t => t.TerminalA.Direction == e.Direction || t.TerminalB.Direction == e.Direction));
    }

    private static TwoConnectedGroups? GetPathBetweenGroups(Context context, List<Group> groups, Group group, int maxTurns)
    {
        return groups
            .Select(g => ConnectTwoGroups(context, g, group, maxTurns))
            .Where(g => g.Lines.Count > 0)
            .MinBy(g => g.MinDistance);
    }

    private static TwoConnectedGroups ConnectTwoGroups(Context context, Group a, Group b, int maxTurns)
    {
        var aLocations = a.Paths.SelectMany(x => x).ToList();
        var bLocations = b.Paths.SelectMany(x => x).ToList();

        var lines = aLocations
            .SelectMany(al => bLocations.Where(bl => al.X == bl.X || al.Y == bl.Y).Select(bl => KeyValuePair.Create(al, bl)))
            .Select(p => new PathAndTurns(new Endpoints(p.Key, p.Value), MakeStraightLine(p.Key, p.Value), Turns: 0))
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
                var result = AStar.GetShortestPath(context.SharedInstances, context.Grid, l.A, new HashSet<Location> { l.B });

                if (result.ReachedGoal is null)
                {
                    // Visualizer.Show(context.Grid, new[] { l.A, l.B }.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                    throw new NoPathBetweenTerminalsException(l.A, l.B);
                }

                var path = result.Path;

                /*
                var path = BreadthFirstFinder.GetShortestPath(context.SharedInstances, context.Grid, l.B, l.A);
                if (path is null)
                {
                    // Visualizer.Show(context.Grid, new[] { l.A, l.B }.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                    throw new NoPathBetweenTerminalsException();
                }
                */

                var turns = CountTurns(path);
                return new PathAndTurns(l, path, turns);
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

    private class Group
    {
        private readonly List<TerminalLocation> _entities;
        private double _sumX = 0;
        private double _sumY = 0;

        public Group(List<TerminalLocation> entities, List<List<Location>> paths)
        {
            _entities = entities;
            UpdateLocation(0);
            Paths = paths;
        }

        public IReadOnlyList<TerminalLocation> Entities => _entities;
        public List<List<Location>> Paths { get; }
        public Location Location { get; private set; }

        public void Add(TerminalLocation entity)
        {
            _entities.Add(entity);
            UpdateLocation(_entities.Count - 1);
        }

        public void AddRange(IEnumerable<TerminalLocation> entities)
        {
            var countBefore = _entities.Count;
            _entities.AddRange(entities);
            UpdateLocation(countBefore);
        }

        private void UpdateLocation(int countBefore)
        {
            for (var i = countBefore; i < _entities.Count; i++)
            {
                _sumX += _entities[i].Center.X;
                _sumY += _entities[i].Center.Y;
            }

            Location = new Location(
                (int)Math.Round(_sumX / _entities.Count, 0),
                (int)Math.Round(_sumY / _entities.Count, 0));
        }
    }

    private record PumpjackConnection(Endpoints Endpoints, List<TerminalPair> Connections)
    {
        public double AverageDistance => Connections.Count > 0 ? Connections.Average(x => x.Line.Count - 1) : 0;
    }

    private record TerminalPair(TerminalLocation TerminalA, TerminalLocation TerminalB, List<Location> Line)
    {
#if ENABLE_GRID_TOSTRING
        public override string ToString()
        {
            return $"{TerminalA.Terminal} -> {TerminalB.Terminal} (length {Line.Count})";
        }
#endif
    }
}
