using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class AddBeacons
{
    public static void Execute(Context context)
    {
        var poweredEntities = context.CenterToTerminals.Keys.Select(c => new ProviderRecipients(c, Width: 3, Height: 3)).ToList();
        var candidateToCovered = GetCandidateToCovered(
            context,
            poweredEntities,
            context.Options.BeaconWidth,
            context.Options.BeaconHeight,
            context.Options.BeaconSupplyWidth,
            context.Options.BeaconSupplyHeight);
    }
}
