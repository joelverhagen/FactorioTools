using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField.Grid;

public class ElectricPoleCenter : GridEntity
{
    private readonly ElectricPoleCenterSet _neighbors = new();

#if ENABLE_GRID_TOSTRING
    public override string Label => "E";
#endif

    public ElectricPoleCenterSet Neighbors => _neighbors;

    public void AddNeighbor(ElectricPoleCenter neighbor)
    {
        _neighbors.Add(neighbor);
        neighbor._neighbors.Add(this);
    }

    public void ClearNeighbors()
    {
        foreach (var neighbor in _neighbors.EnumerateItems())
        {
            neighbor._neighbors.Remove(this);
        }

        _neighbors.Clear();
    }

    public override void Unlink()
    {
        ClearNeighbors();
    }
}
