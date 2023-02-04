using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.OilField.Data;

internal class Icon
{
    [JsonPropertyName("signal")]
    public required SignalID Signal { get; set; }

    [JsonPropertyName("index")]
    public required int Index { get; set; }
}
