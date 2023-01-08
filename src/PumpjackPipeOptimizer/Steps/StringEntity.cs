using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

internal class StringEntity : GridEntity
{
    public StringEntity(string label)
    {
        Label = label;
    }

    public override string Label { get; }
}