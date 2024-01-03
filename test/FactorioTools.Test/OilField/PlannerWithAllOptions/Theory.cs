namespace Knapcode.FactorioTools.OilField;

public class PlannerOnAllBlueprintsAndOptions : BasePlannerFacts
{
    [Theory]
    [MemberData(nameof(BlueprintsAndOptions))]
    public async Task Execute(int blueprintIndex, string electricPole, bool addBeacons, bool overlapBeacons, bool useUndergroundPipes)
    {
        // Arrange
        var options = ElectricPoleToOptions[electricPole]();
        options.AddBeacons = addBeacons;
        options.UseUndergroundPipes = useUndergroundPipes;
        options.OverlapBeacons = overlapBeacons;
        options.ValidateSolution = true;

        var blueprintString = SmallListBlueprintStrings[blueprintIndex];
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var result = Planner.Execute(options, blueprint);

        // Assert
#if USE_VERIFY
        await Verify(GetGridString(result))
            .UseTypeName("Theory")
            .UseMethodName("E")
            .UseTextForParameters($"{blueprintIndex:D4}_{electricPole}{(addBeacons ? "_b" : "")}{(overlapBeacons ? "_o" : "")}{(useUndergroundPipes ? "_u" : "")}");
#else
        await Task.Yield();
#endif
    }
}
