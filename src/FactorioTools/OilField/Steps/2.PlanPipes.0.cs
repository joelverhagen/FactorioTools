using Knapcode.FactorioTools.OilField.Algorithms;
using Knapcode.FactorioTools.OilField.Grid;
using System.Diagnostics.CodeAnalysis;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddPipes
{
    public static HashSet<Location> Execute(Context context, bool eliminateStrandedTerminals)
    {
        var originalCenterToTerminals = context.CenterToTerminals;
        var originalLocationToTerminals = context.LocationToTerminals;

        var solutions = new List<Solution>();
        var connectedCentersToSolutions = new Dictionary<Dictionary<Location, HashSet<Location>>, Solution>();

        if (eliminateStrandedTerminals)
        {
            EliminateStrandedTerminals(context);
        }

        foreach (var strategy in Enum.GetValues<PlanPipesStrategy>())
        {
            context.CenterToTerminals = originalCenterToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());
            context.LocationToTerminals = originalLocationToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());

            switch (strategy)
            {
                case PlanPipesStrategy.FBE:
                    {
                        var pipes = ExecuteWithFBE(context);

                        var optimizedPipes = RotateOptimize.Execute(context, pipes);

                        solutions.Add(new Solution
                        {
                            Strategies = new HashSet<PlanPipesStrategy> { strategy },
                            CenterToConnectedCenters = null,
                            CenterToTerminals = context.CenterToTerminals,
                            LocationToTerminals = context.LocationToTerminals,
                            Pipes = optimizedPipes,
                        });
                    }
                    break;

                case PlanPipesStrategy.ConnectedCenters_Delaunay:
                case PlanPipesStrategy.ConnectedCenters_DelaunayMst:
                case PlanPipesStrategy.ConnectedCenters_FLUTE:
                    {
                        Dictionary<Location, HashSet<Location>> centerToConnectedCenters = GetConnectedPumpjacks(context, strategy);
                        if (connectedCentersToSolutions.TryGetValue(centerToConnectedCenters, out var solution))
                        {
                            solution.Strategies.Add(strategy);
                            continue;
                        }

                        var pipes = FindTrunksAndConnect(context, centerToConnectedCenters);

                        var optimizedPipes = RotateOptimize.Execute(context, pipes);

                        solution = new Solution
                        {
                            Strategies = new HashSet<PlanPipesStrategy> { strategy },
                            CenterToConnectedCenters = centerToConnectedCenters,
                            CenterToTerminals = context.CenterToTerminals,
                            LocationToTerminals = context.LocationToTerminals,
                            Pipes = optimizedPipes,
                        };

                        solutions.Add(solution);
                        connectedCentersToSolutions.Add(centerToConnectedCenters, solution);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        var bestSolution = solutions.MinBy(s => s.Pipes.Count)!;
        context.CenterToTerminals = bestSolution.CenterToTerminals;

        AddPipeEntities.Execute(context.Grid, context.CenterToTerminals, bestSolution.Pipes);

        return bestSolution.Pipes;
    }

    private static void EliminateStrandedTerminals(Context context)
    {
        var locationsToExplore = context.LocationToTerminals.Keys.ToHashSet();

        while (locationsToExplore.Count > 0)
        {
            var goals = new HashSet<Location>(locationsToExplore);
            var start = goals.First();
            goals.Remove(start);

            var result = Dijkstras.GetShortestPaths(context.Grid, start, goals, stopOnFirstGoal: false);

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

    private static void EliminateOtherTerminals(Context context, TerminalLocation selectedTerminal)
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

    private enum PlanPipesStrategy
    {
        FBE,
        ConnectedCenters_Delaunay,
        ConnectedCenters_DelaunayMst,
        ConnectedCenters_FLUTE,
    }

    private class Solution
    {
        public required HashSet<PlanPipesStrategy> Strategies { get; set; }
        public required Dictionary<Location, HashSet<Location>>? CenterToConnectedCenters { get; set; }
        public required IReadOnlyDictionary<Location, List<TerminalLocation>> CenterToTerminals { get; set; }
        public required Dictionary<Location, List<TerminalLocation>> LocationToTerminals { get; set; }
        public required HashSet<Location> Pipes { get; set; }
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
