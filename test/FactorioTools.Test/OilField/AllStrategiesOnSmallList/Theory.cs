namespace Knapcode.FactorioTools.OilField;

public class PlannerOnSmallList : BasePlannerFacts
{
    [Theory]
    [MemberData(nameof(SmallListIndexTestData))]
    public async Task Execute(int blueprintIndex)
    {
        // Arrange
        var blueprintString = SmallListBlueprintStrings[blueprintIndex];

        // Act
        var context = ExecuteAllStrategies(blueprintString);

        // Assert
#if USE_VERIFY
        await Verify(GetGridString(context))
            .UseTypeName("Theory")
            .UseMethodName("E")
            .UseTextForParameters($"{blueprintIndex:D4}");
#endif
    }
}
