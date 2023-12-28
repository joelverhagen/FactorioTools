using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public partial class PlannerFacts
{
    [Theory]
    [MemberData(nameof(BlueprintsAndOptions))]
    public void PlannerOnAllBlueprintsAndOptions(int blueprintIndex, string electricPole, bool addBeacons, bool overlapBeacons, bool useUndergroundPipes)
    {
        var options = ElectricPoleToOptions[electricPole]();
        options.AddBeacons = addBeacons;
        options.UseUndergroundPipes = useUndergroundPipes;
        options.OverlapBeacons = overlapBeacons;
        options.ValidateSolution = true;

        var blueprintString = BlueprintStrings[blueprintIndex];
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        Planner.Execute(options, blueprint);
    }
}
