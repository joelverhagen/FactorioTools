namespace Knapcode.FactorioTools.OilField;

public class BeaconCenter : GridEntity
{
    public BeaconCenter(int id) : base(id)
    {
    }

#if ENABLE_GRID_TOSTRING
    public override string Label => "B";
#endif
}