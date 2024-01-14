using System;
using System.Collections.Generic;
using DelaunatorSharp;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

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
    public static readonly IReadOnlyList<(Direction Direction, Location Location)> TerminalOffsets = new List<(Direction, Location)>
    {
        (Direction.Up, new Location(1, -2)),
        (Direction.Right, new Location(2, -1)),
        (Direction.Down, new Location(-1, 2)),
        (Direction.Left, new Location(-2, 1)),
    };

    public static PumpjackCenter AddPumpjack(SquareGrid grid, Location center)
    {
        var centerEntity = new PumpjackCenter(grid.GetId());
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                GridEntity entity = x != 0 || y != 0 ? new PumpjackSide(grid.GetId(), centerEntity) : centerEntity;
                grid.AddEntity(new Location(center.X + x, center.Y + y), entity);
            }
        }

        return centerEntity;
    }

    public static ILocationDictionary<List<TerminalLocation>> GetCenterToTerminals(Context context, SquareGrid grid, IEnumerable<Location> centers)
    {
        var centerToTerminals = context.GetLocationDictionary<List<TerminalLocation>>();
        PopulateCenterToTerminals(centerToTerminals, grid, centers);
        return centerToTerminals;
    }

    public static void PopulateCenterToTerminals(ILocationDictionary<List<TerminalLocation>> centerToTerminals, SquareGrid grid, IEnumerable<Location> centers)
    {
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

            if (candidateTerminals.Count == 0)
            {
                throw new FactorioToolsException("At least one pumpjack has no room for a pipe connection. Try removing some pumpjacks.", badInput: true);
            }

            centerToTerminals.Add(center, candidateTerminals);
        }
    }

    public static ILocationDictionary<List<TerminalLocation>> GetLocationToTerminals(Context context, ILocationDictionary<List<TerminalLocation>> centerToTerminals)
    {
        var locationToTerminals = context.GetLocationDictionary<List<TerminalLocation>>();
        PopulateLocationToTerminals(locationToTerminals, centerToTerminals);
        return locationToTerminals;
    }

    public static void PopulateLocationToTerminals(ILocationDictionary<List<TerminalLocation>> locationToTerminals, ILocationDictionary<List<TerminalLocation>> centerToTerminals)
    {
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
    }

    public static (ILocationDictionary<TInfo> CandidateToInfo, CountedBitArray CoveredEntities, ILocationDictionary<BeaconCenter> Providers) GetBeaconCandidateToCovered<TInfo>(
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

    public static (ILocationDictionary<TInfo> CandidateToInfo, CountedBitArray CoveredEntities, ILocationDictionary<ElectricPoleCenter> Providers) GetElectricPoleCandidateToCovered<TInfo>(
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

    private static (ILocationDictionary<TInfo> CandidateToInfo, CountedBitArray CoveredEntities, ILocationDictionary<TProvider> Providers) GetCandidateToCovered<TProvider, TInfo>(
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
        var candidateToInfo = context.GetLocationDictionary<TInfo>();
        var coveredEntities = new CountedBitArray(recipients.Count);

        var providers = context.GetLocationDictionary<TProvider>();
        foreach (var location in context.Grid.EntityLocations.EnumerateItems())
        {
            var provider = context.Grid[location] as TProvider;
            if (provider is null)
            {
                continue;
            }

            providers.Add(location, provider);
        }
        var unusedProviders = providers.Keys.ToReadOnlySet(context, allowEnumerate: removeUnused);

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
#if USE_SHARED_INSTANCES
            var coveredCenters = context.SharedInstances.LocationSetA;
#else
            var coveredCenters = context.GetLocationSet();
#endif

            try
            {
                foreach (var center in unusedProviders.EnumerateItems())
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
#if USE_SHARED_INSTANCES
                coveredCenters.Clear();
#endif
            }
        }

        if (providers.Count > 0 || unusedProviders.Count > 0)
        {
            // Remove candidates that only cover recipients that are already covered.
            var toRemove = new List<Location>();
            foreach ((var candidate, var info) in candidateToInfo.EnumeratePairs())
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

    public static ILocationDictionary<ILocationSet> GetProviderCenterToCoveredCenters(
        Context context,
        int providerWidth,
        int providerHeight,
        int supplyWidth,
        int supplyHeight,
        IEnumerable<Location> providerCenters,
        bool includePumpjacks,
        bool includeBeacons)
    {
        var poleCenterToCoveredCenters = context.GetLocationDictionary<ILocationSet>();

        foreach (var center in providerCenters)
        {
            var coveredCenters = context.GetLocationSet(allowEnumerate: true);
            AddCoveredCenters(
                coveredCenters,
                context.Grid,
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

    public static ILocationDictionary<ILocationSet> GetCoveredCenterToProviderCenters(Context context, ILocationDictionary<ILocationSet> providerCenterToCoveredCenters)
    {
        var output = context.GetLocationDictionary<ILocationSet>();

        foreach ((var center, var covered) in providerCenterToCoveredCenters.EnumeratePairs())
        {
            foreach (var otherCenter in covered.EnumerateItems())
            {
                if (!output.TryGetValue(otherCenter, out var centers))
                {
                    centers = context.GetLocationSet(allowEnumerate: true);
                    output.Add(otherCenter, centers);
                }

                centers.Add(center);
            }
        }

        return output;
    }

    private static void AddCoveredCenters(
        ILocationSet coveredCenters,
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
                    coveredCenters.Add(grid.EntityIdToLocation[pumpjackSide.Center.Id]);
                }
                else if (includeBeacons && entity is BeaconCenter)
                {
                    coveredCenters.Add(location);
                }
                else if (includeBeacons && entity is BeaconSide beaconSide)
                {
                    coveredCenters.Add(grid.EntityIdToLocation[beaconSide.Center.Id]);
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

    public static void AddProviderAndPreventMultipleProviders<TInfo>(
        Context context,
        Location center,
        TInfo centerInfo,
        int providerWidth,
        int providerHeight,
        List<ProviderRecipient> recipients,
        CountedBitArray coveredEntities,
        Dictionary<int, ILocationDictionary<TInfo>> coveredToCandidates,
        ILocationDictionary<TInfo> candidateToInfo)
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
            context.Grid,
            center,
            providerWidth,
            providerHeight,
            candidateToInfo,
            coveredToCandidates);

#if USE_SHARED_INSTANCES
        var toRemove = context.SharedInstances.LocationListA;
        var updated = context.SharedInstances.LocationSetA;
#else
        var toRemove = new List<Location>();
        var updated = context.GetLocationSet();
#endif

        try
        {
            for (var i = 0; i < recipients.Count; i++)
            {
                if (!newlyCovered[i])
                {
                    continue;
                }

                foreach ((var otherCandidate, var otherInfo) in coveredToCandidates[i].EnumeratePairs())
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
#if USE_SHARED_INSTANCES
            toRemove.Clear();
            updated.Clear();
#endif
        }
    }

    public static void AddProviderAndAllowMultipleProviders<TInfo>(
        Context context,
        Location center,
        TInfo centerInfo,
        int providerWidth,
        int providerHeight,
        List<ProviderRecipient> recipients,
        CountedBitArray coveredEntities,
        Dictionary<int, ILocationDictionary<TInfo>> coveredToCandidates,
        ILocationDictionary<TInfo> candidateToInfo,
        ILocationDictionary<TInfo> scopedCandidateToInfo,
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
            context.Grid,
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

#if USE_SHARED_INSTANCES
        var toRemove = context.SharedInstances.LocationListA;
        var updated = context.SharedInstances.LocationSetA;
#else
        var toRemove = new List<Location>();
        var updated = context.GetLocationSet();
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

                var currentCandidates = coveredToCandidates[i];
                foreach ((var otherCandidate, var otherInfo) in currentCandidates.EnumeratePairs())
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
                            modified = true;

                            // avoid modifying the collection we are enumerating
                            if (i != j)
                            {
                                coveredToCandidates[j].Remove(otherCandidate);
                            }
                        }
                    }

                    if (otherInfo.Covered.TrueCount == 0)
                    {
                        toRemove.Add(otherCandidate);
                        coveredCountBatches.RemoveCandidate(otherCandidate, oldCoveredCount);
                    }
                    else if (modified)
                    {
                        coveredCountBatches.MoveCandidate(context, otherCandidate, otherInfo, oldCoveredCount, otherInfo.Covered.TrueCount);

                        double entityDistance = 0;
                        for (var j = 0; j < recipients.Count; j++)
                        {
                            entityDistance += otherCandidate.GetEuclideanDistance(recipients[j].Center);
                        }

                        otherInfo.EntityDistance = entityDistance;
                    }
                }

                // now that we're done enumerating this dictionary, we can clear it
                currentCandidates.Clear();

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
#if USE_SHARED_INSTANCES
            toRemove.Clear();
            updated.Clear();
#endif
        }
    }

    public static (ILocationDictionary<ILocationSet> PoleCenterToCoveredCenters, ILocationDictionary<ILocationSet> CoveredCenterToPoleCenters) GetElectricPoleCoverage(
        Context context,
        List<ProviderRecipient> poweredEntities,
        IEnumerable<Location> electricPoleCenters)
    {
        var poleCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
            context,
            context.Options.ElectricPoleWidth,
            context.Options.ElectricPoleHeight,
            context.Options.ElectricPoleSupplyWidth,
            context.Options.ElectricPoleSupplyHeight,
            electricPoleCenters,
            includePumpjacks: true,
            includeBeacons: true);

        var coveredCenterToPoleCenters = GetCoveredCenterToProviderCenters(context, poleCenterToCoveredCenters);

        if (coveredCenterToPoleCenters.Count != poweredEntities.Count)
        {
            throw new FactorioToolsException("Not all powered entities are covered by an electric pole.");
        }

        return (PoleCenterToCoveredCenters: poleCenterToCoveredCenters, CoveredCenterToPoleCenters: coveredCenterToPoleCenters);
    }

    public static (List<ProviderRecipient> PoweredEntities, bool HasBeacons) GetPoweredEntities(Context context)
    {
        var poweredEntities = new List<ProviderRecipient>();
        var hasBeacons = false;

        foreach (var location in context.Grid.EntityLocations.EnumerateItems())
        {
            var entity = context.Grid[location];
            if (entity is PumpjackCenter)
            {
                poweredEntities.Add(new ProviderRecipient(location, PumpjackWidth, PumpjackHeight));
            }
            else if (entity is BeaconCenter)
            {
                poweredEntities.Add(new ProviderRecipient(location, context.Options.BeaconWidth, context.Options.BeaconHeight));
                hasBeacons = true;
            }
        }

        // sort the result so the above dictionary enumerator order does not impact output
        poweredEntities.Sort((a, b) =>
        {
            var c = a.Center.Y.CompareTo(b.Center.Y);
            if (c != 0)
            {
                return c;
            }

            return a.Center.X.CompareTo(b.Center.X);
        });

        return (poweredEntities, hasBeacons);
    }

    public static Dictionary<int, ILocationDictionary<TInfo>> GetCoveredToCandidates<TInfo>(
        Context context,
        ILocationDictionary<TInfo> allCandidateToInfo,
        CountedBitArray coveredEntities)
        where TInfo : CandidateInfo
    {
        var coveredToCandidates = new Dictionary<int, ILocationDictionary<TInfo>>(coveredEntities.Count);
        for (var i = 0; i < coveredEntities.Count; i++)
        {
            var candidates = context.GetLocationDictionary<TInfo>();
            foreach ((var candidate, var info) in allCandidateToInfo.EnumeratePairs())
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
        ILocationDictionary<TInfo> candidateToInfo)
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
        ILocationDictionary<TInfo> candidateToInfo,
        Dictionary<int, ILocationDictionary<TInfo>> coveredToCandidates)
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
        ILocationDictionary<TInfo> candidateToInfo,
        ILocationDictionary<TInfo> scopedCandidateToInfo,
        Dictionary<int, ILocationDictionary<TInfo>> coveredToCandidates)
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
                new BeaconCenter(grid.GetId()),
                c => new BeaconSide(grid.GetId(), c),
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

    public static List<Location> GetPath(ILocationDictionary<Location> cameFrom, Location start, Location reachedGoal)
    {
        var sizeEstimate = 2 * start.GetManhattanDistance(reachedGoal);
        var path = new List<Location>(sizeEstimate);
        AddPath(cameFrom, reachedGoal, path);
        return path;
    }

    public static void AddPath(ILocationDictionary<Location> cameFrom, Location reachedGoal, List<Location> outputList)
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

    public static bool AreLocationsCollinear(IReadOnlyList<Location> locations)
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

    public static List<Location>? MakeStraightLineOnEmpty(SquareGrid grid, Location a, Location b)
    {
        if (a.X == b.X)
        {
            (var min, var max) = a.Y < b.Y ? (a.Y, b.Y) : (b.Y, a.Y);
            var line = new List<Location>(max - min + 1);
            for (var y = min; y <= max; y++)
            {
                if (!grid.IsEmpty(new Location(a.X, y)))
                {
                    return null;
                }

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
                if (!grid.IsEmpty(new Location(x, a.Y)))
                {
                    return null;
                }

                line.Add(new Location(x, a.Y));
            }

            return line;
        }

        throw new ArgumentException("The two points must be one the same line either horizontally or vertically.");
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

    public static List<Endpoints> PointsToLines(IReadOnlyCollection<Location> nodes)
    {
        return PointsToLines(nodes.ToList(), sort: true);
    }

    /// <summary>
    /// Source: https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts#L62
    /// </summary>
    public static List<Endpoints> PointsToLines(IReadOnlyList<Location> nodes, bool sort)
    {
        IReadOnlyList<Location> filteredNodes;
        if (sort)
        {
            var sortedXY = nodes.ToList();
            sortedXY.Sort((a, b) =>
            {
                var c = a.X.CompareTo(b.X);
                if (c != 0)
                {
                    return c;
                }

                return a.Y.CompareTo(b.Y);
            });
            filteredNodes = sortedXY;
        }
        else
        {
            filteredNodes = nodes;
        }

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
            var collinearLines = new List<Endpoints>(filteredNodes.Count - 1);
            for (var i = 1; i < filteredNodes.Count; i++)
            {
                collinearLines.Add(new Endpoints(filteredNodes[i - 1], filteredNodes[i]));
            }

            return collinearLines;
        }

        var points = new IPoint[filteredNodes.Count];
        for (var i = 0; i < filteredNodes.Count; i++)
        {
            var node = filteredNodes[i];
            points[i] = new Point(node.X, node.Y);
        }
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
}
