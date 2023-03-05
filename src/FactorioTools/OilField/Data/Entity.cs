using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

public class Entity
{
    [JsonPropertyName("entity_number")]
    public int EntityNumber { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("position")]
    public Position Position { get; set; } = null!;

    [JsonPropertyName("direction")]
    public Direction? Direction { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, int>? Items { get; set; }

    [JsonPropertyName("neighbours")]
    public int[]? Neighbours { get; set; }
}
