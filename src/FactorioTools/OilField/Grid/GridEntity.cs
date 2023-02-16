namespace Knapcode.FactorioTools.OilField.Grid;

internal abstract class GridEntity
{
    public abstract string Label { get; }

    public virtual void Unlink()
    {
    }
}
