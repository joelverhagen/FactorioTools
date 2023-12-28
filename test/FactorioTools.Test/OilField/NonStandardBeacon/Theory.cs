﻿using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public class NonStandardBeacon : BasePlannerFacts
{
    [Theory]
    [MemberData(nameof(BlueprintIndexTestData))]
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

        var blueprintString = BlueprintStrings[blueprintIndex];
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