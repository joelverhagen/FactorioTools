namespace Knapcode.FactorioTools.OilField;

/// <summary>
/// An entity (e.g. a pumpjack) that receives the effect of a provider entity (e.g. electric pole, beacon).
/// </summary>
public class ProviderRecipient(Location center, int width, int height)
{
    public Location Center { get; } = center;
    public int Width { get; } = width;
    public int Height { get; } = height;
}
