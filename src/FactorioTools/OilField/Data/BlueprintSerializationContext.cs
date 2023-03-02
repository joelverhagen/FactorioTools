using System.Text.Json;
using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(BlueprintRoot))]
internal partial class BlueprintSerializationContext : JsonSerializerContext
{
}
