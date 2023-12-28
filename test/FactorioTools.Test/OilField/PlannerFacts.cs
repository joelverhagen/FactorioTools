using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

[UsesVerify]
public class BasePlannerFacts : BaseFacts
{
    public static IReadOnlyList<string> BlueprintsWithIsolatedAreas = new[]
    {
        "0eJyU1UluhDAQBdC71NoLKBtouEoUtWjaipw0BjFEQYi7x26zyIDkzxIwj/LwqZVuj1n3g7ETVSuZprMjVS8rjebN1g9/z9atpor6ue3f6+aDBE1L7++YSbe0CTL2rr+oSrdXQdpOZjI6GM+L5Wrn9qYHN0AcWH03uhc667/kEFaCFjdUOvduBt2EZ8km/nEMcJJhTiJcAXMK4ZLA5b85PuCyE9UBXA5w6c4V8ckWAKfywF3i3AU5KHt1ZZwrkbWTMJcmyOIFjxPAO5ELTgGP8aMHeSeSwQx4SDR4rw9IWprhx4UV4OUn1k/Fs+ZzFPcUXh8SD5XgHpIPhdfnty6+H3s+gJ8LI/mQ5aF3tB+M5IPdoXKeBPLhp4I2IsiDWkf4m8rsr+d68LMvVz8au6BPPYz7gO0bAAD//wMAR/eaPg==",
        "0eJyM1dtuhCAQBuB3mWsuBDy/StNsXJc0tCsaD02N8d0L4kXrkvBfivg5wMyw0f25qGHUZqZ6I932ZqL6baNJf5jm6cZM0ymqaVi64bNpv4jRvA5uRM+qo52RNg/1QzXf3xkpM+tZK28cD+vNLN1djXYCC1hDP9kPeuP+ZBEpGK12qrTuQ4+q9e+Snb1wAuB47rk8zkmAE4XnyjiXIlzquSrOZchiq4MTSZzLkaPw0Qke5wpksf5khfjPpQGuRLgsyIWiqxCugjmeIGdReu+SxyLkIXXBz+VmQHwCT72rF4wPqozEe0Ch8RSvNMhDakNy7xWAhxSH2xTnlcD+QdVxekArcKkV90rcq/D8k0AzcKmArlcC3UAg9SF8o796ofNwjQj10pd+YO+4496r/1ycjL7VOJ0T9l8AAAD//wMAYfxmNQ==",
        "0eJyUl1tugzAQAO/ibz7wi9dVqqrKw6rcFgcFUjWKuHtxTKVGQWL4DCGTtXdn176J/dfFdWcfBtHchD+cQi+al5vo/XvYfcVnYdc60Yju0nYfu8OnyMRw7eITP7hWjJnw4eh+RCPH10y4MPjBu8S4f7i+hUu7d+fphWyB1Z366QenEP9pgugiE9fpVT1xj/7sDum7fMyecIrjVL6O0wQnE06u4wzAqRmn1nGW4EzCgb0rCK5MOLOOKzekAuCqDXtnH3FmAVdvyKxdj07mgCfn1RaAJzcko3zkqSUeEUPpxKtAfMQMVd95GogmiRpacx5yI+VXS7B/SA6beMA1SexQKvFIvVS8nhEP+ZHqT5egL+e8/nQNeBvmhsnX8xu3msZnyBzSvF4MWa/h+UU8y/NrgW+q4P3PgkkZWxv1F/GIH2bmkfzWfPZaMD808qPiPOTHhviIH7EJRR7oL7GV03q2oL/EpeB6AfNNo/lRLvKW+ktsRevrrXl8yI85v6AfaOJHHKoTryAHZzQ/8kXe0v6ZnNcziS+qTvNbyPXjpNkwPwrQXwzxQ848cHo2yI/UnwvgryF+/NUL8Neg89W83ic/pjvm/d7Z/Lu4ZuLbnfv5hfEXAAD//wMAIsjYKA==",
    };

    public static IEnumerable<object[]> BlueprintsWithIsolatedAreasIndexes = Enumerable
        .Range(0, BlueprintsWithIsolatedAreas.Count)
        .Select(i => new object[] { i });

    public static IEnumerable<object[]> AllPipeStrategiesTestData => Enum
        .GetValues<PipeStrategy>()
        .Select(x => new object[] { x });

    public static IReadOnlyDictionary<string, Func<OilFieldOptions>> ElectricPoleToOptions { get; } = new[]
    {
            () => OilFieldOptions.ForSmallElectricPole,
            () => OilFieldOptions.ForMediumElectricPole,
            () => OilFieldOptions.ForBigElectricPole,
            () => OilFieldOptions.ForSubstation,
        }.ToDictionary(o => o().ElectricPoleEntityName, o => o);

    public static string DataFilePath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "blueprints.txt");
    public static string BigListFilePath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "big-list.txt");

    public static IReadOnlyList<string> BlueprintStrings { get; } = ParseBlueprint.ReadBlueprintFile(DataFilePath);
    public static IReadOnlyList<string> BigListBlueprintStrings { get; } = ParseBlueprint.ReadBlueprintFile(BigListFilePath);

    public static string GetGridString(Context context)
    {
        Assert.NotNull(context?.Grid);
        using var stringWriter = new StringWriter();
        context.Grid!.WriteTo(stringWriter, spacing: 1);
        return stringWriter.ToString();
    }

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
                            if (!addBeacons && overlapBeacons)
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
