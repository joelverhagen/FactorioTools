using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Knapcode.FactorioTools.WebApp.Models;

/// <summary>
/// Source: https://stackoverflow.com/a/68987970
/// </summary>
public class RequireNonNullablePropertiesSchemaFilter<T> : ISchemaFilter
{
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type != typeof(T))
        {
            return;
        }

        var additionalRequiredProps = model
            .Properties
            .Where(x => !x.Value.Nullable && !model.Required.Contains(x.Key))
            .Select(x => x.Key);

        foreach (var propKey in additionalRequiredProps)
        {
            model.Required.Add(propKey);
        }
    }
}
