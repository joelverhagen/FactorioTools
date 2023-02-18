using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddBeacons
{
    public static void Execute(Context context)
    {
        HashSet<Location>? fbe = null;
        if (context.Options.BeaconStrategies.Contains(BeaconStrategy.FBE))
        {
            fbe = AddBeacons_FBE(context);
        }

        Dictionary<Location, BeaconCenter>? snug = null;
        if (context.Options.BeaconStrategies.Contains(BeaconStrategy.Snug))
        {
            snug = AddBeacons_Snug(context);
        }

        if (fbe is null && snug is null)
        {
            throw new InvalidOperationException("At least one beacon strategy must be used.");
        }

        if (snug is null)
        {
            AddBeaconsToGrid(context, fbe!);
        }
        else if (fbe is not null && fbe.Count > snug.Count)
        {
            RemoveBeaconsFromGrid(context, snug.Keys);
            AddBeaconsToGrid(context, fbe);
        }
    }

    private static void RemoveBeaconsFromGrid(Context context, IEnumerable<Location> centers)
    {
        foreach (var center in centers)
        {
            RemoveProvider(
                context.Grid,
                center,
                context.Options.BeaconWidth,
                context.Options.BeaconHeight);
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
}