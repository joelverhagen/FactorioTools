using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.Data;

public class BlueprintRoot
{
    [JsonPropertyName("blueprint")]
    public Blueprint Blueprint { get; set; } = null!;
}
