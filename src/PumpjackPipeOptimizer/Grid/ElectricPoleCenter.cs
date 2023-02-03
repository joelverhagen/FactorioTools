namespace PumpjackPipeOptimizer.Grid;

internal class ElectricPoleCenter : GridEntity
{
    private readonly HashSet<ElectricPoleCenter> _neighbors = new HashSet<ElectricPoleCenter>();

    public override string Label => "E";

    public IReadOnlySet<ElectricPoleCenter> Neighbors => _neighbors;

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
}
