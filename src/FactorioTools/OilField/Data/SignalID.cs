using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

internal class SignalID
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }
}
