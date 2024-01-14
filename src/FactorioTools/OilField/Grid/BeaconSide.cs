namespace Knapcode.FactorioTools.OilField;

public class BeaconSide : GridEntity
{
    public BeaconSide(int id, BeaconCenter center) : base(id)
    {
        Center = center;
    }

    public BeaconCenter Center { get; }

#if ENABLE_GRID_TOSTRING
    public override string Label => "b";
#endif
}
