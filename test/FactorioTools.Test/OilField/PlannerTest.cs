using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public class PlannerTest
{
    public class Execute : Facts
    {
        [Theory]
        [MemberData(nameof(BlueprintsAndOptions))]
        public void PlannerOnAllBlueprintsAndOptions(int blueprintIndex, string electricPole, bool addBeacons, bool useUndergroundPipes)
        {
            var options = ElectricPoleToOptions[electricPole]();
            options.AddBeacons = addBeacons;
            options.UseUndergroundPipes = useUndergroundPipes;
            options.ValidateSolution = true;

            var blueprintString = BlueprintStrings[blueprintIndex];
            var blueprint = ParseBlueprint.Execute(blueprintString);

            // Act
            Planner.Execute(options, blueprint);
        }

        private static IReadOnlyDictionary<string, Func<Options>> ElectricPoleToOptions { get; } = new[]
        {
            () => Options.ForSmallElectricPole,
            () => Options.ForMediumElectricPole,
            () => Options.ForBigElectricPole,
            () => Options.ForSubstation,
        }.ToDictionary(o => o().ElectricPoleEntityName, o => o);

        private static string DataFilePath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "blueprints.txt");
        private static IReadOnlyList<string> BlueprintStrings { get; } = ParseBlueprint.ReadBlueprintFile(DataFilePath);

        public static TheoryData<int, string, bool, bool> BlueprintsAndOptions
        {
            get
            {
                var theoryData = new TheoryData<int, string, bool, bool>();

                foreach (var blueprintIndex in Enumerable.Range(0, BlueprintStrings.Count))
                {
                    foreach (var electricPole in ElectricPoleToOptions.Keys)
                    {
                        foreach (var addBeacons in new[] { false, true })
                        {
                            foreach (var useUndergroundPipes in new[] { false, true })
                            {
                                theoryData.Add(blueprintIndex, electricPole, addBeacons, useUndergroundPipes);
                            }
                        }
                    }
                }

                return theoryData;
            }
        }
    }
}
