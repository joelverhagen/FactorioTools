namespace Knapcode.FactorioTools.OilField.Grid;

public abstract class GridEntity
{
#if ENABLE_VISUALIZER
    public abstract string Label { get; }
#endif

    public virtual void Unlink()
    {
    }
}
