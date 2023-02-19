namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddBeacons
{
    public static List<Location> Execute(Context context, HashSet<Location> pipes)
    {
        var solutions = new List<Solution>(context.Options.BeaconStrategies.Count);

        foreach (var strategy in context.Options.BeaconStrategies)
        {
            var beacons = strategy switch
            {
                BeaconStrategy.FBE => AddBeacons_FBE(context, pipes),
                BeaconStrategy.Snug => AddBeacons_Snug(context, pipes),
                _ => throw new NotImplementedException(),
            };

            solutions.Add(new Solution(strategy, beacons));
        }

        if (solutions.Count == 0)
        {
            throw new InvalidOperationException("At least one beacon strategy must be used.");
        }

        return solutions.MaxBy(s => s.Beacons.Count)!.Beacons;
    }

    private record Solution(BeaconStrategy Strategy, List<Location> Beacons);
}