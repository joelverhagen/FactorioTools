namespace Knapcode.FactorioTools.OilField;

public class TemporaryEntity : GridEntity
{
    public TemporaryEntity(int id) : base(id)
    {
    }

#if ENABLE_GRID_TOSTRING
    public override string Label => "?";
#endif
}