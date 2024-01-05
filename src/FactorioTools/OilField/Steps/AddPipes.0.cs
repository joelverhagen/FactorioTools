using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Knapcode.FactorioTools.Data;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

public static partial class AddPipes
{
    public static (List<OilFieldPlan> SelectedPlans, List<OilFieldPlan> AlternatePlans, List<OilFieldPlan> UnusedPlans)
        Execute(Context context, bool eliminateStrandedTerminals)
    {
        if (eliminateStrandedTerminals)
        {
            EliminateStrandedTerminals(context);
        }

        List<OilFieldPlan> selectedPlans;
        List<OilFieldPlan> alternatePlans;
        List<OilFieldPlan> unusedPlans;
        Solution bestSolution;
        BeaconSolution? bestBeacons;

        try
        {
            (selectedPlans, alternatePlans, unusedPlans, bestSolution, bestBeacons) = GetBestSolution(context);
        }
        catch (NoPathBetweenTerminalsException) when (!eliminateStrandedTerminals)
        {
            EliminateStrandedTerminals(context);
            (selectedPlans, alternatePlans, unusedPlans, bestSolution, bestBeacons) = GetBestSolution(context);
        }

        context.CenterToTerminals = bestSolution.CenterToTerminals;
        context.LocationToTerminals = bestSolution.LocationToTerminals;

        AddPipeEntities.Execute(context, bestSolution.Pipes, bestSolution.UndergroundPipes);

        if (bestBeacons is not null)
        {
            // Visualizer.Show(context.Grid, bestSolution.Beacons.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());
            AddBeaconsToGrid(context.Grid, context.Options, bestBeacons.Beacons);
        }

        return (selectedPlans, alternatePlans, unusedPlans);
    }

    private static (List<OilFieldPlan> SelectedPlans, List<OilFieldPlan> AltnernatePlans, List<OilFieldPlan> UnusedPlans, Solution BestSolution, BeaconSolution? BestBeacons)
        GetBestSolution(Context context)
    {
        var sortedPlans = GetAllPlans(context)
            .OrderByDescending(x => x.Plan.BeaconEffectCount) // more effects = better
            .ThenBy(x => x.Plan.BeaconCount) // fewer beacons = better (less power)
            .ThenBy(x => x.Plan.PipeCount) // fewer pipes = better
            .ThenByDescending(x => x.GroupSize) // prefer solutions that more algorithms find
            .ThenBy(x => x.Plan.PipeStrategy)  // the rest of the sorting is for arbitrary tie breaking
            .ThenBy(x => x.Plan.OptimizePipes)
            .ThenBy(x => x.Plan.BeaconStrategy)
            .ThenBy(x => x.GroupNumber);

        (int GroupNumber, int GroupSize, OilFieldPlan Plan, Solution Pipes, BeaconSolution? Beacons)? bestPlanInfo = null;
        var noMoreAlternates = false;
        var selectedPlans = new List<OilFieldPlan>();
        var alternatePlans = new List<OilFieldPlan>();
        var unusedPlans = new List<OilFieldPlan>();

        foreach (var planInfo in sortedPlans)
        {
            if (noMoreAlternates)
            {
                unusedPlans.Add(planInfo.Plan);
                continue;
            }
            else if (bestPlanInfo is null)
            {
                bestPlanInfo = planInfo;
                selectedPlans.Add(planInfo.Plan);
                continue;
            }

            var (bestGroupNumber, _, bestPlan, _, _) = bestPlanInfo.Value;
            if (planInfo.Plan.IsEquivalent(bestPlan))
            {
                if (planInfo.GroupNumber == bestGroupNumber)
                {
                    selectedPlans.Add(planInfo.Plan);
                }
                else
                {
                    alternatePlans.Add(planInfo.Plan);
                }
            }
            else
            {
                noMoreAlternates = true;
                unusedPlans.Add(planInfo.Plan);
            }
        }

        if (bestPlanInfo is null)
        {
            throw new FactorioToolsException("At least one pipe strategy must be used.");
        }

        return (selectedPlans, alternatePlans, unusedPlans, bestPlanInfo.Value.Pipes, bestPlanInfo.Value.Beacons);
    }


    private static IEnumerable<(int GroupNumber, int GroupSize, OilFieldPlan Plan, Solution Pipes, BeaconSolution? Beacons)> GetAllPlans(Context context)
    {
        var solutionGroups = GetSolutionGroups(context);
        foreach ((var solutionGroup, var groupNumber) in solutionGroups)
        {
            foreach (var solution in solutionGroup)
            {
                if (solution.BeaconSolutions is null)
                {
                    foreach (var strategy in solution.Strategies)
                    {
                        foreach (var optimized in solution.Optimized)
                        {
                            var plan = new OilFieldPlan(
                                strategy,
                                optimized,
                                BeaconStrategy: null,
                                BeaconEffectCount: 0,
                                BeaconCount: 0,
                                solution.Pipes.Count);

                            yield return (groupNumber, solutionGroup.Count, plan, solution, null);
                        }
                    }
                }
                else
                {
                    foreach (var beacons in solution.BeaconSolutions)
                    {
                        foreach (var strategy in solution.Strategies)
                        {
                            foreach (var optimized in solution.Optimized)
                            {
                                var plan = new OilFieldPlan(
                                    strategy,
                                    optimized,
                                    beacons.Strategy,
                                    beacons.Effects,
                                    beacons.Beacons.Count,
                                    solution.Pipes.Count);

                                yield return (groupNumber, solutionGroup.Count, plan, solution, beacons);
                            }
                        }
                    }
                }
            }
        }
    }

    private static IEnumerable<SolutionsAndGroupNumber> GetSolutionGroups(Context context)
    {
        var originalCenterToTerminals = context.CenterToTerminals;
        var originalLocationToTerminals = context.LocationToTerminals;

        var pipesToSolutions = new Dictionary<LocationSet, SolutionsAndGroupNumber>(LocationSetComparer.Instance);
        var connectedCentersToSolutions = new Dictionary<Dictionary<Location, LocationSet>, List<Solution>>(ConnectedCentersComparer.Instance);

        if (context.CenterToTerminals.Count == 1)
        {
            var terminal = context
                .CenterToTerminals
                .Single()
                .Value
                .OrderBy(x => x.Direction)
                .First();
            EliminateOtherTerminals(context, terminal);
            var pipes = context.GetLocationSet(terminal.Terminal);
            var solutions = OptimizeAndAddSolutions(context, pipesToSolutions, default, pipes, centerToConnectedCenters: null);
            foreach (var solution in solutions)
            {
                solution.Strategies.Clear();
            }
        }
        else
        {
            var completedStrategies = new CountedBitArray((int)PipeStrategy.ConnectedCentersFlute + 1); // max value
            foreach (var strategy in context.Options.PipeStrategies)
            {
                if (completedStrategies[(int)strategy])
                {
                    continue;
                }

                context.CenterToTerminals = originalCenterToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
                context.LocationToTerminals = originalLocationToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());

                switch (strategy)
                {
                    case PipeStrategy.FbeOriginal:
                    case PipeStrategy.Fbe:
                        {
                            (var pipes, var finalStrategy) = ExecuteWithFbe(context, strategy);
                            completedStrategies[(int)finalStrategy] = true;

                            OptimizeAndAddSolutions(context, pipesToSolutions, finalStrategy, pipes, centerToConnectedCenters: null);
                        }
                        break;

                    case PipeStrategy.ConnectedCentersDelaunay:
                    case PipeStrategy.ConnectedCentersDelaunayMst:
                    case PipeStrategy.ConnectedCentersFlute:
                        {
                            Dictionary<Location, LocationSet> centerToConnectedCenters = GetConnectedPumpjacks(context, strategy);
                            completedStrategies[(int)strategy] = true;

                            if (connectedCentersToSolutions.TryGetValue(centerToConnectedCenters, out var solutions))
                            {
                                foreach (var solution in solutions)
                                {
                                    solution.Strategies.Add(strategy);
                                }
                                continue;
                            }

                            var pipes = FindTrunksAndConnect(context, centerToConnectedCenters);
                            solutions = OptimizeAndAddSolutions(context, pipesToSolutions, strategy, pipes, centerToConnectedCenters);
                            connectedCentersToSolutions.Add(centerToConnectedCenters, solutions);
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        return pipesToSolutions.Values;
    }

    private static List<Solution> OptimizeAndAddSolutions(
        Context context,
        Dictionary<LocationSet, SolutionsAndGroupNumber> pipesToSolutions,
        PipeStrategy strategy,
        LocationSet pipes,
        Dictionary<Location, LocationSet>? centerToConnectedCenters)
    {
        SolutionsAndGroupNumber? solutionsAndIndex;
        if (pipesToSolutions.TryGetValue(pipes, out solutionsAndIndex))
        {
            foreach (var solution in solutionsAndIndex.Solutions)
            {
                solution.Strategies.Add(strategy);
            }

            return solutionsAndIndex.Solutions;
        }

        // Visualizer.Show(context.Grid, pipes.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());

        var originalCenterToTerminals = context.CenterToTerminals;
        var originalLocationToTerminals = context.LocationToTerminals;

        LocationSet optimizedPipes = pipes;
        if (context.Options.OptimizePipes)
        {
            context.CenterToTerminals = originalCenterToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            context.LocationToTerminals = originalLocationToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            optimizedPipes = new LocationSet(pipes);
            RotateOptimize.Execute(context, optimizedPipes);

            // Visualizer.Show(context.Grid, optimizedPipes.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
        }

        List<Solution> solutions;
        if (pipes.SetEquals(optimizedPipes))
        {
            optimizedPipes = context.Options.UseUndergroundPipes ? new LocationSet(pipes) : pipes;
            solutions = new List<Solution>
            {
                GetSolution(context, strategy, optimized: false, centerToConnectedCenters, optimizedPipes)
            };

            if (context.Options.OptimizePipes)
            {
                solutions[0].Optimized.Add(true);
            }
        }
        else
        {
            var solutionA = GetSolution(context, strategy, optimized: true, centerToConnectedCenters, optimizedPipes);

            context.CenterToTerminals = originalCenterToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            context.LocationToTerminals = originalLocationToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            var pipesB = context.Options.UseUndergroundPipes ? new LocationSet(pipes) : pipes;
            var solutionB = GetSolution(context, strategy, optimized: false, centerToConnectedCenters, pipesB);

            Validate.PipesDoNotMatch(context, solutionA.Pipes, solutionA.UndergroundPipes, solutionB.Pipes, solutionB.UndergroundPipes);

            solutions = new List<Solution> { solutionA, solutionB };
        }

        pipesToSolutions.Add(pipes, new SolutionsAndGroupNumber(solutions, pipesToSolutions.Count + 1));

        return solutions;
    }

    private record SolutionsAndGroupNumber(List<Solution> Solutions, int GroupNumber);

    private static Solution GetSolution(
        Context context,
        PipeStrategy strategy,
        bool optimized,
        Dictionary<Location, LocationSet>? centerToConnectedCenters,
        LocationSet optimizedPipes)
    {
        Validate.PipesAreConnected(context, optimizedPipes);

        Dictionary<Location, Direction>? undergroundPipes = null;
        if (context.Options.UseUndergroundPipes)
        {
            undergroundPipes = PlanUndergroundPipes.Execute(context, optimizedPipes);
        }

        List<BeaconSolution>? beaconSolutions = null;
        if (context.Options.AddBeacons)
        {
            beaconSolutions = PlanBeacons.Execute(context, optimizedPipes);
        }

        Validate.NoOverlappingEntities(context, optimizedPipes, undergroundPipes, beaconSolutions);

        // Visualizer.Show(context.Grid, optimizedPipes.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        return new Solution
        {
            Strategies = new List<PipeStrategy> { strategy },
            Optimized = new List<bool> { optimized },
            CenterToConnectedCenters = centerToConnectedCenters,
            CenterToTerminals = context.CenterToTerminals,
            LocationToTerminals = context.LocationToTerminals,
            Pipes = optimizedPipes,
            UndergroundPipes = undergroundPipes,
            BeaconSolutions = beaconSolutions,
        };
    }

    private static void EliminateStrandedTerminals(Context context)
    {
        var locationsToExplore = context.LocationToTerminals.Keys.ToSet(context);

        while (locationsToExplore.Count > 0)
        {
            var goals = new LocationSet(locationsToExplore);
            var start = goals.EnumerateItems().First();
            goals.Remove(start);

            var result = Dijkstras.GetShortestPaths(context, context.Grid, start, goals, stopOnFirstGoal: false);

            var reachedTerminals = result.ReachedGoals;
            reachedTerminals.Add(start);

            var unreachedTerminals = goals.EnumerateItems().Except(result.ReachedGoals.EnumerateItems()).ToSet(context);

            var reachedPumpjacks = result.ReachedGoals.EnumerateItems().SelectMany(l => context.LocationToTerminals[l]).Select(t => t.Center).ToSet(context);

            LocationSet terminalsToEliminate;
            if (reachedPumpjacks.Count == context.CenterToTerminals.Count)
            {
                terminalsToEliminate = unreachedTerminals;
                locationsToExplore.Clear();
            }
            else
            {
                terminalsToEliminate = reachedTerminals;
                locationsToExplore = unreachedTerminals;
            }

            Location strandedTerminal = default;
            bool foundStranded = false;
            foreach (var location in terminalsToEliminate.EnumerateItems())
            {
                foreach (var terminal in context.LocationToTerminals[location])
                {
                    var terminals = context.CenterToTerminals[terminal.Center];
                    terminals.Remove(terminal);

                    if (terminals.Count == 0)
                    {
                        strandedTerminal = terminal.Terminal;
                        foundStranded = true;
                    }
                }

                context.LocationToTerminals.Remove(location);
            }
            
            if (foundStranded)
            {
                /*
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, new LocationSet(), undergroundPipes: null, allowMultipleTerminals: true);
                Visualizer.Show(clone, new[] { strandedTerminal.Value, locationsToExplore.First() }.Select(x => (IPoint)new Point(x.X, x.Y)), Array.Empty<IEdge>());
                */

                throw new NoPathBetweenTerminalsException(strandedTerminal, locationsToExplore.EnumerateItems().First());
            }
        }
    }

    private class Solution
    {
        public required List<PipeStrategy> Strategies { get; set; }
        public required List<bool> Optimized { get; set; }
        public required Dictionary<Location, LocationSet>? CenterToConnectedCenters { get; set; }
        public required Dictionary<Location, List<TerminalLocation>> CenterToTerminals { get; set; }
        public required Dictionary<Location, List<TerminalLocation>> LocationToTerminals { get; set; }
        public required LocationSet Pipes { get; set; }
        public required Dictionary<Location, Direction>? UndergroundPipes { get; set; }
        public required List<BeaconSolution>? BeaconSolutions { get; set; }
    }

    private class LocationSetComparer : IEqualityComparer<LocationSet>
    {
        public static readonly LocationSetComparer Instance = new LocationSetComparer();

        public bool Equals(LocationSet? x, LocationSet? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if (x is null)
            {
                return false;
            }

            if (y is null)
            {
                return false;
            }

            if (x.Count != y.Count)
            {
                return false;
            }

            return x.SetEquals(y);
        }

        public int GetHashCode([DisallowNull] LocationSet obj)
        {
            var hashCode = new HashCode();

            foreach (var l in obj.EnumerateItems().OrderBy(p => p.X).ThenBy(p => p.Y))
            {
                hashCode.Add(l);
            }

            return hashCode.ToHashCode();
        }
    }

    private class ConnectedCentersComparer : IEqualityComparer<Dictionary<Location, LocationSet>>
    {
        public static readonly ConnectedCentersComparer Instance = new ConnectedCentersComparer();

        public bool Equals(Dictionary<Location, LocationSet>? x, Dictionary<Location, LocationSet>? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if (x is null)
            {
                return false;
            }

            if (y is null)
            {
                return false;
            }

            if (x.Keys.Count != y.Keys.Count)
            {
                return false;
            }

            foreach ((var key, var xValue) in x)
            {
                if (!y.TryGetValue(key, out var yValue))
                {
                    return false;
                }

                if (!xValue.SetEquals(yValue))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode([DisallowNull] Dictionary<Location, LocationSet> obj)
        {
            var hashCode = new HashCode();

            foreach ((var key, var value) in obj.OrderBy(p => p.Key.X).ThenBy(p => p.Key.Y))
            {
                hashCode.Add(key);
                foreach (var l in value.EnumerateItems().OrderBy(p => p.X).ThenBy(p => p.Y))
                {
                    hashCode.Add(l);
                }
            }

            return hashCode.ToHashCode();
        }
    }
}
