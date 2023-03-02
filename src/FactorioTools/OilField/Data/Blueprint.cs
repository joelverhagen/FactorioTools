using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

internal class Blueprint
{
    [JsonPropertyName("icons")]
    public Icon[] Icons { get; set; } = null!;

    [JsonPropertyName("entities")]
    public Entity[] Entities { get; set; } = null!;

    [JsonPropertyName("item")]
    public string Item { get; set; } = null!;

    [JsonPropertyName("version")]
    public long Version { get; set; }
}
