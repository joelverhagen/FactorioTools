using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public class RejectsBlueprintWithBlockingIsolatedArea : BasePlannerFacts
{
    [Theory]
    [MemberData(nameof(BlueprintsWithIsolatedAreasIndexes))]
    public void Execute(int index)
    {
        var options = OilFieldOptions.ForMediumElectricPole;

        // this has a pumpjack that has it's top and right terminal blocked by other pumpjacks and the bottom and
        // left terminals pointed into an isolated area. There is probably a solution if you place underground pipes
        // from the beginning, but that's not supported today. Underground pipes are only optimized from a fully
        // connected system of above ground pipes.
        var blueprintString = BlueprintsWithIsolatedAreas[index];

        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var ex = Assert.Throws<NoPathBetweenTerminalsException>(() => Planner.Execute(options, blueprint));
    }
}
