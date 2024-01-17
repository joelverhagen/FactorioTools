using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        var sortedPlans = GetAllPlans(context);
        sortedPlans.Sort((a, b) =>
        {
            // more effects = better
            var c = b.Plan.BeaconEffectCount.CompareTo(a.Plan.BeaconEffectCount);
            if (c != 0)
            {
                return c;
            }

            // fewer beacons = better (less power)
            c = a.Plan.BeaconCount.CompareTo(b.Plan.BeaconCount);
            if (c != 0)
            {
                return c;
            }

            // fewer pipes = better
            c = a.Plan.PipeCount.CompareTo(b.Plan.PipeCount);
            if (c != 0)
            {
                return c;
            }

            // prefer solutions that more algorithms find
            c = b.GroupSize.CompareTo(a.GroupSize);
            if (c != 0)
            {
                return c;
            }

            // the rest of the sorting is for arbitrary tie breaking
            c = a.Plan.PipeStrategy.CompareTo(b.Plan.PipeStrategy);
            if (c != 0)
            {
                return c;
            }

            c = a.Plan.OptimizePipes.CompareTo(b.Plan.OptimizePipes);
            if (c != 0)
            {
                return c;
            }

            c = Comparer<BeaconStrategy?>.Default.Compare(a.Plan.BeaconStrategy, b.Plan.BeaconStrategy);
            if (c != 0)
            {
                return c;
            }

            return a.GroupNumber.CompareTo(b.GroupNumber);
        });

        PlanInfo? bestPlanInfo = null;
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

            var (bestGroupNumber, _, bestPlan, _, _) = bestPlanInfo;
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

        return (selectedPlans, alternatePlans, unusedPlans, bestPlanInfo.Pipes, bestPlanInfo.Beacons);
    }


    private static List<PlanInfo> GetAllPlans(Context context)
    {
        var solutionGroups = GetSolutionGroups(context);
        var plans = new List<PlanInfo>();
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
                                solution.Pipes.Count,
                                solution.PipeCountWithoutUnderground);

                            plans.Add(new PlanInfo(groupNumber, solutionGroup.Count, plan, solution, null));
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
                                    solution.Pipes.Count,
                                    solution.PipeCountWithoutUnderground);

                                plans.Add(new PlanInfo(groupNumber, solutionGroup.Count, plan, solution, beacons));
                            }
                        }
                    }
                }
            }
        }

        return plans;
    }

    private static IReadOnlyCollection<SolutionsAndGroupNumber> GetSolutionGroups(Context context)
    {
        var originalCenterToTerminals = context.CenterToTerminals;
        var originalLocationToTerminals = context.LocationToTerminals;

        var pipesToSolutions = new Dictionary<ILocationSet, SolutionsAndGroupNumber>(LocationSetComparer.Instance);
        var connectedCentersToSolutions = new Dictionary<ILocationDictionary<ILocationSet>, List<Solution>>(ConnectedCentersComparer.Instance);

        if (context.CenterToTerminals.Count == 1)
        {
            var terminal = context.CenterToTerminals.EnumeratePairs().Single().Value[0];
            EliminateOtherTerminals(context, terminal);
            var pipes = context.GetSingleLocationSet(terminal.Terminal);
            var solutions = OptimizeAndAddSolutions(context, pipesToSolutions, default, pipes, centerToConnectedCenters: null);
            var solution = solutions.Single();
            solution.Strategies.Clear();
            solution.Strategies.AddRange(context.Options.PipeStrategies);
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

                context.CenterToTerminals = originalCenterToTerminals.EnumeratePairs().ToDictionary(context, x => x.Key, x => x.Value.ToList());
                context.LocationToTerminals = originalLocationToTerminals.EnumeratePairs().ToDictionary(context, x => x.Key, x => x.Value.ToList());

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
                            ILocationDictionary<ILocationSet> centerToConnectedCenters = GetConnectedPumpjacks(context, strategy);
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
        Dictionary<ILocationSet, SolutionsAndGroupNumber> pipesToSolutions,
        PipeStrategy strategy,
        ILocationSet pipes,
        ILocationDictionary<ILocationSet>? centerToConnectedCenters)
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

        ILocationSet optimizedPipes = pipes;
        if (context.Options.OptimizePipes)
        {
            context.CenterToTerminals = originalCenterToTerminals.EnumeratePairs().ToDictionary(context, x => x.Key, x => x.Value.ToList());
            context.LocationToTerminals = originalLocationToTerminals.EnumeratePairs().ToDictionary(context, x => x.Key, x => x.Value.ToList());
            optimizedPipes = context.GetLocationSet(pipes);
            RotateOptimize.Execute(context, optimizedPipes);

            // Visualizer.Show(context.Grid, optimizedPipes.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
        }

        List<Solution> solutions;
        if (pipes.SetEquals(optimizedPipes))
        {
            optimizedPipes = context.Options.UseUndergroundPipes ? context.GetLocationSet(pipes) : pipes;
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

            context.CenterToTerminals = originalCenterToTerminals.EnumeratePairs().ToDictionary(context, x => x.Key, x => x.Value.ToList());
            context.LocationToTerminals = originalLocationToTerminals.EnumeratePairs().ToDictionary(context, x => x.Key, x => x.Value.ToList());
            var pipesB = context.Options.UseUndergroundPipes ? context.GetLocationSet(pipes) : pipes;
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
        ILocationDictionary<ILocationSet>? centerToConnectedCenters,
        ILocationSet optimizedPipes)
    {
        Validate.PipesAreConnected(context, optimizedPipes);

        var pipeCountBeforeUnderground = optimizedPipes.Count;

        ILocationDictionary<Direction>? undergroundPipes = null;
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
            PipeCountWithoutUnderground = pipeCountBeforeUnderground,
            Pipes = optimizedPipes,
            UndergroundPipes = undergroundPipes,
            BeaconSolutions = beaconSolutions,
        };
    }

    private static void EliminateStrandedTerminals(Context context)
    {
        var locationsToExplore = context.LocationToTerminals.Keys.ToReadOnlySet(context, allowEnumerate: true);

        while (locationsToExplore.Count > 0)
        {
            var goals = context.GetLocationSet(locationsToExplore);
            var start = goals.EnumerateItems().First();
            goals.Remove(start);

            var result = Dijkstras.GetShortestPaths(context, context.Grid, start, goals, stopOnFirstGoal: false, allowGoalEnumerate: true);

            var reachedTerminals = result.ReachedGoals;
            reachedTerminals.Add(start);

            var unreachedTerminals = context.GetLocationSet(goals);
            unreachedTerminals.ExceptWith(result.ReachedGoals);

            var reachedPumpjacks = context.GetLocationSet();
            foreach (var location in result.ReachedGoals.EnumerateItems())
            {
                var terminals = context.LocationToTerminals[location];
                for (var i = 0; i < terminals.Count; i++)
                {
                    reachedPumpjacks.Add(terminals[i].Center);
                }
            }

            ILocationSet terminalsToEliminate;
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

            Location strandedTerminal = Location.Invalid;
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
                AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, new ILocationSet(), undergroundPipes: null, allowMultipleTerminals: true);
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
        public required ILocationDictionary<ILocationSet>? CenterToConnectedCenters { get; set; }
        public required ILocationDictionary<List<TerminalLocation>> CenterToTerminals { get; set; }
        public required ILocationDictionary<List<TerminalLocation>> LocationToTerminals { get; set; }
        public required int PipeCountWithoutUnderground { get; set; }
        public required ILocationSet Pipes { get; set; }
        public required ILocationDictionary<Direction>? UndergroundPipes { get; set; }
        public required List<BeaconSolution>? BeaconSolutions { get; set; }
    }

    private class LocationSetComparer : IEqualityComparer<ILocationSet>
    {
        public static readonly LocationSetComparer Instance = new LocationSetComparer();

        public bool Equals(ILocationSet? x, ILocationSet? y)
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

        public int GetHashCode([DisallowNull] ILocationSet obj)
        {
            var sumX = 0;
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var sumY = 0;
            var minY = int.MaxValue;
            var maxY = int.MinValue;

            foreach (var l in obj.EnumerateItems())
            {
                sumX += l.X;

                if (l.X < minX)
                {
                    minX = l.X;
                }

                if (l.X > maxX)
                {
                    maxX = l.X;
                }

                sumY += l.Y;

                if (l.Y < minY)
                {
                    minY = l.Y;
                }

                if (l.Y > maxY)
                {
                    maxY = l.Y;
                }
            }

            var hash = 17;
            hash = hash * 23 + obj.Count;
            hash = hash * 23 + sumX;
            hash = hash * 23 + minX;
            hash = hash * 23 + maxX;
            hash = hash * 23 + sumY;
            hash = hash * 23 + minY;
            hash = hash * 23 + maxY;

            return hash;
        }
    }

    private class ConnectedCentersComparer : IEqualityComparer<ILocationDictionary<ILocationSet>>
    {
        public static readonly ConnectedCentersComparer Instance = new ConnectedCentersComparer();

        public bool Equals(ILocationDictionary<ILocationSet>? x, ILocationDictionary<ILocationSet>? y)
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

            foreach ((var key, var xValue) in x.EnumeratePairs())
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

        public int GetHashCode([DisallowNull] ILocationDictionary<ILocationSet> obj)
        {
            var sumX = 0;
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var sumY = 0;
            var minY = int.MaxValue;
            var maxY = int.MinValue;
            var locationSum = 0;

            foreach (var (l, s) in obj.EnumeratePairs())
            {
                sumX += l.X;

                if (l.X < minX)
                {
                    minX = l.X;
                }

                if (l.X > maxX)
                {
                    maxX = l.X;
                }

                sumY += l.Y;

                if (l.Y < minY)
                {
                    minY = l.Y;
                }

                if (l.Y > maxY)
                {
                    maxY = l.Y;
                }

                locationSum = s.Count;
            }

            var hash = 17;
            hash = hash * 23 + obj.Count;
            hash = hash * 23 + sumX;
            hash = hash * 23 + minX;
            hash = hash * 23 + maxX;
            hash = hash * 23 + sumY;
            hash = hash * 23 + minY;
            hash = hash * 23 + maxY;
            hash = hash * 23 + locationSum;

            return hash;
        }
    }

    private record PlanInfo(int GroupNumber, int GroupSize, OilFieldPlan Plan, Solution Pipes, BeaconSolution? Beacons);
}
