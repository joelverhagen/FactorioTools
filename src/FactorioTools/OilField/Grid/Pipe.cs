namespace Knapcode.FactorioTools.OilField;

public class Pipe : GridEntity
{
    public Pipe(int id) : base(id)
    {
    }

#if ENABLE_GRID_TOSTRING
    public override string Label => "o";
#endif
}
