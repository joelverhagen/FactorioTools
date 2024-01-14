namespace Knapcode.FactorioTools.OilField;

public class ElectricPoleSide : GridEntity
{
    public ElectricPoleSide(int id, ElectricPoleCenter center) : base(id)
    {
        Center = center;
    }

    public ElectricPoleCenter Center { get; }

#if ENABLE_GRID_TOSTRING
    public override string Label => "e";
#endif
}