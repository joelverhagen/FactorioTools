using System.IO.Pipelines;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.WebApp.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Models;

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
            options.RejectionStatusCode = 429;

            options.OnRejected = (context, token) =>
            {
                var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                context.HttpContext.Response.WriteAsJsonAsync(factory.CreateProblemDetails(
                    context.HttpContext,
                    statusCode: 429,
                    title: "Too Many Requests",
                    type: "https://www.rfc-editor.org/rfc/rfc6585.html#section-4"));
                return ValueTask.CompletedTask;
            };

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
            options.SchemaFilter<GenericCollectionSchemaFilter>();
            options.SchemaFilter<OilFieldPlanRequestDefaultsSchemaFilter>();
            options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter<OilFieldNormalizeResponse>>();
            options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter<OilFieldNormalizeRequestResponse>>();
            options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter<OilFieldPlanResponse>>();
            options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter<OilFieldPlanRequestResponse>>();
            options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter<OilFieldPlanSummary>>();
            options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter<OilFieldPlan>>();
            options.MapType<ITableList<BeaconStrategy>>(() => new OpenApiSchema());
            options.MapType<ITableList<PipeStrategy>>(() => new OpenApiSchema());
            options.MapType<ITableList<OilFieldPlan>>(() => new OpenApiSchema());
            options.SupportNonNullableReferenceTypes();
            options.UseAllOfToExtendReferenceSchemas();
            options.IncludeXmlComments(Path.ChangeExtension(typeof(OilFieldOptions).Assembly.Location, ".xml"));
            options.IncludeXmlComments(Path.ChangeExtension(typeof(OilFieldPlanRequest).Assembly.Location, ".xml"));
        });

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyMethod();
                    policy.AllowAnyHeader();
                });
        });

        var app = builder.Build();

        app.UseCors();

        app.UseStaticFiles();

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
