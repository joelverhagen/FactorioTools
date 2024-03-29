﻿namespace Knapcode.FactorioTools.OilField;

public class NonStandardBeacon : BasePlannerTest
{
    [Theory]
    [MemberData(nameof(SmallListIndexTestData))]
    public async Task Execute(int blueprintIndex)
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.AddBeacons = true;
        options.UseUndergroundPipes = true;
        options.OverlapBeacons = true;
        options.ValidateSolution = true;
        options.BeaconWidth = 2;
        options.BeaconHeight = 4;
        options.BeaconSupplyWidth = 4;
        options.BeaconSupplyHeight = 6;
        options.BeaconStrategies = OilFieldOptions.AllBeaconStrategies.ToList();

        var blueprintString = SmallListBlueprintStrings[blueprintIndex];
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var result = Planner.Execute(options, blueprint);

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
