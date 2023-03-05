namespace Knapcode.FactorioTools.OilField.Grid;

public class PumpjackSide : GridEntity
{
    public PumpjackSide(PumpjackCenter center)
    {
        Center = center;
    }

    public PumpjackCenter Center { get; }

    public override string Label => "j";
}
