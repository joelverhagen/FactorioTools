using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

internal class BlueprintRoot
{
    [JsonPropertyName("blueprint")]
    public Blueprint Blueprint { get; set; } = null!;
}
