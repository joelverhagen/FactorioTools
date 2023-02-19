using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;
using System.Diagnostics.CodeAnalysis;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddPipes
{
    public static void Execute(Context context, bool eliminateStrandedTerminals)
    {
        var originalCenterToTerminals = context.CenterToTerminals;
        var originalLocationToTerminals = context.LocationToTerminals;

        var pipesToSolution = new Dictionary<HashSet<Location>, Solution>(LocationSetComparer.Instance);
        var connectedCentersToSolutions = new Dictionary<Dictionary<Location, HashSet<Location>>, Solution>(ConnectedCentersComparer.Instance);
        var optimizePipePlans = new Dictionary<HashSet<Location>, (HashSet<Location> OptimizedPipes, Dictionary<Location, Direction>? UndergroundPipes)>(LocationSetComparer.Instance);
        var beaconPlans = new Dictionary<HashSet<Location>, List<Location>>(LocationSetComparer.Instance);

        if (eliminateStrandedTerminals)
        {
            EliminateStrandedTerminals(context);
        }

        foreach (var strategy in context.Options.PipeStrategies)
        {
            context.CenterToTerminals = originalCenterToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            context.LocationToTerminals = originalLocationToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());

            switch (strategy)
            {
                case PipeStrategy.FBE:
                    {
                        var pipes = ExecuteWithFBE(context);
                        OptimizeAndAddSolution(context, pipesToSolution, optimizePipePlans, beaconPlans, strategy, pipes, centerToConnectedCenters: null);
                    }
                    break;

                case PipeStrategy.ConnectedCenters_Delaunay:
                case PipeStrategy.ConnectedCenters_DelaunayMst:
                case PipeStrategy.ConnectedCenters_FLUTE:
                    {
                        Dictionary<Location, HashSet<Location>> centerToConnectedCenters = GetConnectedPumpjacks(context, strategy);
                        if (connectedCentersToSolutions.TryGetValue(centerToConnectedCenters, out var solution))
                        {
                            solution.Strategies.Add(strategy);
                            continue;
                        }

                        var pipes = FindTrunksAndConnect(context, centerToConnectedCenters);
                        solution = OptimizeAndAddSolution(context, pipesToSolution, optimizePipePlans, beaconPlans, strategy, pipes, centerToConnectedCenters);
                        connectedCentersToSolutions.Add(centerToConnectedCenters, solution);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        if (pipesToSolution.Count == 0)
        {
            throw new InvalidOperationException("At least one pipe strategy must be used.");
        }

        var bestSolution = pipesToSolution.Values.MaxBy(s => (s.Beacons?.Count, -s.Pipes.Count))!;

        context.CenterToTerminals = bestSolution.CenterToTerminals;
        context.LocationToTerminals = bestSolution.LocationToTerminals;

        AddPipeEntities.Execute(context, bestSolution.Pipes, bestSolution.UndergroundPipes);
        if (bestSolution.Beacons is not null)
        {
            AddBeaconsToGrid(context, bestSolution.Beacons);
        }
    }

    private static Solution OptimizeAndAddSolution(
        Context context,
        Dictionary<HashSet<Location>, Solution> pipesToSolutions,
        Dictionary<HashSet<Location>, (HashSet<Location> OptimizedPipes, Dictionary<Location, Direction>? UndergroundPipes)> optimizePipePlans,
        Dictionary<HashSet<Location>, List<Location>> beaconPlans,
        PipeStrategy strategy,
        HashSet<Location> pipes,
        Dictionary<Location, HashSet<Location>>? centerToConnectedCenters)
    {
        if (pipesToSolutions.TryGetValue(pipes, out var solution))
        {
            solution.Strategies.Add(strategy);
            return solution;
        }

        // Visualizer.Show(context.Grid, pipes.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        var optimizedPipes = pipes;
        Dictionary<Location, Direction>? undergroundPipes = null;

        if ((context.Options.OptimizePipes || context.Options.UseUndergroundPipes))
        {
            if (optimizePipePlans.TryGetValue(pipes, out var plan))
            {
                optimizedPipes = plan.OptimizedPipes;
                undergroundPipes = plan.UndergroundPipes;
            }
            else
            {
                if (context.Options.OptimizePipes)
                {
                    optimizedPipes = pipes == optimizedPipes ? new HashSet<Location>(pipes) : optimizedPipes;

                    RotateOptimize.Execute(context, optimizedPipes);
                }

                if (context.Options.ValidateSolution)
                {
                    foreach (var terminals in context.CenterToTerminals.Values)
                    {
                        if (terminals.Count != 1)
                        {
                            throw new InvalidOperationException("A pumpjack has more than one terminal.");
                        }
                    }

                    var goals = context.CenterToTerminals.Values.SelectMany(ts => ts).Select(t => t.Terminal).ToHashSet();
                    var clone = new ExistingPipeGrid(context.Grid, optimizedPipes);
                    var start = goals.First();
                    goals.Remove(start);
                    var result = Dijkstras.GetShortestPaths(context.SharedInstances, clone, start, goals, stopOnFirstGoal: false);
                    var reachedGoals = result.ReachedGoals;
                    reachedGoals.Add(start);
                    var unreachedGoals = goals.Except(reachedGoals).ToHashSet();
                    if (unreachedGoals.Count > 0)
                    {
                        // Visualizer.Show(context.Grid, solution.Pipes.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());
                        throw new InvalidOperationException("The pipes are not fully connected.");
                    }
                }

                if (context.Options.UseUndergroundPipes)
                {
                    optimizedPipes = pipes == optimizedPipes ? new HashSet<Location>(pipes) : optimizedPipes;

                    undergroundPipes = UseUndergroundPipes.Execute(context, optimizedPipes);
                }

                optimizePipePlans.Add(pipes, (optimizedPipes, undergroundPipes));
            }
        }

        // TODO: perf idea, cache beacons per pipes
        List<Location>? beacons = null;
        if (context.Options.AddBeacons)
        {
            if (!beaconPlans.TryGetValue(optimizedPipes, out beacons))
            {
                beacons = AddBeacons.Execute(context, optimizedPipes);
                beaconPlans.Add(optimizedPipes, beacons);
            }
        }

        // Visualizer.Show(context.Grid, optimizedPipes.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        solution = new Solution
        {
            Strategies = new HashSet<PipeStrategy> { strategy },
            CenterToConnectedCenters = centerToConnectedCenters,
            CenterToTerminals = context.CenterToTerminals,
            LocationToTerminals = context.LocationToTerminals,
            Pipes = optimizedPipes,
            UndergroundPipes = undergroundPipes,
            Beacons = beacons,
        };

        pipesToSolutions.Add(pipes, solution);

        return solution;
    }

    private static void EliminateStrandedTerminals(Context context)
    {
        var locationsToExplore = context.LocationToTerminals.Keys.ToHashSet();

        while (locationsToExplore.Count > 0)
        {
            var goals = new HashSet<Location>(locationsToExplore);
            var start = goals.First();
            goals.Remove(start);

            var result = Dijkstras.GetShortestPaths(context.SharedInstances, context.Grid, start, goals, stopOnFirstGoal: false);

            var reachedTerminals = result.ReachedGoals;
            reachedTerminals.Add(start);

            var unreachedTerminals = goals.Except(result.ReachedGoals).ToHashSet();

            var reachedPumpjacks = result.ReachedGoals.SelectMany(l => context.LocationToTerminals[l]).Select(t => t.Center).ToHashSet();

            HashSet<Location> terminalsToEliminate;
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

            foreach (var location in terminalsToEliminate)
            {
                foreach (var terminal in context.LocationToTerminals[location])
                {
                    var terminals = context.CenterToTerminals[terminal.Center];
                    terminals.Remove(terminal);
                    if (terminals.Count == 0)
                    {
                        throw new InvalidOperationException("No path can be found for any of the terminals on a pumpjack.");
                    }
                }
            }
        }
    }

    private static void AddBeaconsToGrid(Context context, IEnumerable<Location> centers)
    {
        foreach (var center in centers)
        {
            AddProvider(
                context.Grid,
                center,
                new BeaconCenter(),
                c => new BeaconSide(c),
                context.Options.BeaconWidth,
                context.Options.BeaconHeight);
        }
    }

    private class Solution
    {
        public required HashSet<PipeStrategy> Strategies { get; set; }
        public required Dictionary<Location, HashSet<Location>>? CenterToConnectedCenters { get; set; }
        public required Dictionary<Location, List<TerminalLocation>> CenterToTerminals { get; set; }
        public required Dictionary<Location, List<TerminalLocation>> LocationToTerminals { get; set; }
        public required HashSet<Location> Pipes { get; set; }
        public required Dictionary<Location, Direction>? UndergroundPipes { get; set; }
        public List<Location>? Beacons { get; set; }
    }

    private class LocationSetComparer : IEqualityComparer<HashSet<Location>>
    {
        public static readonly LocationSetComparer Instance = new LocationSetComparer();

        public bool Equals(HashSet<Location>? x, HashSet<Location>? y)
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

        public int GetHashCode([DisallowNull] HashSet<Location> obj)
        {
            var hashCode = new HashCode();
            foreach (var x in obj)
            {
                hashCode.Add(x);
            }

            return hashCode.ToHashCode();
        }
    }

    private class ConnectedCentersComparer : IEqualityComparer<Dictionary<Location, HashSet<Location>>>
    {
        public static readonly ConnectedCentersComparer Instance = new ConnectedCentersComparer();

        public bool Equals(Dictionary<Location, HashSet<Location>>? x, Dictionary<Location, HashSet<Location>>? y)
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

        public int GetHashCode([DisallowNull] Dictionary<Location, HashSet<Location>> obj)
        {
            var hashCode = new HashCode();
            foreach ((var key, var value) in obj)
            {
                hashCode.Add(key);
                foreach (var x in value)
                {
                    hashCode.Add(x);
                }
            }

            return hashCode.ToHashCode();
        }
    }
}
