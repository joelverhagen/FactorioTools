using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public class PumpjackCenter : GridEntity
{
    public PumpjackCenter(int id) : base(id)
    {
    }

    public Direction Direction { get; set; }

#if ENABLE_GRID_TOSTRING
    public override string Label => "J";
#endif
}
