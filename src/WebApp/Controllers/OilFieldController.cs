using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.OilField.Steps;
using Microsoft.AspNetCore.Mvc;

namespace Knapcode.FactorioTools.WebApp.Controllers;

[ApiController]
[Route("api/v1/oil-field")]
public class OilFieldController : ControllerBase
{
    private readonly ILogger<OilFieldController> _logger;

    public OilFieldController(ILogger<OilFieldController> logger)
    {
        _logger = logger;
    }

    [HttpGet("plan")]
    public object GetPlan(string blueprint, [FromQuery] OilFieldOptions options, bool addOffsetCorrection = false)
    {
        _logger.LogInformation("Planning for blueprint: {Blueprint}.", blueprint);
        var parsedBlueprint = ParseBlueprint.Execute(blueprint);
        (var context, _) = Planner.Execute(options, parsedBlueprint);
        var outputBlueprint = GridToBlueprintString.Execute(context, addOffsetCorrection);
        return new { Blueprint = outputBlueprint };
    }
}
