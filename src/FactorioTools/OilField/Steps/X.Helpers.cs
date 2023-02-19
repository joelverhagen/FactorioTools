using System.Collections;
using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class Helpers
{
    public const int PumpjackWidth = 3;
    public const int PumpjackHeight = 3;

    /// <summary>
    /// . . . + .
    /// . j j j +
    /// . j J j .
    /// + j j j .
    /// . + . . .
    /// </summary>
    public static readonly IReadOnlyList<(Direction Direction, (int DeltaX, int DeltaY))> TerminalOffsets = new List<(Direction Direction, (int DeltaX, int DeltaY))>
    {
        (Direction.Up, (1, -2)),
        (Direction.Right, (2, -1)),
        (Direction.Down, (-1, 2)),
        (Direction.Left, (-2, 1)),
    };

    public static PumpjackCenter AddPumpjack(SquareGrid grid, Location center)
    {
        var centerEntity = new PumpjackCenter();
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                GridEntity entity = x != 0 || y != 0 ? new PumpjackSide(centerEntity) : centerEntity;
                grid.AddEntity(new Location(center.X + x, center.Y + y), entity);
            }
        }

        return centerEntity;
    }

    public static Dictionary<Location, List<TerminalLocation>> GetCenterToTerminals(SquareGrid grid, IEnumerable<Location> centers)
    {
        var centerToTerminals = new Dictionary<Location, List<TerminalLocation>>();
        foreach (var center in centers)
        {
            var candidateTerminals = new List<TerminalLocation>();
            foreach ((var direction, var translation) in TerminalOffsets)
            {
                var location = center.Translate(translation);
                var terminal = new TerminalLocation(center, location, direction);
                var existing = grid[location];
                if (existing is null || existing is Pipe)
                {
                    candidateTerminals.Add(terminal);
                }
            }

            centerToTerminals.Add(center, candidateTerminals);
        }

        return centerToTerminals;
    }

    public static Dictionary<Location, List<TerminalLocation>> GetLocationToTerminals(Dictionary<Location, List<TerminalLocation>> centerToTerminals)
    {
        var locationToTerminals = new Dictionary<Location, List<TerminalLocation>>();
        foreach (var terminals in centerToTerminals.Values)
        {
            foreach (var terminal in terminals)
            {
                if (!locationToTerminals.TryGetValue(terminal.Terminal, out var list))
                {
                    list = new List<TerminalLocation>(2);
                    locationToTerminals.Add(terminal.Terminal, list);
                }

                list.Add(terminal);
            }
        }

        return locationToTerminals;
    }

    public static (Dictionary<Location, BitArray> CandidateToCovered, BitArray CoveredEntities, Dictionary<Location, BeaconCenter> Providers) GetBeaconCandidateToCovered(
        Context context,
        List<ProviderRecipient> recipients,
        bool removeUnused)
    {
        return GetCandidateToCovered<BeaconCenter>(
            context,
            recipients,
            context.Options.BeaconWidth,
            context.Options.BeaconHeight,
            context.Options.BeaconSupplyWidth,
            context.Options.BeaconSupplyHeight,
            removeUnused,
            includePumpjacks: true,
            includeBeacons: false);
    }

    public static (Dictionary<Location, BitArray> CandidateToCovered, BitArray CoveredEntities, Dictionary<Location, ElectricPoleCenter> Providers) GetElectricPoleCandidateToCovered(
        Context context,
        List<ProviderRecipient> recipients,
        bool removeUnused)
    {
        return GetCandidateToCovered<ElectricPoleCenter>(
            context,
            recipients,
            context.Options.ElectricPoleWidth,
            context.Options.ElectricPoleHeight,
            context.Options.ElectricPoleSupplyWidth,
            context.Options.ElectricPoleSupplyHeight,
            removeUnused,
            includePumpjacks: true,
            includeBeacons: true);
    }

    private static (Dictionary<Location, BitArray> CandidateToCovered, BitArray CoveredEntities, Dictionary<Location, TProvider> Providers) GetCandidateToCovered<TProvider>(
        Context context,
        List<ProviderRecipient> recipients,
        int providerWidth,
        int providerHeight,
        int supplyWidth,
        int supplyHeight,
        bool removeUnused,
        bool includePumpjacks,
        bool includeBeacons)
        where TProvider : GridEntity
    {
        var candidateToCovered = new Dictionary<Location, BitArray>();
        var coveredEntities = new BitArray(recipients.Count);

        var providers = context
            .Grid
            .EntityToLocation
            .Select(p => (Location: p.Value, Entity: p.Key as TProvider))
            .Where(p => p.Entity is not null)
            .ToDictionary(p => p.Location, p => p.Entity!);
        var unusedProviders = new HashSet<Location>(providers.Keys);

        for (int i = 0; i < recipients.Count; i++)
        {
            var entity = recipients[i];

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
                    if (context.Grid[candidate] is not null)
                    {
                        if (providers.TryGetValue(candidate, out var existing))
                        {
                            unusedProviders.Remove(candidate);
                            coveredEntities[i] = true;
                        }
                    }
                    else
                    {
                        var fits = DoesProviderFit(context.Grid, providerWidth, providerHeight, candidate);
                        if (!fits)
                        {
                            continue;
                        }

                        if (!candidateToCovered.TryGetValue(candidate, out var covered))
                        {
                            covered = new BitArray(recipients.Count);
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
        }

        if (removeUnused && unusedProviders.Count > 0)
        {
#if USE_SHARED_INSTANCES
            var coveredCenters = context.SharedInstances.LocationSetA;
#else
            var coveredCenters = new HashSet<Location>();
#endif

            try
            {
                foreach (var center in unusedProviders)
                {
                    var entityMinX = center.X - ((providerWidth - 1) / 2);
                    var entityMinY = center.Y - ((providerHeight - 1) / 2);
                    var entityMaxX = center.X + (providerWidth / 2);
                    var entityMaxY = center.Y + (providerHeight / 2);

                    // Expand the loop bounds beyond the entity bounds so we can removed candidates that are not longer valid with
                    // the newly added provider, i.e. they would overlap with what was just added.
                    var minX = Math.Max((providerWidth - 1) / 2, entityMinX - (providerWidth / 2));
                    var minY = Math.Max((providerHeight - 1) / 2, entityMinY - (providerHeight / 2));
                    var maxX = Math.Min(context.Grid.Width - (providerWidth / 2) - 1, entityMaxX + ((providerWidth - 1) / 2));
                    var maxY = Math.Min(context.Grid.Height - (providerHeight / 2) - 1, entityMaxY + ((providerHeight - 1) / 2));

                    for (var x = minX; x <= maxX; x++)
                    {
                        for (var y = minY; y <= maxY; y++)
                        {
                            var candidate = new Location(x, y);

                            if (x >= entityMinX && x <= entityMaxX && y >= entityMinY && y <= entityMaxY)
                            {
                                context.Grid.RemoveEntity(candidate);
                            }
                            else
                            {
                                AddCoveredCenters(
                                    coveredCenters,
                                    context.Grid,
                                    candidate,
                                    providerWidth,
                                    providerHeight,
                                    supplyWidth,
                                    supplyHeight,
                                    includePumpjacks,
                                    includeBeacons);

                                if (!candidateToCovered.TryGetValue(candidate, out var covered))
                                {
                                    covered = new BitArray(recipients.Count);
                                    candidateToCovered.Add(candidate, covered);
                                }

                                for (var i = 0; i < recipients.Count; i++)
                                {
                                    if (coveredCenters.Contains(recipients[i].Center))
                                    {
                                        covered[i] = true;
                                    }
                                }

                                coveredCenters.Clear();
                            }
                        }
                    }

                    providers.Remove(center);
                }
            }
            finally
            {
#if USE_SHARED_INSTANCES
                coveredCenters.Clear();
#endif
            }
        }

        if (providers.Count > 0 || unusedProviders.Count > 0)
        {
            // Remove candidates that only cover recipients that are already covered.
            var toRemove = new List<Location>();
            foreach ((var candidate, var covered) in candidateToCovered)
            {
                var subset = new BitArray(covered);
                subset.Not();
                subset.Or(coveredEntities);
                if (subset.All(true))
                {
                    toRemove.Add(candidate);
                }
            }

            for (var i = 0; i < toRemove.Count; i++)
            {
                candidateToCovered.Remove(toRemove[i]);
            }
        }

        return (candidateToCovered, coveredEntities, providers);
    }

    public static Dictionary<Location, double> GetCandidateToEntityDistance(List<ProviderRecipient> poweredEntities, Dictionary<Location, BitArray> candidateToCovered)
    {
        return candidateToCovered.ToDictionary(
            x => x.Key,
            x =>
            {
                double sum = 0;
                for (var i = 0; i < poweredEntities.Count; i++)
                {
                    if (x.Value[i])
                    {
                        sum += x.Key.GetEuclideanDistance(poweredEntities[i].Center);
                    }
                }

                return sum;
            });
    }

    public static Dictionary<Location, HashSet<Location>> GetProviderCenterToCoveredCenters(
        SquareGrid grid,
        int providerWidth,
        int providerHeight,
        int supplyWidth,
        int supplyHeight,
        IEnumerable<Location> providerCenters,
        bool includePumpjacks,
        bool includeBeacons)
    {
        var poleCenterToCoveredCenters = new Dictionary<Location, HashSet<Location>>();

        foreach (var center in providerCenters)
        {
            var coveredCenters = new HashSet<Location>();
            AddCoveredCenters(
                coveredCenters,
                grid,
                center,
                providerWidth,
                providerHeight,
                supplyWidth,
                supplyHeight,
                includePumpjacks,
                includeBeacons);

            poleCenterToCoveredCenters.Add(center, coveredCenters);
        }

        return poleCenterToCoveredCenters;
    }

    private static void AddCoveredCenters(
        HashSet<Location> coveredCenters,
        SquareGrid grid,
        Location center,
        int providerWidth,
        int providerHeight,
        int supplyWidth,
        int supplyHeight,
        bool includePumpjacks,
        bool includeBeacons)
    {
        const int minPoweredEntityWidth = 3;
        const int minPoweredEntityHeight = 3;

        var minX = Math.Max(minPoweredEntityWidth - 1, center.X - (supplyWidth / 2) + ((providerWidth - 1) % 2));
        var minY = Math.Max(minPoweredEntityHeight - 1, center.Y - (supplyHeight / 2) + ((providerHeight - 1) % 2));
        var maxX = Math.Min(grid.Width - minPoweredEntityWidth + 1, center.X + (supplyWidth / 2));
        var maxY = Math.Min(grid.Height - minPoweredEntityHeight + 1, center.Y + (supplyHeight / 2));

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var location = new Location(x, y);

                var entity = grid[location];
                if (includePumpjacks && entity is PumpjackCenter)
                {
                    coveredCenters.Add(location);
                }
                else if (includePumpjacks && entity is PumpjackSide pumpjackSide)
                {
                    coveredCenters.Add(grid.EntityToLocation[pumpjackSide.Center]);
                }
                else if (includeBeacons && entity is BeaconCenter)
                {
                    coveredCenters.Add(location);
                }
                else if (includeBeacons && entity is BeaconSide beaconSide)
                {
                    coveredCenters.Add(grid.EntityToLocation[beaconSide.Center]);
                }
            }
        }
    }

    /// <summary>
    /// Checks if the provider fits at the provided center location. This does NOT account for grid bounds.
    /// </summary>
    public static bool DoesProviderFit(SquareGrid grid, int providerWidth, int providerHeight, Location center)
    {
        var minX = center.X - ((providerWidth - 1) / 2);
        var maxX = center.X + (providerWidth / 2);
        var minY = center.Y - ((providerHeight - 1) / 2);
        var maxY = center.Y + (providerHeight / 2);

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                if (!grid.IsEmpty(new Location(x, y)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Add a new provider to the grid at the specified location and update the state of
    /// <paramref name="coveredEntities"/>, <paramref name="candidateToCovered"/>, and
    /// <paramref name="candidateToEntityDistance"/> based on the latest state of <paramref name="coveredEntities"/>.
    /// </summary>
    public static void AddProviderAndUpdateCandidateState<TCenter, TSide>(
        SquareGrid grid,
        SharedInstances sharedInstances,
        Location center,
        TCenter centerEntity,
        Func<TCenter, TSide> getNewSide,
        int providerWidth,
        int providerHeight,
        List<ProviderRecipient> recipients,
        BitArray coveredEntities,
        Dictionary<Location, BitArray> candidateToCovered,
        Dictionary<Location, double> candidateToEntityDistance)
        where TCenter : GridEntity
        where TSide : GridEntity
    {
        coveredEntities.Or(candidateToCovered[center]);

        AddProvider(
            grid,
            center,
            centerEntity,
            getNewSide,
            providerWidth,
            providerHeight,
            candidateToCovered);

        if (coveredEntities.All(true))
        {
            return;
        }

#if USE_SHARED_INSTANCES
        var toRemove = sharedInstances.LocationListA;
#else
        var toRemove = new List<Location>();
#endif

        try
        {
            // Remove the covered entities from the candidate data, so that the next candidates are discounted
            // by the entities that no longer need to be covered.
            foreach ((var otherCandidate, var otherCovered) in candidateToCovered)
            {
                var modified = false;
                var otherCoveredCount = otherCovered.CountTrue();
                for (var i = 0; i < recipients.Count && otherCoveredCount > 0; i++)
                {
                    if (coveredEntities[i] && otherCovered[i])
                    {
                        otherCovered[i] = false;
                        modified = true;
                        otherCoveredCount--;
                    }
                }

                if (otherCoveredCount == 0)
                {
                    toRemove.Add(otherCandidate);
                    candidateToEntityDistance.Remove(otherCandidate);
                }
                else if (modified)
                {
                    double entityDistance = 0;
                    for (var i = 0; i < recipients.Count; i++)
                    {
                        entityDistance += otherCandidate.GetEuclideanDistance(recipients[i].Center);
                    }
                    candidateToEntityDistance[otherCandidate] = entityDistance;
                }
            }

            if (toRemove.Count > 0)
            {
                for (var i = 0; i < toRemove.Count; i++)
                {
                    candidateToCovered.Remove(toRemove[i]);
                }

                toRemove.Clear();
            }
        }
        finally
        {
#if USE_SHARED_INSTANCES
            toRemove.Clear();
#endif
        }
    }

    public static (
        int EntityMinX,
        int EntityMinY,
        int EntityMaxX,
        int EntityMaxY,
        int OverlapMinX,
        int OverlapMinY,
        int OverlapMaxX,
        int OverlapMaxY
    ) GetProviderBounds(
        SquareGrid grid,
        Location center,
        int providerWidth,
        int providerHeight)
    {
        var entityMinX = center.X - ((providerWidth - 1) / 2);
        var entityMinY = center.Y - ((providerHeight - 1) / 2);
        var entityMaxX = center.X + (providerWidth / 2);
        var entityMaxY = center.Y + (providerHeight / 2);

        var overlapMinX = Math.Max((providerWidth - 1) / 2, entityMinX - (providerWidth / 2));
        var overlapMinY = Math.Max((providerHeight - 1) / 2, entityMinY - (providerHeight / 2));
        var overlapMaxX = Math.Min(grid.Width - (providerWidth / 2) - 1, entityMaxX + ((providerWidth - 1) / 2));
        var overlapMaxY = Math.Min(grid.Height - (providerHeight / 2) - 1, entityMaxY + ((providerHeight - 1) / 2));

        return (
            entityMinX,
            entityMinY,
            entityMaxX,
            entityMaxY,
            overlapMinX,
            overlapMinY,
            overlapMaxX,
            overlapMaxY
        );
    }

    public static void AddProvider<TCenter, TSide>(
        SquareGrid grid,
        Location center,
        TCenter centerEntity,
        Func<TCenter, TSide> getNewSide,
        int providerWidth,
        int providerHeight,
        Dictionary<Location, BitArray> candidateToCovered)
        where TCenter : GridEntity
        where TSide : GridEntity
    {
        var (
            entityMinX,
            entityMinY,
            entityMaxX,
            entityMaxY,
            overlapMinX,
            overlapMinY,
            overlapMaxX,
            overlapMaxY
        ) = GetProviderBounds(grid, center, providerWidth, providerHeight);

        for (var x = overlapMinX; x <= overlapMaxX; x++)
        {
            for (var y = overlapMinY; y <= overlapMaxY; y++)
            {
                var location = new Location(x, y);
                candidateToCovered.Remove(location);

                if (x >= entityMinX && x <= entityMaxX
                    && y >= entityMinY && y <= entityMaxY)
                {
                    grid.AddEntity(location, location == center ? centerEntity : getNewSide(centerEntity));
                }
            }
        }
    }

    public static void RemoveProvider(SquareGrid grid, Location center, int providerWidth, int providerHeight)
    {
        var minX = center.X - ((providerWidth - 1) / 2);
        var maxX = center.X + (providerWidth / 2);
        var minY = center.Y - ((providerHeight - 1) / 2);
        var maxY = center.Y + (providerHeight / 2);

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                grid.RemoveEntity(new Location(x, y));
            }
        }
    }

    public static void AddProvider<TCenter, TSide>(
        SquareGrid grid,
        Location center,
        TCenter centerEntity,
        Func<TCenter, TSide> getNewSide,
        int providerWidth,
        int providerHeight)
        where TCenter : GridEntity
        where TSide : GridEntity
    {
        var minX = center.X - ((providerWidth - 1) / 2);
        var maxX = center.X + (providerWidth / 2);
        var minY = center.Y - ((providerHeight - 1) / 2);
        var maxY = center.Y + (providerHeight / 2);

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var location = new Location(x, y);
                grid.AddEntity(location, location == center ? centerEntity : getNewSide(centerEntity));
            }
        }
    }

    public static bool IsProviderInBounds(SquareGrid grid, int providerWidth, int providerHeight, Location center)
    {
        return center.X - ((providerWidth - 1) / 2) > 0
            && center.Y - ((providerHeight - 1) / 2) > 0
            && center.X + (providerWidth / 2) < grid.Width
            && center.Y + (providerHeight / 2) < grid.Height;
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

    public static bool All(this BitArray array, bool val)
    {
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i] != val)
            {
                return false;
            }
        }

        return true;
    }

    public static bool Any(this BitArray array, bool val)
    {
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i] == val)
            {
                return true;
            }
        }

        return false;
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

    /// <summary>
    /// An entity (e.g. a pumpjack) that receives the effect of a provider entity (e.g. electric pole, beacon).
    /// </summary>
    public record ProviderRecipient(Location Center, int Width, int Height);

    public record Endpoints(Location A, Location B);
}
