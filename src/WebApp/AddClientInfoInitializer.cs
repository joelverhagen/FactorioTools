using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Knapcode.FactorioTools.WebApp;

public class AddClientInfoInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AddClientInfoInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not RequestTelemetry request)
        {
            return;
        }

        if (int.TryParse(request.ResponseCode, out var statusCode)
            && (statusCode == 400 || statusCode == 404 || statusCode == 429))
        {
            request.Success = true;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        var clientIp = httpContext.GetClientIpAddress();
        var telemetryIp = telemetry.Context.Location.Ip;
        request.Properties["ClientIp"] = clientIp;
        if (telemetryIp != clientIp && !string.IsNullOrWhiteSpace(telemetryIp))
        {
            request.Properties["TelemetryIp"] = telemetryIp;
        }

        request.Properties["UserAgent"] = httpContext.Request.Headers.UserAgent.ToString();
    }
}
