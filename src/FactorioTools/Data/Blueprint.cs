using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.Data;

public class Blueprint
{
    [JsonPropertyName("icons")]
    public Icon[] Icons { get; set; } = null!;

    [JsonPropertyName("entities")]
    public Entity[] Entities { get; set; } = null!;

    [JsonPropertyName("item")]
    public string Item { get; set; } = null!;

    [JsonPropertyName("version")]
    public ulong Version { get; set; }
}
