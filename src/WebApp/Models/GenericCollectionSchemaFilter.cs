using System.Diagnostics;
using System.Reflection;
using Knapcode.FactorioTools.OilField;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Knapcode.FactorioTools.WebApp.Models;

public class GenericCollectionSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        var propertyToInfo = context
            .Type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var (propertyKey, property) in model.Properties)
        {
            if (propertyToInfo.TryGetValue(propertyKey, out var info))
            {
                if (info.PropertyType.IsGenericType
                    && info.PropertyType.GetGenericTypeDefinition() == typeof(ITableArray<>))
                {
                    var listType = typeof(List<>).MakeGenericType(info.PropertyType.GenericTypeArguments[0]);
                    var generated = context.SchemaGenerator.GenerateSchema(listType, context.SchemaRepository);
                    property.AllOf = generated.AllOf;
                    property.Type = generated.Type;
                    property.Items = generated.Items;
                }
            }
        }
    }
}