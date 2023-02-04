using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

internal class Position
{
    [JsonPropertyName("x")]
    public required float X { get; set; }

    [JsonPropertyName("y")]
    public required float Y { get; set; }
}
