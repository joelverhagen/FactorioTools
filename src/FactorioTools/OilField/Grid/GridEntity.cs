namespace Knapcode.FactorioTools.OilField.Grid;

public abstract class GridEntity
{
    public abstract string Label { get; }

    public virtual void Unlink()
    {
    }
}
