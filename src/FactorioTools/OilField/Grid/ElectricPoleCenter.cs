using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class ElectricPoleCenter : GridEntity
{
    private readonly HashSet<int> _neighbors = new();

    public ElectricPoleCenter(int id) : base(id)
    {
    }

#if ENABLE_GRID_TOSTRING
    public override string Label => "E";
#endif

    public HashSet<int> Neighbors => _neighbors;

    public void AddNeighbor(ElectricPoleCenter neighbor)
    {
        _neighbors.Add(neighbor.Id);
        neighbor._neighbors.Add(Id);
    }

    public void ClearNeighbors(SquareGrid grid)
    {
        foreach (var id in _neighbors)
        {
            var neighbor = grid.GetEntity<ElectricPoleCenter>(id);
            neighbor._neighbors.Remove(Id);
        }

        _neighbors.Clear();
    }

    public override void Unlink(SquareGrid grid)
    {
        ClearNeighbors(grid);
    }
}
