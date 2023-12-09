using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public class PlannerTest
{
    public class Execute : Facts
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

        [Theory]
        [MemberData(nameof(AllPipeStrategiesTestData))]
        public void RejectsBlueprintWithIsolatedArea(PipeStrategy strategy)
        {
            var options = OilFieldOptions.ForMediumElectricPole;
            options.PipeStrategies = new List<PipeStrategy> { strategy };
            var blueprintString = "0eJyM1dtuhCAQBuB3mWsuBDy/StNsXJc0tCsaD02N8d0L4kXrkvBfivg5wMyw0f25qGHUZqZ6I932ZqL6baNJf5jm6cZM0ymqaVi64bNpv4jRvA5uRM+qo52RNg/1QzXf3xkpM+tZK28cD+vNLN1djXYCC1hDP9kPeuP+ZBEpGK12qrTuQ4+q9e+Snb1wAuB47rk8zkmAE4XnyjiXIlzquSrOZchiq4MTSZzLkaPw0Qke5wpksf5khfjPpQGuRLgsyIWiqxCugjmeIGdReu+SxyLkIXXBz+VmQHwCT72rF4wPqozEe0Ch8RSvNMhDakNy7xWAhxSH2xTnlcD+QdVxekArcKkV90rcq/D8k0AzcKmArlcC3UAg9SF8o796ofNwjQj10pd+YO+4496r/1ycjL7VOJ0T9l8AAAD//wMAYfxmNQ==";

            var blueprint = ParseBlueprint.Execute(blueprintString);

            // Act
            Assert.Throws<NoPathBetweenTerminalsException>(() => Planner.Execute(options, blueprint));
        }

        public static IEnumerable<object[]> AllPipeStrategiesTestData => Enum
            .GetValues<PipeStrategy>()
            .Select(x => new object[] { x });

        private static IReadOnlyDictionary<string, Func<OilFieldOptions>> ElectricPoleToOptions { get; } = new[]
        {
            () => OilFieldOptions.ForSmallElectricPole,
            () => OilFieldOptions.ForMediumElectricPole,
            () => OilFieldOptions.ForBigElectricPole,
            () => OilFieldOptions.ForSubstation,
        }.ToDictionary(o => o().ElectricPoleEntityName, o => o);

        private static string DataFilePath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "blueprints.txt");
        private static IReadOnlyList<string> BlueprintStrings { get; } = ParseBlueprint.ReadBlueprintFile(DataFilePath);

        public static TheoryData<int> BlueprintIndexTestData
        {
            get
            {
                var theoryData = new TheoryData<int>();
                foreach (var blueprintIndex in Enumerable.Range(0, BlueprintStrings.Count))
                {
                    theoryData.Add(blueprintIndex);
                }

                return theoryData;
            }
        }

        public static TheoryData<int, string, bool, bool, bool> BlueprintsAndOptions
        {
            get
            {
                var theoryData = new TheoryData<int, string, bool, bool, bool>();

                foreach (var blueprintIndex in Enumerable.Range(0, BlueprintStrings.Count))
                {
                    foreach (var electricPole in ElectricPoleToOptions.Keys)
                    {
                        foreach (var addBeacons in new[] { false, true })
                        {
                            foreach (var overlapBeacons in new[] { false, true })
                            {
                                if (!addBeacons && !overlapBeacons)
                                {
                                    continue;
                                }

                                foreach (var useUndergroundPipes in new[] { false, true })
                                {
                                    theoryData.Add(blueprintIndex, electricPole, addBeacons, overlapBeacons, useUndergroundPipes);
                                }
                            }
                        }
                    }
                }

                return theoryData;
            }
        }
    }
}
