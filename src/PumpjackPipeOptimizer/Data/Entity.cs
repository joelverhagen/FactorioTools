using System.Text.Json.Serialization;

namespace PumpjackPipeOptimizer.Data;

internal class Entity
{
    [JsonPropertyName("entity_number")]
    public required int EntityNumber { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("position")]
    public required Position Position { get; set; }

    [JsonPropertyName("direction")]
    public Direction? Direction { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, int>? Items { get; set; }

    [JsonPropertyName("neighbours")]
    public int[]? Neighbours { get; set; }
}
