namespace Knapcode.FactorioTools.OilField.Grid;

public class PumpjackSide : GridEntity
{
    public PumpjackSide(PumpjackCenter center)
    {
        Center = center;
    }

    public PumpjackCenter Center { get; }

#if ENABLE_VISUALIZER
    public override string Label => "j";
#endif
}
