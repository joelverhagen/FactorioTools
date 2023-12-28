using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public partial class PlannerFacts
{
    [Theory]
    [MemberData(nameof(BlueprintIndexTestData))]
    public void NonStandardBeacon(int blueprintIndex)
    {
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
        Planner.Execute(options, blueprint);
    }
}
