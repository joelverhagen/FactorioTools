using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class ElectricPoleCenter : GridEntity
{
    private readonly HashSet<ElectricPoleCenter> _neighbors = new();

    public ElectricPoleCenter(int id) : base(id)
    {
    }

#if ENABLE_GRID_TOSTRING
    public override string Label => "E";
#endif

    public HashSet<ElectricPoleCenter> Neighbors => _neighbors;

    public void AddNeighbor(ElectricPoleCenter neighbor)
    {
        _neighbors.Add(neighbor);
        neighbor._neighbors.Add(this);
    }

    public void ClearNeighbors()
    {
        foreach (var neighbor in _neighbors)
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
