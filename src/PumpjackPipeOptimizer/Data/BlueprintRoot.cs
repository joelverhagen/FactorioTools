using System.Text.Json.Serialization;

namespace PumpjackPipeOptimizer.Data;

internal class BlueprintRoot
{
    [JsonPropertyName("blueprint")]
    public required Blueprint Blueprint { get; set; }
}
