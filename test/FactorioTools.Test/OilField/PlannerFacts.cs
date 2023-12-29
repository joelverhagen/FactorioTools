using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

[UsesVerify]
public class BasePlannerFacts : BaseFacts
{
    public static IEnumerable<object[]> AllPipeStrategiesTestData => OilFieldOptions
        .AllPipeStrategies
        .Select(x => new object[] { x });

    public static IReadOnlyDictionary<string, Func<OilFieldOptions>> ElectricPoleToOptions { get; } = new[]
    {
        () => OilFieldOptions.ForSmallElectricPole,
        () => OilFieldOptions.ForMediumElectricPole,
        () => OilFieldOptions.ForBigElectricPole,
        () => OilFieldOptions.ForSubstation,
    }.ToDictionary(o => o().ElectricPoleEntityName, o => o);

    public static string SmallListFilePath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "small-list.txt");
    public static string BigListFilePath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "big-list.txt");

    public static IReadOnlyList<string> SmallListBlueprintStrings { get; } = ParseBlueprint.ReadBlueprintFile(SmallListFilePath);
    public static IReadOnlyList<string> BigListBlueprintStrings { get; } = ParseBlueprint.ReadBlueprintFile(BigListFilePath);

    public static Context ExecuteAllStrategies(string blueprintString)
    {
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        options.PipeStrategies = OilFieldOptions.AllPipeStrategies.Where(x => x == PipeStrategy.FbeOriginal).ToList();
        options.BeaconStrategies = OilFieldOptions.AllBeaconStrategies.Where(x => x == BeaconStrategy.FbeOriginal).ToList();
        var blueprint = ParseBlueprint.Execute(blueprintString);

        var (context, _) = Planner.Execute(options, blueprint);
        return context;
    }

    public static string GetGridString(Context context)
    {
        Assert.NotNull(context?.Grid);
        using var stringWriter = new StringWriter();
        context.Grid!.WriteTo(stringWriter, spacing: 1);
        return stringWriter.ToString();
    }

    public static TheoryData<int> SmallListIndexTestData
    {
        get
        {
            var theoryData = new TheoryData<int>();
            foreach (var blueprintIndex in Enumerable.Range(0, SmallListBlueprintStrings.Count))
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

            foreach (var blueprintIndex in Enumerable.Range(0, SmallListBlueprintStrings.Count))
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
