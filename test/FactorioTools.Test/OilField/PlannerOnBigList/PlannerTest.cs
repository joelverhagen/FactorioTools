using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public partial class PlannerFacts
{
    [Theory]
    [MemberData(nameof(BigListIndexTestData))]
    public void PlannerOnBigList(int blueprintIndex)
    {
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        var blueprintString = BigListBlueprintStrings[blueprintIndex];
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        Planner.Execute(options, blueprint);
    }
}
