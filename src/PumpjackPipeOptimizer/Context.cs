using PumpjackPipeOptimizer.Data;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer;

internal class Context
{
    public required Options Options { get; set; }
    public required BlueprintRoot InputBlueprint { get; set; }
    public required SquareGrid Grid { get; set; }
    public required IReadOnlySet<Location> Centers { get; set; }
    public required IReadOnlyDictionary<Location, HashSet<Location>> CenterToTerminals { get; set; }
}
