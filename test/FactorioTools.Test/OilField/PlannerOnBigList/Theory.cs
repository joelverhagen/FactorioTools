using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public class PlannerOnBigList : BasePlannerFacts
{
    [Theory]
    [MemberData(nameof(BigListIndexTestData))]
    public async Task Execute(int blueprintIndex)
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        var blueprintString = BigListBlueprintStrings[blueprintIndex];
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (context, _) = Planner.Execute(options, blueprint);

        // Assert
        await Verify(GetGridString(context))
            .UseTypeName("Theory")
            .UseMethodName("E")
            .UseTextForParameters($"{blueprintIndex:D4}");
    }
}
