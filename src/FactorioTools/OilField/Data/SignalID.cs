using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

internal class SignalID
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;
}
