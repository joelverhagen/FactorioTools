using System.ComponentModel.DataAnnotations;

namespace Knapcode.FactorioTools.WebApp.Models;

/// <summary>
/// The properties needed to normalize a oil field blueprint.
/// </summary>
public class OilFieldNormalizeRequestResponse
{
    /// <summary>
    /// The input blueprint containing at least one pumpjack.
    /// </summary>
    [Required] public string Blueprint { get; set; } = null!;
}
