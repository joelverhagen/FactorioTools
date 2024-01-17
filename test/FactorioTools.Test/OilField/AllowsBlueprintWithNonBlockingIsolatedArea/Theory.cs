namespace Knapcode.FactorioTools.OilField;

public class AllowsBlueprintWithNonBlockingIsolatedArea : BasePlannerTest
{
    [Theory]
    [MemberData(nameof(AllPipeStrategiesTestData))]
    public async Task Execute(PipeStrategy strategy)
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.PipeStrategies = new List<PipeStrategy> { strategy };
        var blueprintString = "0eJyU1ctugzAQBdB/mbUXZmwg8CtVFRFiVW6Dg3hURYh/r8Fe9IHEZQmYw2DN9cx0e4ym7awbqJzJ1k/XU/kyU2/fXPVY77mqMVRSOzbte1V/kKBhatc7djANLYKsu5svKpPlVZBxgx2sCcZ2MV3d2NxM5xeIHat99v6Fp1u/5BHWgia/VHn3bjtTh2dyEf84BjjFMKcQLoc5jXAycNlvjne49ER1AJcBXBK5/Phnc4DTWeAux9wFaZRYXXHMFcjeKZhLJLJ5wWMJeCdywQngQcGQuHciGcyAh0SDY31A0hIkG7FdWAMeEg6lcQ9Kh8Q9JB76RH1IPjj2M3AYMJIPVex6e2cVQ/lIN08B/cwnBgfkQfkIp59K/3p+Zm5ztPwxiAV9mq6PC5ZvAAAA//8DADIXgDE=";

        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var result = Planner.Execute(options, blueprint);

        // Assert
#if USE_VERIFY
        await Verify(GetGridString(result))
            .UseTypeName("Theory")
            .UseMethodName("E")
            .UseTextForParameters($"{strategy}");
#else
        await Task.Yield();
#endif
    }
}
