namespace Knapcode.FactorioTools.OilField;

public class Terminal : Pipe
{
    public Terminal(int id) : base(id)
    {
    }

#if ENABLE_GRID_TOSTRING
    public override string Label => "+";
#endif
}
