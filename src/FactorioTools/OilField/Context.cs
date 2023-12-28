using System.Collections.Generic;
using Knapcode.FactorioTools.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField;

public class Context
{
    public required OilFieldOptions Options { get; set; }
    public required Blueprint InputBlueprint { get; set; }
    public required SquareGrid Grid { get; set; }
    public required Dictionary<Location, List<TerminalLocation>> CenterToTerminals { get; set; }
    public required Dictionary<Location, List<TerminalLocation>> LocationToTerminals { get; set; }
    public required int[,] LocationToAdjacentCount { get; set; }

    public required SharedInstances SharedInstances { get; set; }
}

/// <summary>
/// A summary of the various oil field plans attempted.
/// </summary>
/// <param name="MissingPumpjacks">The number of pumpjacks removed to allow for electric poles. This is usually zero.</param>
/// <param name="SelectedPlans">The set of plans which are equivalent and determines to be the best.</param>
/// <param name="UnusedPlans">The set of plans that were not the best and were discarded.</param>
public record OilFieldPlanSummary(
    int MissingPumpjacks,
    IReadOnlyList<OilFieldPlan> SelectedPlans,
    IReadOnlyList<OilFieldPlan> UnusedPlans);

/// <summary>
/// A particular attempt oil field plan.
/// </summary>
/// <param name="PipeStrategy">The pipe strategy used to generate the plan.</param>
/// <param name="OptimizePipes">Whether or not the pipe optimized was used.</param>
/// <param name="BeaconStrategy">Which beacon strategy, if any, was used.</param>
/// <param name="BeaconEffectCount">The number of effects the beacons provided to pumpjacks. Higher is better.</param>
/// <param name="BeaconCount">The number of beacons in the plan. For the same number of beacon effects, lower is better.</param>
/// <param name="PipeCount">The number of pipes in the plan. For the same number of beacon effects and beacons, lower is better.</param>
public record OilFieldPlan(
    PipeStrategy PipeStrategy,
    bool OptimizePipes,
    BeaconStrategy? BeaconStrategy,
    int BeaconEffectCount,
    int BeaconCount,
    int PipeCount);
