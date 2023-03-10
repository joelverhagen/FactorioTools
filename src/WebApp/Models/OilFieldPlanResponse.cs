using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools.WebApp.Models;

/// <summary>
/// The resulting oil field plan.
/// </summary>
/// <param name="Request"> The original request provided, included expanded defaults. </param>
/// <param name="Blueprint"> The output blueprint, containing the planned oil field. </param>
/// <param name="Summary"> A summary of different oil field plans attempt and their performance. </param>
public record OilFieldPlanResponse(OilFieldPlanRequest Request, string Blueprint, OilFieldPlanSummary Summary);