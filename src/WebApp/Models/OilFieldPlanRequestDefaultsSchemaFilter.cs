using System.Reflection;
using Knapcode.FactorioTools.OilField;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Knapcode.FactorioTools.WebApp.Models;

public class OilFieldPlanRequestDefaultsSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type != typeof(OilFieldPlanRequest))
        {
            return;
        }

        var defaults = new OilFieldPlanRequest();

        if (model.Properties.ContainsKey("blueprint"))
        {
            model.Properties["blueprint"].Example = new OpenApiString("0eJyMj70OwjAMhN/lZg8NbHkVhFB/rMrQuFGSIqoq707aMiCVgcWSz+fP5wXNMLEPogl2gbSjRtjLgii91sOqae0YFn5y/l63DxDS7FdFEjtkgmjHL1iTrwTWJEl4Z2zNfNPJNRyKgX6w/BjLwqjrpQI5E+ZSC7WTwO0+qTIdYKc/YKbaaOaAK0G38Pbre8KTQ/wY8hsAAP//AwAEfF3F");
        }

        var propertyToValue = defaults
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p.GetValue(defaults), StringComparer.OrdinalIgnoreCase);

        var optionalProperties = model
            .Properties
            .Where(x => !model.Required.Contains(x.Key))
            .Select(x => x.Key);

        foreach (var propertyKey in optionalProperties)
        {
            if (propertyToValue.TryGetValue(propertyKey, out var defaultValue))
            {
                IOpenApiAny value = defaultValue switch
                {
                    string v => new OpenApiString(v),
                    int v => new OpenApiInteger(v),
                    double v => new OpenApiDouble(v),
                    bool v => new OpenApiBoolean(v),
                    IEnumerable<BeaconStrategy> v => ToStringArray(v),
                    IEnumerable<PipeStrategy> v => ToStringArray(v),
                    IEnumerable<KeyValuePair<string, int>> v => ToObjectArray(v),
                    _ => throw new NotImplementedException(),
                };

                model.Properties[propertyKey].Default = value;
            }
        }
    }

    private OpenApiArray ToStringArray<T>(IEnumerable<T> values)
    {
        var array = new OpenApiArray();
        foreach (var value in values)
        {
            array.Add(new OpenApiString(value!.ToString()));
        }

        return array;
    }

    private OpenApiObject ToObjectArray(IEnumerable<KeyValuePair<string, int>> values)
    {
        var array = new OpenApiObject();
        foreach ((var key, var value) in values)
        {
            array.Add(key, new OpenApiInteger(value));
        }

        return array;
    }
}