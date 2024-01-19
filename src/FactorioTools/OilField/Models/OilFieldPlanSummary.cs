using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

/// <summary>
/// A summary of the various oil field plans attempted.
/// </summary>
/// <param name="MissingPumpjacks">The number of pumpjacks removed to allow for electric poles. This must be zero.</param>
/// <param name="RotatedPumpjacks">The number of pumpjacks that were rotated from their original position.</param>
/// <param name="SelectedPlans">The set of plans which exactly the same and determined to be the best.</param>
/// <param name="AlternatePlans">The set of plans which are equivalent to the selected plans by ranking but not exactly the same.</param>
/// <param name="UnusedPlans">The set of plans that were not the best and were discarded.</param>
public record OilFieldPlanSummary(
    int MissingPumpjacks,
    int RotatedPumpjacks,
    ITableList<OilFieldPlan> SelectedPlans,
    ITableList<OilFieldPlan> AlternatePlans,
    ITableList<OilFieldPlan> UnusedPlans);
