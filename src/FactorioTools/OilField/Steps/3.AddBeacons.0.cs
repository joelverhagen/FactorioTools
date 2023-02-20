using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddBeacons
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
                BeaconStrategy.FBE => AddBeacons_FBE(context),
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

        return solutions.MaxBy(s => s.Beacons.Count)!.Beacons;
    }

    private record Solution(BeaconStrategy Strategy, List<Location> Beacons);
}