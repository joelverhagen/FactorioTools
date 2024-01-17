namespace Knapcode.FactorioTools.OilField;

public class AllStrategiesOnSmallList : BasePlannerTest
{
    [Theory]
    [MemberData(nameof(SmallListIndexTestData))]
    public async Task Execute(int blueprintIndex)
    {
        // Arrange
        var blueprintString = SmallListBlueprintStrings[blueprintIndex];

        // Act
        var result = ExecuteAllStrategies(blueprintString);

        // Assert
#if USE_VERIFY
        await Verify(GetGridString(result))
            .UseTypeName("Theory")
            .UseMethodName("E")
            .UseTextForParameters($"{blueprintIndex:D4}");
#else
        await Task.Yield();
#endif
    }
}
