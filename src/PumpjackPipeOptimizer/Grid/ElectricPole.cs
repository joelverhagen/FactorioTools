namespace PumpjackPipeOptimizer.Grid;

internal class ElectricPole : GridEntity
{
    private readonly HashSet<ElectricPole> _neighbors = new HashSet<ElectricPole>();

    public override string Label => "e";

    public IReadOnlySet<ElectricPole> Neighbors => _neighbors;

    public void AddNeighbor(ElectricPole neighbor)
    {
        _neighbors.Add(neighbor);
        neighbor._neighbors.Add(this);
    }
}