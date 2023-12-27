using System.Text.Json.Serialization;

namespace Knapcode.FactorioTools.Data;

public class Icon
{
    [JsonPropertyName("signal")]
    public SignalID Signal { get; set; } = null!;

    [JsonPropertyName("index")]
    public int Index { get; set; }
}
