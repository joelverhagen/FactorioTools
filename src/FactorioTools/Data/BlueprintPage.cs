using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.Data;

public class BlueprintPage
{
    [JsonPropertyName("blueprint")]
    public Blueprint Blueprint { get; set; } = null!;

    [JsonPropertyName("index")]
    public int Index { get; set; }
}
