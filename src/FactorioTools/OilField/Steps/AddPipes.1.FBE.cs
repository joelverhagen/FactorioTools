using System;
using System.Collections.Generic;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

/// <summary>
/// This "FBE" implementation is copied from Teoxoy's Factorio Blueprint Editor (FBE).
/// Source:
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/pipe.ts
/// - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts
/// 
/// Teoxoy came up with the idea to use Delaunay triangulation for this problem. Awesome!
/// </summary>
public static class AddPipesFbe
{
    public record FbeResult(ILocationSet Pipes, PipeStrategy FinalStrategy);

    public static Result<FbeResult> Execute(Context context, PipeStrategy strategy)
    {
        // HACK: it appears FBE does not adjust the grid middle by the 2 cell buffer added to the side of the grid.
        // We'll apply this hack for now to reproduce FBE results.
        var middle = context.Grid.Middle.Translate(-2, -2);

        var result = DelaunayTriangulation(context, middle, strategy);
        if (result.Exception is not null)
        {
            return Result.NewException<FbeResult>(result.Exception);
        }

        (var terminals, var pipes, var finalStrategy) = result.Data!;

        for (var i = 0; i < terminals.Count; i++)
        {
            EliminateOtherTerminals(context, terminals[i]);
        }

        return Result.NewData(new FbeResult(pipes, finalStrategy));
    }

    private record FbeResultInfo(IReadOnlyTableArray<TerminalLocation> Terminals, ILocationSet Pipes, PipeStrategy FinalStrategy);

    private static Result<FbeResultInfo> DelaunayTriangulation(Context context, Location middle, PipeStrategy strategy)
    {
        // GENERATE LINES
        var lines = TableArray.New<PumpjackConnection>();
        var allLines = PointsToLines(context.Centers, sort: false);
        for (var i = 0; i < allLines.Count; i++)
        {
            var line = allLines[i];
            var connections = TableArray.New<TerminalPair>();

            var terminalsA = context.CenterToTerminals[line.A];
            for (var j = 0; j < terminalsA.Count; j++)
            {
                var tA = terminalsA[j];
                var terminalsB = context.CenterToTerminals[line.B];
                for (var k = 0; k < terminalsB.Count; k++)
                {
                    var tB = terminalsB[k];
                    if (tA.Terminal.X != tB.Terminal.X && tA.Terminal.Y != tB.Terminal.Y)
                    {
                        continue;
                    }

                    var straightLine = MakeStraightLineOnEmpty(context.Grid, tA.Terminal, tB.Terminal);
                    if (straightLine is null)
                    {
                        continue;
                    }

                    connections.Add(new TerminalPair(tA, tB, straightLine, middle));
                }
            }

            if (connections.Count == 0)
            {
                continue;
            }

            lines.Add(new PumpjackConnection(new Endpoints(line.A, line.B), connections, middle));
        }

        // GENERATE GROUPS
        var groups = TableArray.New<Group>();
        var addedPumpjacks = TableArray.New<TerminalLocation>();
        var leftoverPumpjacks = context.Centers.EnumerateItems().ToSet(context, allowEnumerate: true);
        while (lines.Count > 0)
        {
            var line = GetNextLine(lines, addedPumpjacks);

            var addedA = addedPumpjacks.EnumerateItems().FirstOrDefault(x => x.Center == line.Endpoints.A);
            var addedB = addedPumpjacks.EnumerateItems().FirstOrDefault(x => x.Center == line.Endpoints.B);

            line.Connections.Sort((a, b) =>
            {
                var c = a.CenterDistance.CompareTo(b.CenterDistance);
                if (c != 0)
                {
                    return c;
                }

                return a.Line.Count.CompareTo(b.Line.Count);
            });

            for (var i = 0; i < line.Connections.Count; i++)
            {
                var connection = line.Connections[i];
                if (addedA is null && addedB is null)
                {
                    var group = new Group(context, connection, TableArray.New(connection.Line));
                    groups.Add(group);
                    addedPumpjacks.Add(connection.TerminalA);
                    addedPumpjacks.Add(connection.TerminalB);
                    leftoverPumpjacks.Remove(connection.TerminalA.Center);
                    leftoverPumpjacks.Remove(connection.TerminalB.Center);
                    break;
                }

                if (addedA is null && addedB is not null && addedB.Direction == connection.TerminalB.Direction)
                {
                    var group = groups.EnumerateItems().First(g => g.HasTerminal(addedB));
                    group.Add(connection.TerminalA);
                    group.Paths.Add(connection.Line);
                    addedPumpjacks.Add(connection.TerminalA);
                    leftoverPumpjacks.Remove(connection.TerminalA.Center);
                    break;
                }

                if (addedA is not null && addedB is null && addedA.Direction == connection.TerminalA.Direction)
                {
                    var group = groups.EnumerateItems().First(g => g.HasTerminal(addedA));
                    group.Add(connection.TerminalB);
                    group.Paths.Add(connection.Line);
                    addedPumpjacks.Add(connection.TerminalB);
                    leftoverPumpjacks.Remove(connection.TerminalB.Center);
                    break;
                }
            }
        }

        // if no LINES were generated, add 2 pumpjacks to a group here
        // this will only happen when only a few pumpjacks need to be connected
        if (groups.Count == 0)
        {
            TerminalPair? connection = null;
            for (var i = 0; i < allLines.Count; i++)
            {
                var line = allLines[i];
                var terminalsA = context.CenterToTerminals[line.A];
                for (var j = 0; j < terminalsA.Count; j++)
                {
                    var tA = terminalsA[j];
                    var terminalsB = context.CenterToTerminals[line.B];
                    for (var k = 0; k < terminalsB.Count; k++)
                    {
                        var tB = terminalsB[k];
                        var goals = context.GetSingleLocationSet(tB.Terminal);

                        if (connection is not null)
                        {
                            // don't perform a shortest path search if the Manhattan distance (minimum possible) is longer than the best.
                            var minDistance = tA.Terminal.GetManhattanDistance(tB.Terminal);
                            if (minDistance >= connection.Line.Count)
                            {
                                continue;
                            }
                        }

                        var result = AStar.GetShortestPath(context, context.Grid, tA.Terminal, goals);

                        if (!result.Success)
                        {
                            throw new FactorioToolsException("A goal should have been reached.");
                        }

                        if (connection is null)
                        {
                            connection = new TerminalPair(tA, tB, result.Path, middle);
                            continue;
                        }

                        var c = result.Path.Count.CompareTo(connection.Line.Count);
                        if (c < 0)
                        {
                            connection = new TerminalPair(tA, tB, result.Path, middle);
                        }
                    }
                }
            }

            if (connection is null)
            {
                throw new FactorioToolsException("A connection between terminals should have been found.");
            }

            var group = new Group(context, connection, TableArray.New(connection.Line));
            groups.Add(group);
        }

        // CONNECT GROUPS
        var maxTries = strategy == PipeStrategy.FbeOriginal ? 3 : 20;
        var tries = maxTries;
        var aloneGroups = TableArray.New<Group>();
        Group? finalGroup = null;

        while (groups.Count > 0)
        {
            var group = groups.EnumerateItems().MinBy(x => x.Paths.EnumerateItems().Sum(p => p.Count))!;
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

            var locationToGroup = groups.ToDictionary(context, x => x.Location, x => x);
            locationToGroup.Add(group.Location, group);

            var groupLines = PointsToLines(locationToGroup.Keys);
            var par = TableArray.New<Group>(groupLines.Count);
            for (var i = 0; i < groupLines.Count; i++)
            {
                var line = groupLines[i];
                if (line.A == group.Location)
                {
                    par.Add(locationToGroup[line.B]);
                }
                else if (line.B == group.Location)
                {
                    par.Add(locationToGroup[line.A]);
                }
            }

            var result = GetPathBetweenGroups(
                context,
                par,
                group,
                2 + maxTries - tries,
                strategy);
            if (result.Exception is not null)
            {
                return Result.NewException<FbeResultInfo>(result.Exception);
            }

            var connection = result.Data!;

            if (connection is not null)
            {
                connection.FirstGroup.AddRange(group);
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
            throw new FactorioToolsException("The final group should be initialized at this point.");
        }

        if (aloneGroups.Count > 0)
        {
            // Fallback to the modified FBE algorithm if the original cannot connect all of the groups.
            // Related to https://github.com/teoxoy/factorio-blueprint-editor/issues/254
            if (strategy == PipeStrategy.FbeOriginal)
            {
                return DelaunayTriangulation(context, middle, PipeStrategy.Fbe);
            }

            throw new FactorioToolsException("There should be no more alone groups at this point.");
        }

#if USE_STACKALLOC && LOCATION_AS_STRUCT
        var sortedLeftoverPumpjacks =
            leftoverPumpjacks.Count < 256
            ? stackalloc Location[leftoverPumpjacks.Count]
            : new Location[leftoverPumpjacks.Count];
        leftoverPumpjacks.CopyTo(sortedLeftoverPumpjacks);
        MemoryExtensions.Sort(sortedLeftoverPumpjacks, (a, b) =>
        {
            var aC = a.GetManhattanDistance(middle);
            var bC = b.GetManhattanDistance(middle);
            return aC.CompareTo(bC);
        });
#else
        var sortedLeftoverPumpjacks = new Location[leftoverPumpjacks.Count];
        leftoverPumpjacks.CopyTo(sortedLeftoverPumpjacks);
        Array.Sort(sortedLeftoverPumpjacks, (a, b) =>
        {
            var aC = a.GetManhattanDistance(middle);
            var bC = b.GetManhattanDistance(middle);
            return aC.CompareTo(bC);
        });
#endif

        for (var i = 0; i < sortedLeftoverPumpjacks.Length; i++)
        {
            var center = sortedLeftoverPumpjacks[i];
            var centerTerminals = context.CenterToTerminals[center];
            var terminalGroups = TableArray.New<Group>(centerTerminals.Count);
            for (var j = 0; j < centerTerminals.Count; j++)
            {
                var terminal = centerTerminals[j];
                var group = new Group(context, terminal, TableArray.New(TableArray.New(terminal.Terminal)));
                terminalGroups.Add(group);
            }

            var maxTurns = 2;
            while (true)
            {
                var result = GetPathBetweenGroups(
                    context,
                    terminalGroups,
                    finalGroup,
                    maxTurns,
                    strategy);
                if (result.Exception is not null)
                {
                    return Result.NewException<FbeResultInfo>(result.Exception);
                }

                var connection = result.Data!;

                if (connection is null)
                {
                    // Allow more max turns with the modified FBE algorithm.
                    // Related to https://github.com/teoxoy/factorio-blueprint-editor/issues/253
                    if (strategy == PipeStrategy.FbeOriginal)
                    {
                        return DelaunayTriangulation(context, middle, PipeStrategy.Fbe);
                    }
                    else if (strategy == PipeStrategy.FbeOriginal || maxTurns > maxTries)
                    {
                        throw new FactorioToolsException("There should be at least one connection between a leftover pumpjack and the final group. Max turns: " + maxTurns);
                    }

                    maxTurns++;
                    continue;
                }

                if (connection.FirstGroup.Entities.Count != 1)
                {
                    throw new FactorioToolsException("There should be a single entity in the group.");
                }

                finalGroup.Add(connection.FirstGroup.Entities[0]);
                finalGroup.Paths.Add(connection.Lines[0]);
                break;
            }
        }

        var terminals = finalGroup.Entities;
        var pipes = context.GetLocationSet(allowEnumerate: true);
        for (var i = 0; i < finalGroup.Paths.Count; i++)
        {
            var path = finalGroup.Paths[i];
            for (var j = 0; j < path.Count; j++)
            {
                pipes.Add(path[j]);
            }
        }

        return Result.NewData(new FbeResultInfo(terminals, pipes, strategy));
    }

    private static PumpjackConnection GetNextLine(ITableArray<PumpjackConnection> lines, ITableArray<TerminalLocation> addedPumpjacks)
    {
        PumpjackConnection? next = null;
        int nextIndex = 0;
        bool? nextContainsAddedPumpjack = default;
        double? nextAverageDistance = default;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (next is null)
            {
                next = line;          
                continue;
            }

            if (!nextContainsAddedPumpjack.HasValue)
            {
                nextContainsAddedPumpjack = LineContainsAnAddedPumpjack(addedPumpjacks, next);
            }

            var containsAddedPumpjack = LineContainsAnAddedPumpjack(addedPumpjacks, line);
            var c = containsAddedPumpjack.CompareTo(nextContainsAddedPumpjack.Value);
            if (c > 0)
            {
                next = line;
                nextIndex = i;
                nextContainsAddedPumpjack = containsAddedPumpjack;
                nextAverageDistance = default;
                continue;
            }
            else if (c < 0)
            {
                continue;
            }

            c = line.EndpointDistance.CompareTo(next.EndpointDistance);
            if (c > 0)
            {
                next = line;
                nextIndex = i;
                nextContainsAddedPumpjack = containsAddedPumpjack;
                nextAverageDistance = default;
                continue;
            }
            else if (c < 0)
            {
                continue;
            }

            c = line.Connections.Count.CompareTo(next.Connections.Count);
            if (c < 0)
            {
                next = line;
                nextIndex = i;
                nextContainsAddedPumpjack = containsAddedPumpjack;
                nextAverageDistance = null;
                continue;
            }
            else if (c > 0)
            {
                continue;
            }

            if (!nextAverageDistance.HasValue)
            {
                nextAverageDistance = next.GetAverageDistance();
            }

            var averageDistance = line.GetAverageDistance();
            c = averageDistance.CompareTo(nextAverageDistance);
            if (c < 0)
            {
                next = line;
                nextIndex = i;
                nextContainsAddedPumpjack = containsAddedPumpjack;
                nextAverageDistance = averageDistance;
            }
        }

        lines.RemoveAt(nextIndex);

        return next!;
    }

    private static bool LineContainsAnAddedPumpjack(ITableArray<TerminalLocation> addedPumpjacks, PumpjackConnection ent)
    {
        for (var i = 0; i < addedPumpjacks.Count; i++)
        {
            var terminal = addedPumpjacks[i];
            if (ent.Endpoints.A != terminal.Center && ent.Endpoints.B != terminal.Center)
            {
                continue;
            }

            for (var j = 0; j < ent.Connections.Count; j++)
            {
                var pair = ent.Connections[j];
                if (pair.TerminalA.Direction == terminal.Direction || pair.TerminalB.Direction == terminal.Direction)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static Result<TwoConnectedGroups?> GetPathBetweenGroups(Context context, ITableArray<Group> groups, Group group, int maxTurns, PipeStrategy strategy)
    {
        TwoConnectedGroups? best = null;
        for (var i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            var result = ConnectTwoGroups(context, g, group, maxTurns, strategy);
            if (result.Exception != null)
            {
                return result!;
            }

            var connection = result.Data!;

            if (connection.Lines.Count == 0)
            {
                continue;
            }

            if (best is null)
            {
                best = connection;
                continue;
            }

            var c = connection.MinDistance.CompareTo(best.MinDistance);
            if (c < 0)
            {
                best = connection;
            }
        }

        return Result.NewData(best);
    }

    private static Result<TwoConnectedGroups> ConnectTwoGroups(Context context, Group a, Group b, int maxTurns, PipeStrategy strategy)
    {
        var aLocations = TableArray.New<Location>();
        for (var i = 0; i < a.Paths.Count; i++)
        {
            aLocations.AddRange(a.Paths[i]);
        }

        var bLocations = TableArray.New<Location>();
        for (var i = 0; i < b.Paths.Count; i++)
        {
            bLocations.AddRange(b.Paths[i]);
        }

        var lineInfo = TableArray.New<PathAndTurns>();
        for (var i = 0; i < aLocations.Count; i++)
        {
            var al = aLocations[i];
            for (var j = 0; j < bLocations.Count; j++)
            {
                var bl = bLocations[j];
                if (al.X != bl.X && al.Y != bl.Y)
                {
                    continue;
                }

                var line = MakeStraightLineOnEmpty(context.Grid, al, bl);
                if (line is null)
                {
                    continue;
                }

                lineInfo.Add(new PathAndTurns(new Endpoints(line[0], line[line.Count - 1]), line, Turns: 0, lineInfo.Count));
            }
        }

        if (aLocations.Count == 1)
        {
            var locationToIndex = context.GetLocationDictionary<int>(bLocations.Count);
            for (var i = 0; i < bLocations.Count; i++)
            {
                locationToIndex.TryAdd(bLocations[i], i);
            }

            bLocations.Sort((a, b) =>
            {
                var aC = aLocations[0].GetManhattanDistance(a);
                var bC = aLocations[0].GetManhattanDistance(b);
                var c = aC.CompareTo(bC);
                if (c != 0)
                {
                    return c;
                }

                return locationToIndex[a].CompareTo(locationToIndex[b]);
            });

            if (bLocations.Count > 20)
            {
                bLocations.RemoveRange(20, bLocations.Count - 20);
            }
        }

        var aPlusB = context.GetLocationSet(aLocations.Count + bLocations.Count, allowEnumerate: true);
        aPlusB.UnionWith(aLocations.EnumerateItems());
        aPlusB.UnionWith(bLocations.EnumerateItems());

        var allEndpoints = PointsToLines(aPlusB.EnumerateItems());
        var matches = TableArray.New<Tuple<Endpoints, int, int>>(allEndpoints.Count);
        for (var i = 0; i < allEndpoints.Count; i++)
        {
            var pair = allEndpoints[i];
            if ((aLocations.Contains(pair.A) && aLocations.Contains(pair.B))
                || (bLocations.Contains(pair.A) && bLocations.Contains(pair.B)))
            {
                continue;
            }

            matches.Add(Tuple.Create(pair, pair.A.GetManhattanDistance(pair.B), matches.Count));
        }

        matches.Sort((a, b) =>
        {
            var c = a.Item2.CompareTo(b.Item2);
            if (c != 0)
            {
                return c;
            }

            return a.Item3.CompareTo(b.Item3);
        });

        var takeLines = Math.Min(matches.Count, 5);
        for (var i = 0; i < takeLines; i++)
        {
            var l = matches[i].Item1;

            ITableArray<Location>? path;
            if (strategy == PipeStrategy.FbeOriginal)
            {
                // We can't terminal early based on max turns because this leads to different results since it allows
                // secondary path options that would have otherwise been not considered for a given start and goal state.
                path = BreadthFirstFinder.GetShortestPath(context, l.B, l.A);
                if (path is null)
                {
                    // Visualizer.Show(context.Grid, new[] { l.A, l.B }.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                    return Result.NewException<TwoConnectedGroups>(new NoPathBetweenTerminalsException(l.A, l.B));
                }
            }
            else
            {
                var goals = context.GetSingleLocationSet(l.B);
                var result = AStar.GetShortestPath(context, context.Grid, l.A, goals);
                if (!result.Success)
                {
                    // Visualizer.Show(context.Grid, new[] { l.A, l.B }.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                    return Result.NewException<TwoConnectedGroups>(new NoPathBetweenTerminalsException(l.A, l.B));
                }

                path = result.Path;
            }

            var turns = CountTurns(path);
            if (turns == 0 || turns > maxTurns)
            {
                continue;
            }

            lineInfo.Add(new PathAndTurns(l, path, turns, lineInfo.Count));
        }

        lineInfo.Sort((a, b) =>
        {
            var c = a.Path.Count.CompareTo(b.Path.Count);
            if (c != 0)
            {
                return c;
            }

            return a.OriginalIndex.CompareTo(b.OriginalIndex);
        });

        var lines = TableArray.New<ITableArray<Location>>(lineInfo.Count);
        var minCount = lineInfo.Count == 0 ? 0 : int.MaxValue;
        for (var i = 0; i < lineInfo.Count; i++)
        {
            var path = lineInfo[i].Path;
            lines.Add(path);
            if (path.Count < minCount)
            {
                minCount = path.Count;
            }
        }

        return Result.NewData(new TwoConnectedGroups(lines, minCount, a));
    }

    private record TwoConnectedGroups(ITableArray<ITableArray<Location>> Lines, int MinDistance, Group FirstGroup);

    private record PathAndTurns(Endpoints Endpoints, ITableArray<Location> Path, int Turns, int OriginalIndex);

    private class Group
    {
        private readonly ILocationSet _terminals;
        private readonly ITableArray<TerminalLocation> _entities;
        private double _sumX = 0;
        private double _sumY = 0;

        public Group(Context context, TerminalLocation terminal, ITableArray<ITableArray<Location>> paths) : this(context, paths)
        {
            Add(terminal);
            UpdateLocation();
        }

        public Group(Context context, TerminalPair pair, ITableArray<ITableArray<Location>> paths) : this(context, paths)
        {
            Add(pair.TerminalA);
            Add(pair.TerminalB);
            UpdateLocation();
        }

        private Group(Context context, ITableArray<ITableArray<Location>> paths) 
        {
            _terminals = context.GetLocationSet();
            _entities = TableArray.New<TerminalLocation>();
            Paths = paths;
        }

        public ITableArray<TerminalLocation> Entities => _entities;
        public ITableArray<ITableArray<Location>> Paths { get; }
        public Location Location { get; private set; } = Location.Invalid;

        public bool HasTerminal(TerminalLocation location)
        {
            return _terminals.Contains(location.Terminal);
        }

        public void Add(TerminalLocation entity)
        {
            _entities.Add(entity);
            _terminals.Add(entity.Terminal);
            _sumX += entity.Center.X;
            _sumY += entity.Center.Y;
            UpdateLocation();
        }

        public void AddRange(Group group)
        {
            _entities.AddRange(group.Entities);
            _terminals.UnionWith(group._terminals);
            for (var i = 0; i < group.Entities.Count; i++)
            {
                var entity = group.Entities[i];
                _sumX += entity.Center.X;
                _sumY += entity.Center.Y;
            }
            UpdateLocation();
        }

        private void UpdateLocation()
        {
            Location = new Location(
                (int)Math.Round(_sumX / _entities.Count, 0),
                (int)Math.Round(_sumY / _entities.Count, 0));
        }
    }

    private class PumpjackConnection
    {
        public PumpjackConnection(Endpoints endpoints, ITableArray<TerminalPair> connections, Location middle)
        {
            Endpoints = endpoints;
            Connections = connections;
            EndpointDistance = endpoints.A.GetManhattanDistance(middle) + endpoints.B.GetManhattanDistance(middle);
        }

        public Endpoints Endpoints { get; }
        public ITableArray<TerminalPair> Connections { get; }
        public int EndpointDistance { get; }

        public double GetAverageDistance()
        {
            return Connections.Count > 0 ? Connections.EnumerateItems().Average(x => x.Line.Count - 1) : 0;
        }
    }

    private class TerminalPair
    {
        public TerminalPair(TerminalLocation terminalA, TerminalLocation terminalB, ITableArray<Location> line, Location middle)
        {
            TerminalA = terminalA;
            TerminalB = terminalB;
            Line = line;
            CenterDistance = terminalA.Terminal.GetManhattanDistance(middle);
        }

        public TerminalLocation TerminalA { get; }
        public TerminalLocation TerminalB { get; }
        public ITableArray<Location> Line { get; }
        public int CenterDistance { get; }

#if ENABLE_GRID_TOSTRING
        public override string ToString()
        {
            return $"{TerminalA.Terminal} -> {TerminalB.Terminal} (length {Line.Count})";
        }
#endif
    }
}
