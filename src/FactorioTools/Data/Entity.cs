using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.Data;

public class Entity
{
    [JsonPropertyName("entity_number")]
    public int EntityNumber { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("position")]
    public Position Position { get; set; } = null!;

    /// <summary>
    /// Should be nullable but is blocked by https://github.com/yanghuan/CSharp.lua/issues/465
    /// </summary>
    [JsonPropertyName("direction")]
    public Direction Direction { get; set; } = Direction.Up;

    [JsonPropertyName("items")]
    public Dictionary<string, int>? Items { get; set; }

    [JsonPropertyName("neighbours")]
    public int[]? Neighbours { get; set; }
}
