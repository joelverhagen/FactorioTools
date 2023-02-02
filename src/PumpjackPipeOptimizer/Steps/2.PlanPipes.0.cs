using PumpjackPipeOptimizer.Grid;
using System.Diagnostics.CodeAnalysis;

namespace PumpjackPipeOptimizer.Steps;

internal static partial class PlanPipes
{
    public static HashSet<Location> Execute(Context context)
    {
        var originalCenterToTerminals = context.CenterToTerminals;
        var solutions = new List<Solution>();
        var connectedCentersToSolutions = new Dictionary<Dictionary<Location, HashSet<Location>>, Solution>();

        foreach (var strategy in Enum.GetValues<PlanPipesStrategy>())
        {
            context.CenterToTerminals = originalCenterToTerminals.ToDictionary(x => x.Key, x => x.Value.ToList());

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
                            CenterToTerminals = context.CenterToTerminals,
                            CenterToConnectedCenters = centerToConnectedCenters,
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
        return bestSolution.Pipes;
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
