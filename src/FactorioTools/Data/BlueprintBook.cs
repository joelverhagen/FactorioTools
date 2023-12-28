using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.Data;

public class BlueprintBook
{
    [JsonPropertyName("blueprints")]
    public BlueprintPage[] Blueprints { get; set; } = null!;

    [JsonPropertyName("item")]
    public string Item { get; set; } = null!;

    [JsonPropertyName("label")]
    public string Label { get; set; } = null!;

    [JsonPropertyName("active_index")]
    public int ActiveIndex { get; set; }

    [JsonPropertyName("version")]
    public long Version { get; set; }
}
