namespace Knapcode.FactorioTools.WebApp;

public static class HttpContextExtensions
{
    public static string GetClientIpAddress(this HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
