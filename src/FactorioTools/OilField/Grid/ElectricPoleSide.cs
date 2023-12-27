namespace Knapcode.FactorioTools.OilField.Grid;

public class ElectricPoleSide : GridEntity
{
    public ElectricPoleSide(ElectricPoleCenter center)
    {
        Center = center;
    }

    public ElectricPoleCenter Center { get; }

#if ENABLE_VISUALIZER
    public override string Label => "e";
#endif
}