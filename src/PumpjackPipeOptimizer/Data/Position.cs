using System.Text.Json.Serialization;

namespace PumpjackPipeOptimizer.Data;

internal class Position
{
    [JsonPropertyName("x")]
    public required float X { get; set; }

    [JsonPropertyName("y")]
    public required float Y { get; set; }
}
