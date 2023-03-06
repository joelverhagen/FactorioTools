using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField;

public class Context
{
    public required OilFieldOptions Options { get; set; }
    public required BlueprintRoot InputBlueprint { get; set; }
    public required SquareGrid Grid { get; set; }
    public required Dictionary<Location, List<TerminalLocation>> CenterToTerminals { get; set; }
    public required Dictionary<Location, List<TerminalLocation>> LocationToTerminals { get; set; }
    public required int[,] LocationToAdjacentCount { get; set; }

    public required List<AttemptedPlan> Plans { get; set; }

    public required SharedInstances SharedInstances { get; set; }
}

public record PlanSummary(
    int MissingPumpjacks,
    IReadOnlyList<AttemptedPlan> SelectedPlans,
    IReadOnlyList<AttemptedPlan> AllPlans);

public record AttemptedPlan(
    PipeStrategy PipeStrategy,
    bool OptimizePipes,
    BeaconStrategy? BeaconStrategy,
    int BeaconEffectCount,
    int BeaconCount,
    int PipeCount);