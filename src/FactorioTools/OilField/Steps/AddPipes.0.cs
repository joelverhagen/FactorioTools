using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;
using System.Diagnostics.CodeAnalysis;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

public static partial class AddPipes
{
    public static void Execute(Context context, bool eliminateStrandedTerminals)
    {
        var originalCenterToTerminals = context.CenterToTerminals;
        var originalLocationToTerminals = context.LocationToTerminals;

        var pipesToSolution = new Dictionary<HashSet<Location>, Solution>(LocationSetComparer.Instance);
        var connectedCentersToSolutions = new Dictionary<Dictionary<Location, HashSet<Location>>, Solution>(ConnectedCentersComparer.Instance);

        if (eliminateStrandedTerminals)
        {
            EliminateStrandedTerminals(context);
        }

        if (context.CenterToTerminals.Count == 1)
        {
            var terminal = context
                .CenterToTerminals
                .Single()
                .Value
                .OrderBy(x => x.Direction)
                .First();
            EliminateOtherTerminals(context, terminal);
            var pipes = new HashSet<Location> { terminal.Terminal };
            var solution = OptimizeAndAddSolution(context, pipesToSolution, default, pipes, centerToConnectedCenters: null);
            solution.Strategies.Clear();
        }
        else
        {
            foreach (var strategy in context.Options.PipeStrategies)
            {
                context.CenterToTerminals = originalCenterToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
                context.LocationToTerminals = originalLocationToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());

                switch (strategy)
                {
                    case PipeStrategy.FBE:
                        {
                            var pipes = ExecuteWithFBE(context);
                            OptimizeAndAddSolution(context, pipesToSolution, strategy, pipes, centerToConnectedCenters: null);
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
                            solution = OptimizeAndAddSolution(context, pipesToSolution, strategy, pipes, centerToConnectedCenters);
                            connectedCentersToSolutions.Add(centerToConnectedCenters, solution);
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
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
            // Visualizer.Show(context.Grid, bestSolution.Beacons.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());
            AddBeaconsToGrid(context.Grid, context.Options, bestSolution.Beacons);
        }
    }

    private static Solution OptimizeAndAddSolution(
        Context context,
        Dictionary<HashSet<Location>, Solution> pipesToSolutions,
        PipeStrategy strategy,
        HashSet<Location> pipes,
        Dictionary<Location, HashSet<Location>>? centerToConnectedCenters)
    {
        Solution? solution;
        if (pipesToSolutions.TryGetValue(pipes, out solution))
        {
            solution.Strategies.Add(strategy);
            return solution;
        }

        // Visualizer.Show(context.Grid, pipes.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        var originalCenterToTerminals = context.CenterToTerminals;
        var originalLocationToTerminals = context.LocationToTerminals;

        HashSet<Location> optimizedPipes = pipes;
        if (context.Options.OptimizePipes)
        {
            context.CenterToTerminals = originalCenterToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            context.LocationToTerminals = originalLocationToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            optimizedPipes = new HashSet<Location>(pipes);
            RotateOptimize.Execute(context, optimizedPipes);
        }

        if (pipes.SetEquals(optimizedPipes))
        {
            optimizedPipes = context.Options.UseUndergroundPipes ? new HashSet<Location>(pipes) : pipes;
            solution = GetSolution(context, strategy, centerToConnectedCenters, optimizedPipes);
        }
        else
        {
            var solutionA = GetSolution(context, strategy, centerToConnectedCenters, optimizedPipes);

            context.CenterToTerminals = originalCenterToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            context.LocationToTerminals = originalLocationToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            var pipesB = context.Options.UseUndergroundPipes ? new HashSet<Location>(pipes) : pipes;
            var solutionB = GetSolution(context, strategy, centerToConnectedCenters, pipesB);

            if (context.Options.AddBeacons)
            {
                var c = solutionA.Beacons!.Count.CompareTo(solutionB.Beacons!.Count);
                if (c > 0)
                {
                    solution = solutionA;
                }
                else if (c < 0)
                {
                    solution = solutionB;
                }
                else
                {
                    solution = solutionA.Pipes.Count <= solutionB.Pipes.Count ? solutionA : solutionB;
                }
            }
            else
            {
                solution = solutionA.Pipes.Count <= solutionB.Pipes.Count ? solutionA : solutionB;
            }
        }

        pipesToSolutions.Add(pipes, solution);

        return solution;
    }

    private static Solution GetSolution(
        Context context,
        PipeStrategy strategy,
        Dictionary<Location, HashSet<Location>>? centerToConnectedCenters,
        HashSet<Location> optimizedPipes)
    {
        Solution? solution;
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
                // Visualizer.Show(context.Grid, optimizedPipes.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());
                throw new InvalidOperationException("The pipes are not fully connected.");
            }
        }

        Dictionary<Location, Direction>? undergroundPipes = null;
        if (context.Options.UseUndergroundPipes)
        {
            undergroundPipes = PlanUndergroundPipes.Execute(context, optimizedPipes);
        }

        List<Location>? beacons = null;
        if (context.Options.AddBeacons)
        {
            beacons = PlanBeacons.Execute(context, optimizedPipes);
        }

        if (context.Options.ValidateSolution)
        {
            var clone = new PipeGrid(context.Grid);
            AddPipeEntities.Execute(clone, context.SharedInstances, context.CenterToTerminals, optimizedPipes, undergroundPipes);
            if (beacons is not null)
            {
                AddBeaconsToGrid(clone, context.Options, beacons);
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

    private static void AddBeaconsToGrid(SquareGrid grid, OilFieldOptions options, IEnumerable<Location> centers)
    {
        foreach (var center in centers)
        {
            AddProvider(
                grid,
                center,
                new BeaconCenter(),
                c => new BeaconSide(c),
                options.BeaconWidth,
                options.BeaconHeight);
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
