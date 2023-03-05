using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(BlueprintRoot))]
public partial class BlueprintSerializationContext : JsonSerializerContext
{
}
