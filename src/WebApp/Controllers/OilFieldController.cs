using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.WebApp.Models;
using Microsoft.AspNetCore.Cors;
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

    [HttpPost("normalize")]
    [EnableCors]
    public OilFieldNormalizeResponse NormalizeBlueprint([FromBody] OilFieldNormalizeRequest request)
    {
        var parsedBlueprint = ParseBlueprint.Execute(request.Blueprint);
        _logger.LogInformation("Normalizing blueprint {Blueprint}", request.Blueprint);
        var clean = CleanBlueprint.Execute(parsedBlueprint);
        var outputBlueprint = GridToBlueprintString.SerializeBlueprint(clean, addFbeOffset: false);
        return new OilFieldNormalizeResponse(request, outputBlueprint);
    }

    [HttpPost("plan")]
    [EnableCors]
    public OilFieldPlanResponse GetPlan([FromBody] OilFieldPlanRequest request)
    {
        var parsedBlueprint = ParseBlueprint.Execute(request.Blueprint);
        _logger.LogInformation("Planning oil field for blueprint {Blueprint}", request.Blueprint);
        (var context, var summary) = Planner.Execute(request, parsedBlueprint);
        var outputBlueprint = GridToBlueprintString.Execute(context, request.AddFbeOffset);
        return new OilFieldPlanResponse(request, outputBlueprint, summary);
    }
}
