using System;

namespace Knapcode.FactorioTools.OilField;

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
    int PipeCount)
{
    public bool IsEquivalent(OilFieldPlan other)
    {
        return BeaconEffectCount == other.BeaconEffectCount
            && BeaconCount == other.BeaconCount
            && PipeCount == other.PipeCount;
    }

#if ENABLE_GRID_TOSTRING
    public override string ToString()
    {
        return ToString(includeCounts: false);
    }

    public string ToString(bool includeCounts)
    {
        var output = PipeStrategy switch
        {
            PipeStrategy.FbeOriginal => "FBE",
            PipeStrategy.Fbe => "FBE*",
            PipeStrategy.ConnectedCentersDelaunay => "CC-DT",
            PipeStrategy.ConnectedCentersDelaunayMst => "CC-DT-MST",
            PipeStrategy.ConnectedCentersFlute => "CC-FLUTE",
            _ => throw new NotImplementedException(),
        };

        if (OptimizePipes)
        {
            output += " -> optimize";
        }

        if (BeaconStrategy.HasValue)
        {
            output += BeaconStrategy.Value switch
            {
                OilField.BeaconStrategy.FbeOriginal => " -> FBE",
                OilField.BeaconStrategy.Fbe => " -> FBE*",
                OilField.BeaconStrategy.Snug => " -> snug",
                _ => throw new NotImplementedException(),
            };
        }

        if (includeCounts)
        {
            if (BeaconStrategy.HasValue)
            {
                output += $" (effects: {BeaconEffectCount}, beacons: {BeaconCount}, pipes: {PipeCount})";
            }
            else
            {
                output += $" (pipes: {PipeCount})";
            }
        }

        return output;
    }
#else
    public string ToString(bool includeCounts)
    {
        return ToString();
    }
#endif
}
