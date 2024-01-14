namespace Knapcode.FactorioTools.OilField;

public class PumpjackSide : GridEntity
{
    public PumpjackSide(int id, PumpjackCenter center) : base(id)
    {
        Center = center;
    }

    public PumpjackCenter Center { get; }

#if ENABLE_GRID_TOSTRING
    public override string Label => "j";
#endif
}
