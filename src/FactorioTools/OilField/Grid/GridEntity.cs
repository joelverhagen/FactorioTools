namespace Knapcode.FactorioTools.OilField;

public abstract class GridEntity
{
    protected GridEntity(int id)
    {
        Id = id;
    }

#if ENABLE_GRID_TOSTRING
    public abstract string Label { get; }
#endif

    public int Id { get; }

    public virtual void Unlink()
    {
    }
}
