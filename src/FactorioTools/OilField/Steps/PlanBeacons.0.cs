using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

public static partial class PlanBeacons
{
    public static List<Location> Execute(Context context, HashSet<Location> pipes)
    {
        foreach (var pipe in pipes)
        {
            context.Grid.AddEntity(pipe, new TemporaryEntity());
        }

        var solutions = new List<Solution>(context.Options.BeaconStrategies.Count);

        foreach (var strategy in context.Options.BeaconStrategies)
        {
            var beacons = strategy switch
            {
                BeaconStrategy.FBE_Original => AddBeacons_FBE(context, strategy),
                BeaconStrategy.FBE => AddBeacons_FBE(context, strategy),
                BeaconStrategy.Snug => AddBeacons_Snug(context),
                _ => throw new NotImplementedException(),
            };

            solutions.Add(new Solution(strategy, beacons));
        }

        foreach (var pipe in pipes)
        {
            context.Grid.RemoveEntity(pipe);
        }

        if (solutions.Count == 0)
        {
            throw new InvalidOperationException("At least one beacon strategy must be used.");
        }

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
                        throw new InvalidOperationException("Multiple beacons are providing an effect to a pumpjack.");
                    }
                }
            }
        }

        return solutions.MaxBy(s => s.Beacons.Count)!.Beacons;
    }

    private record Solution(BeaconStrategy Strategy, List<Location> Beacons);
}