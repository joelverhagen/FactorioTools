namespace Knapcode.FactorioTools.OilField;

/// <summary>
/// The strategy to use when planning beacon placement around the pumpjacks.
/// </summary>
public enum BeaconStrategy
{
    /// <summary>
    /// The original Factorio Blueprint Editor (FBE) beacon planning strategy.
    /// </summary>
    FbeOriginal,

    /// <summary>
    /// A modified Factorio Blueprint Editor (FBE) beacon planning strategy.
    /// </summary>
    Fbe,

    /// <summary>
    /// A beacon planning strategy that attempts to make beacons as close ("snug") to each other as possible.
    /// </summary>
    Snug,
}