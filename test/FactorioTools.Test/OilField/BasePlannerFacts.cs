﻿using System.Text;
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

    public static (Context Context, OilFieldPlanSummary Summary) ExecuteAllStrategies(string blueprintString)
    {
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        options.PipeStrategies = OilFieldOptions.AllPipeStrategies.ToList();
        options.BeaconStrategies = OilFieldOptions.AllBeaconStrategies.ToList();
        var blueprint = ParseBlueprint.Execute(blueprintString);

        return Planner.Execute(options, blueprint);
    }

    public static string GetGridString((Context Context, OilFieldPlanSummary Summary) result)
    {
        var builder = new StringBuilder();

        foreach (var plan in result.Summary.SelectedPlans)
        {
            builder.Append("= ");
            builder.AppendLine(plan.ToString(includeCounts: true));
        }

        foreach (var plan in result.Summary.AlternatePlans)
        {
            builder.Append("* ");
            builder.AppendLine(plan.ToString(includeCounts: true));
        }

        foreach (var plan in result.Summary.UnusedPlans)
        {
            builder.Append("- ");
            builder.AppendLine(plan.ToString(includeCounts: true));
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