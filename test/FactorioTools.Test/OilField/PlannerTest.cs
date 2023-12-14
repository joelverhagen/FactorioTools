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
        [MemberData(nameof(BigListIndexTestData))]
        public void PlannerOnBigList(int blueprintIndex)
        {
            var options = OilFieldOptions.ForMediumElectricPole;
            options.ValidateSolution = true;
            var blueprintString = BigListBlueprintStrings[blueprintIndex];
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
        [MemberData(nameof(BlueprintsWithIsolatedAreasIndexes))]
        public void RejectsBlueprintWithBlockingIsolatedArea(int index)
        {
            var options = OilFieldOptions.ForMediumElectricPole;

            // this has a pumpjack that has it's top and right terminal blocked by other pumpjacks and the bottom and
            // left terminals pointed into an isolated area. There is probably a solution if you place underground pipes
            // from the beginning, but that's not supported today. Underground pipes are only optimized from a fully
            // connected system of above ground pipes.
            var blueprintString = BlueprintsWithIsolatedAreas[index];

            var blueprint = ParseBlueprint.Execute(blueprintString);

            // Act
            var ex = Assert.Throws<NoPathBetweenTerminalsException>(() => Planner.Execute(options, blueprint));
        }

        public static IReadOnlyList<string> BlueprintsWithIsolatedAreas = new[]
        {
            "0eJyU1UluhDAQBdC71NoLKBtouEoUtWjaipw0BjFEQYi7x26zyIDkzxIwj/LwqZVuj1n3g7ETVSuZprMjVS8rjebN1g9/z9atpor6ue3f6+aDBE1L7++YSbe0CTL2rr+oSrdXQdpOZjI6GM+L5Wrn9qYHN0AcWH03uhc667/kEFaCFjdUOvduBt2EZ8km/nEMcJJhTiJcAXMK4ZLA5b85PuCyE9UBXA5w6c4V8ckWAKfywF3i3AU5KHt1ZZwrkbWTMJcmyOIFjxPAO5ELTgGP8aMHeSeSwQx4SDR4rw9IWprhx4UV4OUn1k/Fs+ZzFPcUXh8SD5XgHpIPhdfnty6+H3s+gJ8LI/mQ5aF3tB+M5IPdoXKeBPLhp4I2IsiDWkf4m8rsr+d68LMvVz8au6BPPYz7gO0bAAD//wMAR/eaPg==",
            "0eJyM1dtuhCAQBuB3mWsuBDy/StNsXJc0tCsaD02N8d0L4kXrkvBfivg5wMyw0f25qGHUZqZ6I932ZqL6baNJf5jm6cZM0ymqaVi64bNpv4jRvA5uRM+qo52RNg/1QzXf3xkpM+tZK28cD+vNLN1djXYCC1hDP9kPeuP+ZBEpGK12qrTuQ4+q9e+Snb1wAuB47rk8zkmAE4XnyjiXIlzquSrOZchiq4MTSZzLkaPw0Qke5wpksf5khfjPpQGuRLgsyIWiqxCugjmeIGdReu+SxyLkIXXBz+VmQHwCT72rF4wPqozEe0Ch8RSvNMhDakNy7xWAhxSH2xTnlcD+QdVxekArcKkV90rcq/D8k0AzcKmArlcC3UAg9SF8o796ofNwjQj10pd+YO+4496r/1ycjL7VOJ0T9l8AAAD//wMAYfxmNQ==",
            "0eJyUl1tugzAQAO/ibz7wi9dVqqrKw6rcFgcFUjWKuHtxTKVGQWL4DCGTtXdn176J/dfFdWcfBtHchD+cQi+al5vo/XvYfcVnYdc60Yju0nYfu8OnyMRw7eITP7hWjJnw4eh+RCPH10y4MPjBu8S4f7i+hUu7d+fphWyB1Z366QenEP9pgugiE9fpVT1xj/7sDum7fMyecIrjVL6O0wQnE06u4wzAqRmn1nGW4EzCgb0rCK5MOLOOKzekAuCqDXtnH3FmAVdvyKxdj07mgCfn1RaAJzcko3zkqSUeEUPpxKtAfMQMVd95GogmiRpacx5yI+VXS7B/SA6beMA1SexQKvFIvVS8nhEP+ZHqT5egL+e8/nQNeBvmhsnX8xu3msZnyBzSvF4MWa/h+UU8y/NrgW+q4P3PgkkZWxv1F/GIH2bmkfzWfPZaMD808qPiPOTHhviIH7EJRR7oL7GV03q2oL/EpeB6AfNNo/lRLvKW+ktsRevrrXl8yI85v6AfaOJHHKoTryAHZzQ/8kXe0v6ZnNcziS+qTvNbyPXjpNkwPwrQXwzxQ848cHo2yI/UnwvgryF+/NUL8Neg89W83ic/pjvm/d7Z/Lu4ZuLbnfv5hfEXAAD//wMAIsjYKA==",
        };

        public static IEnumerable<object[]> BlueprintsWithIsolatedAreasIndexes = Enumerable
            .Range(0, BlueprintsWithIsolatedAreas.Count)
            .Select(i => new object[] { i });

        [Theory]
        [MemberData(nameof(AllPipeStrategiesTestData))]
        public void AllowsBlueprintWithNonBlockingIsolatedArea(PipeStrategy strategy)
        {
            var options = OilFieldOptions.ForMediumElectricPole;
            options.PipeStrategies = new List<PipeStrategy> { strategy };
            var blueprintString = "0eJyU1dtuhCAQBuB3mWsudEBdfZWm2bguaWhXNB6aGuO7FxcuejDx51LEzxHnh5Vuj1n3g7ETVSuZprMjVS8rjebN1o99zNatpor6ue3f6+aDBE1Lv4+YSbe0CTL2rr+oSrdXQdpOZjLaG8+L5Wrn9qYHN0EcWH03ugc6u7/JIawELW6qdO7dDLrx95JN/OMY4CTDnES4AuYUwiWey39zfMBlEdUBXA5waeCK848tAE7lnruccxekUUJ15TlXImsnYS5NkMXzHieAF5ELTgGP8daDvIhkMAMeEg0O9QFJSzO8XVgBHhIOqXAPSkeCe0g8VER9SD449DOwGey/7nz9ykPvaK9iKB+uCZwngX7miIMD8qB8+N1PZn89d2Y+z9Hqx0Es6FMPY5iwfQMAAP//AwAyF4Ax";

            var blueprint = ParseBlueprint.Execute(blueprintString);

            // Act
            Planner.Execute(options, blueprint);
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
        private static string BigListFilePath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "big-list.txt");

        private static IReadOnlyList<string> BlueprintStrings { get; } = ParseBlueprint.ReadBlueprintFile(DataFilePath);
        private static IReadOnlyList<string> BigListBlueprintStrings { get; } = ParseBlueprint.ReadBlueprintFile(BigListFilePath);

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

        public static TheoryData<int> BigListIndexTestData
        {
            get
            {
                var theoryData = new TheoryData<int>();
                foreach (var blueprintIndex in Enumerable.Range(0, BigListBlueprintStrings.Count))
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
