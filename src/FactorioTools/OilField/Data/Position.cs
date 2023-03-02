using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

internal class Position
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }
}
