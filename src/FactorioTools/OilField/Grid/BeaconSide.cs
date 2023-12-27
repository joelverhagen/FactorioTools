namespace Knapcode.FactorioTools.OilField.Grid;

public class BeaconSide : GridEntity
{
    public BeaconSide(BeaconCenter center)
    {
        Center = center;
    }

    public BeaconCenter Center { get; }

#if ENABLE_VISUALIZER
    public override string Label => "b";
#endif
}
