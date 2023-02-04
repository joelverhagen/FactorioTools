using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

internal class Blueprint
{
    [JsonPropertyName("icons")]
    public required Icon[] Icons { get; set; }

    [JsonPropertyName("entities")]
    public required Entity[] Entities { get; set; }

    [JsonPropertyName("item")]
    public required string Item { get; set; }

    [JsonPropertyName("version")]
    public required long Version { get; set; }
}
