using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Knapcode.FactorioTools.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

public static class Helpers
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

    public static (Dictionary<Location, TInfo> CandidateToInfo, CountedBitArray CoveredEntities, Dictionary<Location, BeaconCenter> Providers) GetBeaconCandidateToCovered<TInfo>(
        Context context,
        List<ProviderRecipient> recipients,
        ICandidateFactory<TInfo> candidateFactory,
        bool removeUnused)
        where TInfo : CandidateInfo
    {
        return GetCandidateToCovered<BeaconCenter, TInfo>(
            context,
            recipients,
            candidateFactory,
            context.Options.BeaconWidth,
            context.Options.BeaconHeight,
            context.Options.BeaconSupplyWidth,
            context.Options.BeaconSupplyHeight,
            removeUnused,
            includePumpjacks: true,
            includeBeacons: false);
    }

    public static (Dictionary<Location, TInfo> CandidateToInfo, CountedBitArray CoveredEntities, Dictionary<Location, ElectricPoleCenter> Providers) GetElectricPoleCandidateToCovered<TInfo>(
        Context context,
        List<ProviderRecipient> recipients,
        ICandidateFactory<TInfo> candidateFactory,
        bool removeUnused)
        where TInfo : CandidateInfo
    {
        return GetCandidateToCovered<ElectricPoleCenter, TInfo>(
            context,
            recipients,
            candidateFactory,
            context.Options.ElectricPoleWidth,
            context.Options.ElectricPoleHeight,
            context.Options.ElectricPoleSupplyWidth,
            context.Options.ElectricPoleSupplyHeight,
            removeUnused,
            includePumpjacks: true,
            includeBeacons: true);
    }

    public interface ICandidateFactory<TInfo> where TInfo : CandidateInfo
    {
        TInfo Create(CountedBitArray covered);
    }

    private static (Dictionary<Location, TInfo> CandidateToInfo, CountedBitArray CoveredEntities, Dictionary<Location, TProvider> Providers) GetCandidateToCovered<TProvider, TInfo>(
        Context context,
        List<ProviderRecipient> recipients,
        ICandidateFactory<TInfo> candidateFactory,
        int providerWidth,
        int providerHeight,
        int supplyWidth,
        int supplyHeight,
        bool removeUnused,
        bool includePumpjacks,
        bool includeBeacons)
        where TProvider : GridEntity
        where TInfo : CandidateInfo
    {
        var candidateToInfo = new Dictionary<Location, TInfo>();
        var coveredEntities = new CountedBitArray(recipients.Count);

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

                        if (!candidateToInfo.TryGetValue(candidate, out var info))
                        {
                            var covered = new CountedBitArray(recipients.Count);
                            covered[i] = true;
                            info = candidateFactory.Create(covered);
                            candidateToInfo.Add(candidate, info);
                        }
                        else
                        {
                            info.Covered[i] = true;
                        }
                    }
                }
            }
        }

        if (removeUnused && unusedProviders.Count > 0)
        {
#if NO_SHARED_INSTANCES
            var coveredCenters = new HashSet<Location>();
#else
            var coveredCenters = context.SharedInstances.LocationSetA;
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

                                if (!candidateToInfo.TryGetValue(candidate, out var info))
                                {
                                    var covered = new CountedBitArray(recipients.Count);
                                    info = candidateFactory.Create(covered);
                                    candidateToInfo.Add(candidate, info);
                                }

                                for (var i = 0; i < recipients.Count; i++)
                                {
                                    if (coveredCenters.Contains(recipients[i].Center))
                                    {
                                        info.Covered[i] = true;
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
#if !NO_SHARED_INSTANCES
                coveredCenters.Clear();
#endif
            }
        }

        if (providers.Count > 0 || unusedProviders.Count > 0)
        {
            // Remove candidates that only cover recipients that are already covered.
            var toRemove = new List<Location>();
            foreach ((var candidate, var info) in candidateToInfo)
            {
                var subset = new CountedBitArray(info.Covered);
                subset.Not();
                subset.Or(coveredEntities);
                if (subset.All(true))
                {
                    toRemove.Add(candidate);
                }
            }

            for (var i = 0; i < toRemove.Count; i++)
            {
                candidateToInfo.Remove(toRemove[i]);
            }
        }

        return (candidateToInfo, coveredEntities, providers);
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

    public static Dictionary<Location, HashSet<Location>> GetCoveredCenterToProviderCenters(Dictionary<Location, HashSet<Location>> providerCenterToCoveredCenters)
    {
        return providerCenterToCoveredCenters
            .SelectMany(p => p.Value.Select(c => (PoleCenter: p.Key, RecipientCenter: c)))
            .GroupBy(p => p.RecipientCenter, p => p.PoleCenter)
            .ToDictionary(g => g.Key, g => g.ToHashSet());
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
    public static bool DoesProviderFit(
        SquareGrid grid,
        int providerWidth,
        int providerHeight,
        Location center)
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
                if (!grid.IsEmpty(location))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static double GetEntityDistance(List<ProviderRecipient> poweredEntities, Location candidate, CountedBitArray covered)
    {
        double sum = 0;
        for (var i = 0; i < poweredEntities.Count; i++)
        {
            if (covered[i])
            {
                sum += candidate.GetEuclideanDistance(poweredEntities[i].Center);
            }
        }

        return sum;
    }

    public class SortedBatches<TInfo>
    {
        private readonly bool _ascending;

        public SortedBatches(IEnumerable<KeyValuePair<int, Dictionary<Location, TInfo>>> pairs, bool ascending)
        {
            _ascending = ascending;
            Queue = new PriorityQueue<Dictionary<Location, TInfo>, int>();
            Lookup = new Dictionary<int, Dictionary<Location, TInfo>>();

            foreach ((var key, var candidateToInfo) in pairs)
            {
                Queue.Enqueue(candidateToInfo, _ascending ? key : -key);
                Lookup.Add(key, candidateToInfo);
            }
        }

        public PriorityQueue<Dictionary<Location, TInfo>, int> Queue { get; }
        public Dictionary<int, Dictionary<Location, TInfo>> Lookup { get; }

        public void RemoveCandidate(Location location, int oldKey)
        {
            Lookup[oldKey].Remove(location);
        }

        public void MoveCandidate(Location location, TInfo info, int oldKey, int newKey)
        {
            Lookup[oldKey].Remove(location);
            Lookup[newKey].Add(location, info);
        }
    }

    public static void AddProviderAndPreventMultipleProviders<TInfo>(
        SquareGrid grid,
        SharedInstances sharedInstances,
        Location center,
        TInfo centerInfo,
        int providerWidth,
        int providerHeight,
        List<ProviderRecipient> recipients,
        CountedBitArray coveredEntities,
        Dictionary<int, Dictionary<Location, TInfo>> coveredToCandidates,
        Dictionary<Location, TInfo> candidateToInfo)
        where TInfo : CandidateInfo
    {
        // Console.WriteLine("adding " + center);

        var newlyCovered = new CountedBitArray(coveredEntities);
        newlyCovered.Not();
        newlyCovered.And(centerInfo.Covered);

        if (newlyCovered.TrueCount == 0)
        {
            throw new FactorioToolsException("At least one recipient should should have been newly covered.");
        }

        coveredEntities.Or(centerInfo.Covered);

        RemoveOverlappingCandidates(
            grid,
            center,
            providerWidth,
            providerHeight,
            candidateToInfo,
            coveredToCandidates);

#if NO_SHARED_INSTANCES
        var toRemove = new List<Location>();
        var updated = new HashSet<Location>();
#else
        var toRemove = sharedInstances.LocationListA;
        var updated = sharedInstances.LocationSetA;
#endif

        try
        {
            for (var i = 0; i < recipients.Count; i++)
            {
                if (!newlyCovered[i])
                {
                    continue;
                }

                foreach ((var otherCandidate, var otherInfo) in coveredToCandidates[i])
                {
                    if (!updated.Add(otherCandidate))
                    {
                        continue;
                    }

                    toRemove.Add(otherCandidate);
                }

                if (toRemove.Count > 0)
                {
                    for (var j = 0; j < toRemove.Count; j++)
                    {
                        candidateToInfo.Remove(toRemove[j]);
                    }

                    toRemove.Clear();
                }
            }

        }
        finally
        {
#if !NO_SHARED_INSTANCES
            toRemove.Clear();
            updated.Clear();
#endif
        }
    }

    public static void AddProviderAndAllowMultipleProviders<TInfo>(
        SquareGrid grid,
        SharedInstances sharedInstances,
        Location center,
        TInfo centerInfo,
        int providerWidth,
        int providerHeight,
        List<ProviderRecipient> recipients,
        CountedBitArray coveredEntities,
        Dictionary<int, Dictionary<Location, TInfo>> coveredToCandidates,
        Dictionary<Location, TInfo> candidateToInfo,
        Dictionary<Location, TInfo> scopedCandidateToInfo,
        SortedBatches<TInfo> coveredCountBatches)
        where TInfo : CandidateInfo
    {
        // Console.WriteLine("adding " + center);

        var newlyCovered = new CountedBitArray(coveredEntities);
        newlyCovered.Not();
        newlyCovered.And(centerInfo.Covered);

        if (newlyCovered.TrueCount == 0)
        {
            throw new FactorioToolsException("At least one recipient should should have been newly covered.");
        }

        coveredEntities.Or(centerInfo.Covered);

        RemoveOverlappingCandidates(
            grid,
            center,
            providerWidth,
            providerHeight,
            candidateToInfo,
            scopedCandidateToInfo,
            coveredToCandidates);

        if (coveredEntities.All(true))
        {
            return;
        }

#if NO_SHARED_INSTANCES
        var toRemove = new List<Location>();
        var updated = new HashSet<Location>();
#else
        var toRemove = sharedInstances.LocationListA;
        var updated = sharedInstances.LocationSetA;
#endif

        try
        {
            // Remove the covered entities from the candidate data, so that the next candidates are discounted
            // by the entities that no longer need to be covered.
            for (var i = 0; i < recipients.Count; i++)
            {
                if (!newlyCovered[i])
                {
                    continue;
                }

                foreach ((var otherCandidate, var otherInfo) in coveredToCandidates[i])
                {
                    if (!updated.Add(otherCandidate))
                    {
                        continue;
                    }

                    var modified = false;
                    var oldCoveredCount = otherInfo.Covered.TrueCount;
                    for (var j = 0; j < recipients.Count && otherInfo.Covered.TrueCount > 0; j++)
                    {
                        if (coveredEntities[j] && otherInfo.Covered[j])
                        {
                            otherInfo.Covered[j] = false;
                            coveredToCandidates[j].Remove(otherCandidate);
                            modified = true;
                        }
                    }

                    if (otherInfo.Covered.TrueCount == 0)
                    {
                        toRemove.Add(otherCandidate);
                        coveredCountBatches.RemoveCandidate(otherCandidate, oldCoveredCount);
                    }
                    else if (modified)
                    {
                        coveredCountBatches.MoveCandidate(otherCandidate, otherInfo, oldCoveredCount, otherInfo.Covered.TrueCount);

                        double entityDistance = 0;
                        for (var j = 0; j < recipients.Count; j++)
                        {
                            entityDistance += otherCandidate.GetEuclideanDistance(recipients[j].Center);
                        }

                        otherInfo.EntityDistance = entityDistance;
                    }
                }

                if (toRemove.Count > 0)
                {
                    for (var j = 0; j < toRemove.Count; j++)
                    {
                        if (candidateToInfo.Remove(toRemove[j]))
                        {
                            scopedCandidateToInfo.Remove(toRemove[j]);
                        }
                    }

                    toRemove.Clear();
                }
            }

        }
        finally
        {
#if !NO_SHARED_INSTANCES
            toRemove.Clear();
            updated.Clear();
#endif
        }
    }

    public static (Dictionary<Location, HashSet<Location>> PoleCenterToCoveredCenters, Dictionary<Location, HashSet<Location>> CoveredCenterToPoleCenters) GetElectricPoleCoverage(
        Context context,
        List<ProviderRecipient> poweredEntities,
        IEnumerable<Location> electricPoleCenters)
    {
        var poleCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
            context.Grid,
            context.Options.ElectricPoleWidth,
            context.Options.ElectricPoleHeight,
            context.Options.ElectricPoleSupplyWidth,
            context.Options.ElectricPoleSupplyHeight,
            electricPoleCenters,
            includePumpjacks: true,
            includeBeacons: true);

        var coveredCenterToPoleCenters = GetCoveredCenterToProviderCenters(poleCenterToCoveredCenters);

        if (coveredCenterToPoleCenters.Count != poweredEntities.Count)
        {
            var uncoveredCenters = poweredEntities
                .Select(e => e.Center)
                .Except(coveredCenterToPoleCenters.Keys)
                .ToList();
            // Visualizer.Show(context.Grid, uncoveredCenters.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());
            throw new FactorioToolsException("Not all powered entities are covered by an electric pole.");
        }

        return (PoleCenterToCoveredCenters: poleCenterToCoveredCenters, CoveredCenterToPoleCenters: coveredCenterToPoleCenters);
    }

    public static (List<ProviderRecipient> PoweredEntities, bool HasBeacons) GetPoweredEntities(Context context)
    {
        var poweredEntities = new List<ProviderRecipient>();
        var hasBeacons = false;
        foreach ((var entity, var location) in context.Grid.EntityToLocation)
        {
            switch (entity)
            {
                case PumpjackCenter:
                    poweredEntities.Add(new ProviderRecipient(location, PumpjackWidth, PumpjackHeight));
                    break;
                case BeaconCenter:
                    poweredEntities.Add(new ProviderRecipient(location, context.Options.BeaconWidth, context.Options.BeaconHeight));
                    hasBeacons = true;
                    break;
            }
        }

        return (poweredEntities, hasBeacons);
    }

    public static Dictionary<int, Dictionary<Location, TInfo>> GetCoveredToCandidates<TInfo>(
        Dictionary<Location, TInfo> allCandidateToInfo,
        CountedBitArray coveredEntities)
        where TInfo : CandidateInfo
    {
        var coveredToCandidates = new Dictionary<int, Dictionary<Location, TInfo>>(coveredEntities.Count);
        for (var i = 0; i < coveredEntities.Count; i++)
        {
            var candidates = new Dictionary<Location, TInfo>();
            foreach ((var candidate, var info) in allCandidateToInfo)
            {
                if (info.Covered[i])
                {
                    candidates.Add(candidate, info);
                }
            }

            coveredToCandidates.Add(i, candidates);
        }

        return coveredToCandidates;
    }

    public static (
        int OverlapMinX,
        int OverlapMinY,
        int OverlapMaxX,
        int OverlapMaxY
    ) GetProviderOverlapBounds(
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
            overlapMinX,
            overlapMinY,
            overlapMaxX,
            overlapMaxY
        );
    }

    public static void RemoveOverlappingCandidates<TInfo>(
        SquareGrid grid,
        Location center,
        int providerWidth,
        int providerHeight,
        Dictionary<Location, TInfo> candidateToInfo)
        where TInfo : CandidateInfo
    {
        var (
            overlapMinX,
            overlapMinY,
            overlapMaxX,
            overlapMaxY
        ) = GetProviderOverlapBounds(grid, center, providerWidth, providerHeight);

        for (var x = overlapMinX; x <= overlapMaxX; x++)
        {
            for (var y = overlapMinY; y <= overlapMaxY; y++)
            {
                var location = new Location(x, y);
                candidateToInfo.Remove(location);
            }
        }
    }

    public static void RemoveOverlappingCandidates<TInfo>(
        SquareGrid grid,
        Location center,
        int providerWidth,
        int providerHeight,
        Dictionary<Location, TInfo> candidateToInfo,
        Dictionary<int, Dictionary<Location, TInfo>> coveredToCandidates)
        where TInfo : CandidateInfo
    {
        var (
            overlapMinX,
            overlapMinY,
            overlapMaxX,
            overlapMaxY
        ) = GetProviderOverlapBounds(grid, center, providerWidth, providerHeight);

        for (var x = overlapMinX; x <= overlapMaxX; x++)
        {
            for (var y = overlapMinY; y <= overlapMaxY; y++)
            {
                var location = new Location(x, y);
                if (candidateToInfo.TryGetValue(location, out var info))
                {
                    candidateToInfo.Remove(location);
                    for (var i = 0; i < info.Covered.Count; i++)
                    {
                        if (info.Covered[i])
                        {
                            coveredToCandidates[i].Remove(location);
                        }
                    }
                }
            }
        }
    }

    public static void RemoveOverlappingCandidates<TInfo>(
        SquareGrid grid,
        Location center,
        int providerWidth,
        int providerHeight,
        Dictionary<Location, TInfo> candidateToInfo,
        Dictionary<Location, TInfo> scopedCandidateToInfo,
        Dictionary<int, Dictionary<Location, TInfo>> coveredToCandidates)
        where TInfo : CandidateInfo
    {
        var (
            overlapMinX,
            overlapMinY,
            overlapMaxX,
            overlapMaxY
        ) = GetProviderOverlapBounds(grid, center, providerWidth, providerHeight);

        for (var x = overlapMinX; x <= overlapMaxX; x++)
        {
            for (var y = overlapMinY; y <= overlapMaxY; y++)
            {
                var location = new Location(x, y);
                if (coveredToCandidates is not null)
                {
                    if (candidateToInfo.TryGetValue(location, out var info))
                    {
                        candidateToInfo.Remove(location);
                        scopedCandidateToInfo.Remove(location);
                        for (var i = 0; i < info.Covered.Count; i++)
                        {
                            if (info.Covered[i])
                            {
                                coveredToCandidates[i].Remove(location);
                            }
                        }
                    }
                }
                else if (candidateToInfo.Remove(location))
                {
                    scopedCandidateToInfo.Remove(location);
                }
            }
        }
    }

    public static void RemoveEntity(SquareGrid grid, Location center, int width, int height)
    {
        var minX = center.X - ((width - 1) / 2);
        var maxX = center.X + (width / 2);
        var minY = center.Y - ((height - 1) / 2);
        var maxY = center.Y + (height / 2);

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                grid.RemoveEntity(new Location(x, y));
            }
        }
    }

    public static void AddProviderToGrid<TCenter, TSide>(
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

    public static void AddBeaconsToGrid(SquareGrid grid, OilFieldOptions options, IEnumerable<Location> centers)
    {
        foreach (var center in centers)
        {
            AddProviderToGrid(
                grid,
                center,
                new BeaconCenter(),
                c => new BeaconSide(c),
                options.BeaconWidth,
                options.BeaconHeight);
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

    /// <summary>
    /// An entity (e.g. a pumpjack) that receives the effect of a provider entity (e.g. electric pole, beacon).
    /// </summary>
    public record ProviderRecipient(Location Center, int Width, int Height);

    public record Endpoints(Location A, Location B);
}
