using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public class AllowsBlueprintWithNonBlockingIsolatedArea : BasePlannerFacts
{
    [Theory]
    [MemberData(nameof(AllPipeStrategiesTestData))]
    public async Task Execute(PipeStrategy strategy)
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.PipeStrategies = new List<PipeStrategy> { strategy };
        var blueprintString = "0eJyU1dtuhCAQBuB3mWsudEBdfZWm2bguaWhXNB6aGuO7FxcuejDx51LEzxHnh5Vuj1n3g7ETVSuZprMjVS8rjebN1o99zNatpor6ue3f6+aDBE1Lv4+YSbe0CTL2rr+oSrdXQdpOZjLaG8+L5Wrn9qYHN0EcWH03ugc6u7/JIawELW6qdO7dDLrx95JN/OMY4CTDnES4AuYUwiWey39zfMBlEdUBXA5waeCK848tAE7lnruccxekUUJ15TlXImsnYS5NkMXzHieAF5ELTgGP8daDvIhkMAMeEg0O9QFJSzO8XVgBHhIOqXAPSkeCe0g8VER9SD449DOwGey/7nz9ykPvaK9iKB+uCZwngX7miIMD8qB8+N1PZn89d2Y+z9Hqx0Es6FMPY5iwfQMAAP//AwAyF4Ax";

        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (context, _) = Planner.Execute(options, blueprint);

        // Assert
        await Verify(GetGridString(context))
            .UseTypeName("Theory")
            .UseMethodName("E")
            .UseTextForParameters($"{strategy}");
    }
}
