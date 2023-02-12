namespace Knapcode.FactorioTools.OilField.Grid;

internal class BeaconSide : GridEntity
{
    public BeaconSide(BeaconCenter center)
    {
        Center = center;
    }

    public BeaconCenter Center { get; }

    public override string Label => "b";
}
