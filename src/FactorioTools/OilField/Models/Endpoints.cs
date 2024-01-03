namespace Knapcode.FactorioTools.OilField;

public class Endpoints
{
    public Endpoints(Location a, Location b)
    {
        A = a;
        B = b;
    }

    public Location A { get; }
    public Location B { get; }
}