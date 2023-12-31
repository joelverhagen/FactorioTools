namespace Knapcode.FactorioTools.WebApp.Models;

/// <summary>
/// The normalized oil field blueprint.
/// </summary>
/// <param name="Request">The original request provided, included expanded defaults.</param>
/// <param name="Blueprint">The output normalized blueprint.</param>
public record OilFieldNormalizeResponse(
    OilFieldNormalizeRequestResponse Request,
    string Blueprint);
