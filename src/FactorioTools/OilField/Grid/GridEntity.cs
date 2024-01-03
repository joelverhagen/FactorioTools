namespace Knapcode.FactorioTools.OilField;

public abstract class GridEntity
{
#if ENABLE_GRID_TOSTRING
    public abstract string Label { get; }
#endif

    public virtual void Unlink()
    {
    }
}
