using System.Collections.Generic;
using Knapcode.FactorioTools.Data;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

public static class Validate
{
    public static void PipesAreConnected(Context context, ILocationSet optimizedPipes)
    {
        if (context.Options.ValidateSolution)
        {
            foreach (var terminals in context.CenterToTerminals.Values)
            {
                if (terminals.Count != 1)
                {
                    throw new FactorioToolsException("A pumpjack has more than one terminal.");
                }
            }

            var goals = context.GetLocationSet(allowEnumerate: true);
            foreach (var terminals in context.CenterToTerminals.Values)
            {
                for (var i = 0; i < terminals.Count; i++)
                {
                    goals.Add(terminals[i].Terminal);
                }
            }

            var clone = new ExistingPipeGrid(context.Grid, optimizedPipes);
            var start = goals.EnumerateItems().First();
            goals.Remove(start);
            var result = Dijkstras.GetShortestPaths(context, clone, start, goals, stopOnFirstGoal: false, allowGoalEnumerate: true);
            var reachedGoals = result.ReachedGoals;
            reachedGoals.Add(start);
            var unreachedGoals = context.GetLocationSet(goals);
            unreachedGoals.ExceptWith(reachedGoals);
            if (unreachedGoals.Count > 0)
            {
                // Visualizer.Show(context.Grid, optimizedPipes.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                throw new FactorioToolsException("The pipes are not fully connected.");
            }
        }
    }

    public static void UndergroundPipesArePipes(Context context, ILocationSet pipes, ILocationDictionary<Direction> locationToDirection)
    {
        if (context.Options.ValidateSolution)
        {
            var missing = locationToDirection.Keys.ToSet(context, allowEnumerate: true);
            missing.ExceptWith(pipes);

            if (missing.Count > 0)
            {
                throw new FactorioToolsException("The underground pipes should be in the pipe set.");
            }
        }
    }

    public static void PipesDoNotMatch(
        Context context,
        ILocationSet pipes1,
        ILocationSet pipes2)
    {
        if (context.Options.ValidateSolution)
        {
            if (pipes1.SetEquals(pipes2))
            {
                throw new FactorioToolsException("The two pipe configurations should not match.");
            }
        }
    }

    public static void BeaconsDoNotOverlap(Context context, ITableList<BeaconSolution> solutions)
    {
        if (context.Options.ValidateSolution && !context.Options.OverlapBeacons)
        {
            for (var i = 0; i < solutions.Count; i++)
            {
                var beaconCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
                    context,
                    context.Options.BeaconWidth,
                    context.Options.BeaconHeight,
                    context.Options.BeaconSupplyWidth,
                    context.Options.BeaconSupplyHeight,
                    solutions[i].Beacons.EnumerateItems(),
                    includePumpjacks: true,
                    includeBeacons: false);

                var coveredCenterToPoleCenters = GetCoveredCenterToProviderCenters(context, beaconCenterToCoveredCenters);

                foreach ((var pumpjackCenter, var beaconCenters) in coveredCenterToPoleCenters.EnumeratePairs())
                {
                    if (beaconCenters.Count > 1)
                    {
                        throw new FactorioToolsException("Multiple beacons are providing an effect to a pumpjack.");
                    }
                }
            }
        }
    }

    public static void NoExistingBeacons(Context context, ILocationDictionary<BeaconCenter> existingBeacons)
    {
        if (context.Options.ValidateSolution && existingBeacons.Count > 0)
        {
            throw new FactorioToolsException("There should not be any existing beacons.");
        }
    }

    public static void NoOverlappingEntities(
        Context context,
        ILocationSet optimizedPipes,
        ILocationDictionary<Direction>? undergroundPipes,
        ITableList<BeaconSolution>? beaconSolutions)
    {
        if (context.Options.ValidateSolution)
        {
            if (beaconSolutions is null)
            {
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(context, clone, optimizedPipes, undergroundPipes);
            }
            else
            {
                for (var i = 0; i < beaconSolutions.Count; i++)
                {
                    var clone = new PipeGrid(context.Grid);
                    AddPipeEntities.Execute(context, clone, optimizedPipes, undergroundPipes);
                    AddBeaconsToGrid(clone, context.Options, beaconSolutions[i].Beacons);
                }
            }
        }
    }

    public static void CandidateCoversMoreEntities(
        Context context,
        ITableList<ProviderRecipient> poweredEntities,
        CountedBitArray coveredEntities,
        Location candidate,
        ElectricPoleCandidateInfo candidateInfo)
    {
        if (context.Options.ValidateSolution)
        {
            var covered = candidateInfo.Covered;
            var isSubsetOf = true;
            for (var i = 0; i < poweredEntities.Count && isSubsetOf; i++)
            {
                if (covered[i])
                {
                    isSubsetOf = coveredEntities[i];
                }
            }

            if (isSubsetOf)
            {
                // Visualizer.Show(context.Grid, new[] { candidate }.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());
                throw new FactorioToolsException($"Candidate {candidate} should have been eliminated.");
            }
        }
    }

    public static void AllEntitiesHavePower(Context context)
    {
        if (context.Options.ValidateSolution)
        {
            (var poweredEntities, _) = GetPoweredEntities(context);

            var electricPoleCenters = TableArray.New<Location>();
            foreach (var location in context.Grid.EntityLocations.EnumerateItems())
            {
                var entity = context.Grid[location];
                if (entity is ElectricPoleCenter)
                {
                    electricPoleCenters.Add(location);
                }
            }

            GetElectricPoleCoverage(context, poweredEntities, electricPoleCenters.EnumerateItems());
        }
    }
}
