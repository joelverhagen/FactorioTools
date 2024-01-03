namespace Knapcode.FactorioTools.OilField;

/// <summary>
/// An entity (e.g. a pumpjack) that receives the effect of a provider entity (e.g. electric pole, beacon).
/// </summary>
public class ProviderRecipient
{
    public ProviderRecipient(Location center, int width, int height)
    {
        Center = center;
        Width = width;
        Height = height;
    }

    public Location Center { get; }
    public int Width { get; }
    public int Height { get; }
}
