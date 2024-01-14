namespace Knapcode.FactorioTools.OilField;

public class PumpjackCenter : GridEntity
{
    public PumpjackCenter(int id) : base(id)
    {
    }

#if ENABLE_GRID_TOSTRING
    public override string Label => "J";
#endif
}
