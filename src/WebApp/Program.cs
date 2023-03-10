using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.WebApp.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpOverrides;

namespace Knapcode.FactorioTools.WebApp;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.GetClientIpAddress(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 10,
                        QueueLimit = 0,
                    }));
        });

        builder.Services.AddSingleton<ITelemetryInitializer, AddClientInfoInitializer>();
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.EnableAdaptiveSampling = false;
        });

        builder.Services.AddHealthChecks();

        builder.Services
            .AddControllers(options =>
            {
                options.Filters.Add<ExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SchemaFilter<OilFieldPlanRequestDefaultsSchemaFilter>();
            options.SupportNonNullableReferenceTypes();
            options.IncludeXmlComments(Path.ChangeExtension(typeof(OilFieldOptions).Assembly.Location, ".xml"));
            options.IncludeXmlComments(Path.ChangeExtension(typeof(OilFieldPlanRequest).Assembly.Location, ".xml"));
        });

        var app = builder.Build();

        app.MapHealthChecks("/healthz");

        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseForwardedHeaders();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
