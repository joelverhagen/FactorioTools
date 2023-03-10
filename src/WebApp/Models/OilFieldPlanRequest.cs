using System.ComponentModel.DataAnnotations;
using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools.WebApp.Models;

/// <summary>
/// The properties needed to generate an oil field plan.
/// </summary>
public class OilFieldPlanRequest : OilFieldOptions
{
    /// <summary>
    /// The input blueprint containing at least one pumpjack.
    /// </summary>
    [Required] public string Blueprint { get; set; } = null!;

    /// <summary>
    /// Whether or not to add a placeholder entity to the output grid so that the planning grid entity coordinates match
    /// the entity coordinate when the output blueprint is pasted into Factorio Blueprint Editor (FBE). This helps with
    /// debugging the planner.
    /// </summary>
    public bool AddFbeOffset { get; set; } = false;
}
