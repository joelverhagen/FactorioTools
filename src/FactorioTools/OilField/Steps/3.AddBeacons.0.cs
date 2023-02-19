using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddBeacons
{
    public static void Execute(Context context)
    {
        var solutions = new List<Solution>(context.Options.BeaconStrategies.Count);

        foreach (var strategy in context.Options.BeaconStrategies)
        {
            var beacons = strategy switch
            {
                BeaconStrategy.FBE => AddBeacons_FBE(context),
                BeaconStrategy.Snug => AddBeacons_Snug(context),
                _ => throw new NotImplementedException(),
            };

            solutions.Add(new Solution(strategy, beacons));
        }

        if (solutions.Count == 0)
        {
            throw new InvalidOperationException("At least one beacon strategy must be used.");
        }

        var bestSolution = solutions.MaxBy(s => s.Beacons.Count)!;
        AddBeaconsToGrid(context, bestSolution.Beacons);

        // Visualizer.Show(context.Grid);
    }

    private record Solution(BeaconStrategy Strategy, List<Location> Beacons);

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
}