using System.Collections.Generic;
using System.Linq;
using Knapcode.FactorioTools.Data;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

public static class Validate
{
    public static void PipesAreConnected(Context context, LocationSet optimizedPipes)
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

            var goals = context.CenterToTerminals.Values.SelectMany(ts => ts).Select(t => t.Terminal).ToSet();
            var clone = new ExistingPipeGrid(context.Grid, optimizedPipes);
            var start = goals.EnumerateItems().First();
            goals.Remove(start);
            var result = Dijkstras.GetShortestPaths(context.SharedInstances, clone, start, goals, stopOnFirstGoal: false);
            var reachedGoals = result.ReachedGoals;
            reachedGoals.Add(start);
            var unreachedGoals = goals.Except(reachedGoals).ToSet();
            if (unreachedGoals.Count > 0)
            {
                // Visualizer.Show(context.Grid, optimizedPipes.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                throw new FactorioToolsException("The pipes are not fully connected.");
            }
        }
    }

    public static void UndergroundPipesArePipes(Context context, LocationSet pipes, Dictionary<Location, Direction> locationToDirection)
    {
        if (context.Options.ValidateSolution)
        {
            var missing = locationToDirection.Keys.Except(pipes.EnumerateItems()).ToList();
            if (missing.Count > 0)
            {
                throw new FactorioToolsException("The underground pipes should be in the pipe set.");
            }
        }
    }

    public static void PipesDoNotMatch(
        Context context,
        LocationSet pipes1,
        Dictionary<Location, Direction>? undergroundPipes1,
        LocationSet pipes2,
        Dictionary<Location, Direction>? undergroundPipes2)
    {
        if (context.Options.ValidateSolution)
        {
            if (pipes1.SetEquals(pipes2))
            {
                throw new FactorioToolsException("The two pipe configurations should not match.");
            }
        }
    }

    public static void BeaconsDoNotOverlap(Context context, List<BeaconSolution> solutions)
    {
        if (context.Options.ValidateSolution && !context.Options.OverlapBeacons)
        {
            foreach (var solution in solutions)
            {
                var beaconCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
                    context.Grid,
                    context.Options.BeaconWidth,
                    context.Options.BeaconHeight,
                    context.Options.BeaconSupplyWidth,
                    context.Options.BeaconSupplyHeight,
                    solution.Beacons,
                    includePumpjacks: true,
                    includeBeacons: false);

                var coveredCenterToPoleCenters = GetCoveredCenterToProviderCenters(beaconCenterToCoveredCenters);

                foreach ((var pumpjackCenter, var beaconCenters) in coveredCenterToPoleCenters)
                {
                    if (beaconCenters.Count > 1)
                    {
                        throw new FactorioToolsException("Multiple beacons are providing an effect to a pumpjack.");
                    }
                }
            }
        }
    }

    public static void NoExistingBeacons(Context context, Dictionary<Location, BeaconCenter> existingBeacons)
    {
        if (context.Options.ValidateSolution && existingBeacons.Count > 0)
        {
            throw new FactorioToolsException("There should not be any existing beacons.");
        }
    }

    public static void NoOverlappingEntities(
        Context context,
        LocationSet optimizedPipes,
        Dictionary<Location, Direction>? undergroundPipes,
        List<BeaconSolution>? beaconSolutions)
    {
        if (context.Options.ValidateSolution)
        {
            if (beaconSolutions is null)
            {
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone, context.SharedInstances, context.CenterToTerminals, optimizedPipes, undergroundPipes);
            }
            else
            {
                foreach (var solution in beaconSolutions)
                {
                    var clone = new PipeGrid(context.Grid);
                    AddPipeEntities.Execute(clone, context.SharedInstances, context.CenterToTerminals, optimizedPipes, undergroundPipes);
                    AddBeaconsToGrid(clone, context.Options, solution.Beacons);
                }
            }
        }
    }

    public static void CandidateCoversMoreEntities(
        Context context,
        List<ProviderRecipient> poweredEntities,
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

            var electricPoleCenters = new List<Location>();
            foreach ((var entity, var location) in context.Grid.EntityToLocation)
            {
                if (entity is ElectricPoleCenter)
                {
                    electricPoleCenters.Add(location);
                }
            }

            GetElectricPoleCoverage(context, poweredEntities, electricPoleCenters);
        }
    }
}
