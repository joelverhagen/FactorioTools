using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

internal class BlueprintRoot
{
    [JsonPropertyName("blueprint")]
    public required Blueprint Blueprint { get; set; }
}
