using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField;

internal class Context
{
    public required Options Options { get; set; }
    public required BlueprintRoot InputBlueprint { get; set; }
    public required SquareGrid Grid { get; set; }
    public required IReadOnlyDictionary<Location, List<TerminalLocation>> CenterToTerminals { get; set; }
    public required Dictionary<Location, List<TerminalLocation>> LocationToTerminals { get; set; }
}
