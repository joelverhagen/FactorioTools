namespace Knapcode.FactorioTools.OilField;

public class AllStrategiesOnBigList : BasePlannerFacts
{
    [Theory]
    [MemberData(nameof(BigListIndexTestData))]
    public async Task Execute(int blueprintIndex)
    {
        // Arrange
        var blueprintString = BigListBlueprintStrings[blueprintIndex];

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
