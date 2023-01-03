using System.Text.Json.Serialization;

namespace PumpjackPipeOptimizer.Data;

internal class Icon
{
    [JsonPropertyName("signal")]
    public required SignalID Signal { get; set; }

    [JsonPropertyName("index")]
    public required int Index { get; set; }
}
