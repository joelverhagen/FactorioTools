using System.Text;

namespace Knapcode.FactorioTools.OilField;

[UsesVerify]
public class BasePlannerTest : BaseTest
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

    public static PlannerResult ExecuteAllStrategies(string blueprintString)
    {
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        options.PipeStrategies = OilFieldOptions.AllPipeStrategies.ToTableList();
        options.BeaconStrategies = OilFieldOptions.AllBeaconStrategies.ToTableList();
        var blueprint = ParseBlueprint.Execute(blueprintString);

        return Planner.Execute(options, blueprint);
    }

    public static string GetGridString(PlannerResult result)
    {
        var builder = new StringBuilder();

        var plans = Enumerable
            .Empty<(char Prefix, OilFieldPlan Plan)>()
            .Concat(result.Summary.SelectedPlans.EnumerateItems().Select(x => (Prefix: 'S', Plan: x)))
            .Concat(result.Summary.AlternatePlans.EnumerateItems().Select(x => (Prefix: 'A', Plan: x)))
            .Concat(result.Summary.UnusedPlans.EnumerateItems().Select(x => (Prefix: ' ', Plan: x)))
            .OrderBy(x => x.Plan.PipeStrategy)
            .ThenBy(x => x.Plan.OptimizePipes)
            .ThenBy(x => x.Plan.BeaconStrategy)
            .Select(x => $"{x.Prefix} {x.Plan.ToString(includeCounts: true)}");

        foreach (var plan in plans)
        {
            builder.AppendLine(plan);
        }

        builder.AppendLine();
        result.Context.Grid.ToString(builder, spacing: 1);

        return builder.ToString();
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
