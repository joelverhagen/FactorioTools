namespace Knapcode.FactorioTools.OilField;

public class AvoidEntity : GridEntity
{
    public AvoidEntity(int id) : base(id)
    {
    }

#if ENABLE_GRID_TOSTRING
    public override string Label => "X";
#endif
}