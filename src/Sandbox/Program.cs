using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.OilField.Grid;
using Knapcode.FactorioTools.OilField.Steps;

public partial class Program
{
    private static readonly string ExistingDataPath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "blueprints.txt");
    private static readonly string BigListDataPath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "big-list.txt");

    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "normalize")
        {
            NormalizeBlueprints.Execute(BigListDataPath, ExistingDataPath);
        }
        else if (args.Length > 0 && args[0] == "measure")
        {
            Measure();
        }
        else
        {
            Sandbox();
        }
    }

    private static string GetRepositoryRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir is not null)
        {
            if (Directory.GetFiles(dir).Select(Path.GetFileName).Contains("FactorioTools.sln"))
            {
                return dir;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException("Could not find the repository root. Current directory: " + Directory.GetCurrentDirectory());
    }

    private static void Sandbox()
    {
        var blueprintStringsAll = ParseBlueprint.ReadBlueprintFile(BigListDataPath).ToArray();
        var blueprintStrings = blueprintStringsAll;
        // var blueprintStrings = new[] { blueprintStringsAll[1] };
        // var blueprintStrings = blueprintStringsAll.Take(5).ToArray();
        // var blueprintStrings = Enumerable.Repeat(blueprintStringsAll[1], 50).ToArray();

        // var optionsAll = new[] { Options.ForSmallElectricPole, Options.ForMediumElectricPole, Options.ForSubstation, Options.ForBigElectricPole };
        // var optionsAll = new[] { Options.ForSmallElectricPole };
        var optionsAll = new[] { OilFieldOptions.ForMediumElectricPole };
        // var optionsAll = new[] { OilFieldOptions.ForBigElectricPole };

        // var addBeaconsAll = new[] { true, false };
        var addBeaconsAll = new[] { true };
        // var addBeaconsAll = new[] { false };

        foreach (var addBeacons in addBeaconsAll)
        {
            foreach (var options in optionsAll)
            {
                for (int i = 0; i < blueprintStrings.Length; i++)
                {
                    Console.WriteLine("index " + i);
                    string? blueprintString = blueprintStrings[i];

                    if (blueprintStrings.Length == 1)
                    {
                        Console.WriteLine(blueprintString);
                    }

                    /*
                    blueprintString = NormalizeBlueprints.Normalize(blueprintString, includeFbeOffset: true);

                    if (blueprintStrings.Length == 1)
                    {
                        Console.WriteLine(blueprintString);
                    }
                    */

                    var inputBlueprint = ParseBlueprint.Execute(blueprintString);

                    // var options = Options.ForSubstation;
                    // options.ElectricPoleWidth = 3;
                    // options.ElectricPoleHeight = 3;
                    // options.ElectricPoleSupplyWidth = 9;
                    // options.ElectricPoleSupplyHeight = 9;
                    // options.AddBeacons = addBeacons;
                    // options.UseUndergroundPipes = options.AddBeacons;
                    // options.OptimizePipes = true;
                    // options.ValidateSolution = true;
                    // options.OverlapBeacons = true;
                    // options.BeaconStrategies.Remove(BeaconStrategy.Fbe);
                    // options.PipeStrategies = new HashSet<PipeStrategy> { PipeStrategy.Fbe };
                    // options.BeaconStrategies = new HashSet<BeaconStrategy> { BeaconStrategy.Snug };

                    (var context, _) = Planner.Execute(options, inputBlueprint);

                    if (blueprintStrings.Length == 1)
                    {
                        var newBlueprint = GridToBlueprintString.Execute(context, addFbeOffset: false);
                        Console.WriteLine(newBlueprint);
                    }
                }
            }
        }
    }

    private static void Measure()
    {
        var blueprintStringsAll = ParseBlueprint.ReadBlueprintFile(ExistingDataPath).ToArray();
        var blueprintStrings = blueprintStringsAll;
        var optionsAll = new[] { OilFieldOptions.ForSmallElectricPole, OilFieldOptions.ForMediumElectricPole, OilFieldOptions.ForSubstation, OilFieldOptions.ForBigElectricPole };

        var outputs = new List<string>();

        var addBeaconsAll = new[] { true, false };
        var overlapBeaconsAll = new[] { true, false };

        foreach (var addBeacons in addBeaconsAll)
        {
            var planToWins = new Dictionary<string, int>();

            foreach (var overlapBeacons in overlapBeaconsAll)
            {
                if (!addBeacons && !overlapBeacons)
                {
                    continue;
                }

                foreach (var options in optionsAll)
                {
                    var pipeSum = 0;
                    var poleSum = 0;
                    var beaconSum = 0;
                    var effectSum = 0;
                    var blueprintCount = 0;
                    for (int i = 0; i < blueprintStrings.Length; i++)
                    {
                        string? blueprintString = blueprintStrings[i];
                        var inputBlueprint = ParseBlueprint.Execute(blueprintString);

                        options.AddBeacons = addBeacons;
                        options.UseUndergroundPipes = options.AddBeacons;
                        options.OptimizePipes = true;
                        options.ValidateSolution = false;
                        options.OverlapBeacons = overlapBeacons;

                        var (context, summary) = Planner.Execute(options, inputBlueprint);

                        var pipeCount = context.Grid.EntityToLocation.Keys.OfType<Pipe>().Count();
                        var poleCount = context.Grid.EntityToLocation.Keys.OfType<ElectricPoleCenter>().Count();
                        var beaconCount = context.Grid.EntityToLocation.Keys.OfType<BeaconCenter>().Count();
                        var effectCount = summary.SelectedPlans[0].BeaconEffectCount;

                        var plans = summary
                            .SelectedPlans
                            .Select(p => string.Join(" -> ", new[]
                            {
                                p.PipeStrategy switch
                                {
                                    PipeStrategy.Fbe => "FBE",
                                    PipeStrategy.ConnectedCentersDelaunay => "CC-DT",
                                    PipeStrategy.ConnectedCentersDelaunayMst => "CC-DT-MST",
                                    PipeStrategy.ConnectedCentersFlute => "CC-FLUTE",
                                    _ => throw new NotImplementedException(),
                                },
                                p.OptimizePipes ? "optimize" : "",
                                p.BeaconStrategy switch
                                {
                                    null => "",
                                    BeaconStrategy.Fbe => "FBE*",
                                    BeaconStrategy.FbeOriginal => "FBE",
                                    BeaconStrategy.Snug => "SNUG",
                                    _ => throw new NotImplementedException(),
                                },
                            }.Where(p => !string.IsNullOrEmpty(p))));

                        foreach (var plan in plans)
                        {
                            if (!planToWins.TryGetValue(plan, out var count))
                            {
                                planToWins.Add(plan, 1);
                            }
                            else
                            {
                                planToWins[plan] = count + 1;
                            }
                        }

                        Console.WriteLine($"{pipeCount},{poleCount},{beaconCount},{effectCount}");

                        pipeSum += pipeCount;
                        poleSum += poleCount;
                        beaconSum += beaconCount;
                        effectSum += effectCount;
                        blueprintCount++;
                    }

                    outputs.Add($"{options.ElectricPoleEntityName} | {options.AddBeacons} | {(options.AddBeacons ? options.OverlapBeacons.ToString() : "N/A")} | {pipeSum * 1.0 / blueprintCount} | {poleSum * 1.0 / blueprintCount} | {beaconSum * 1.0 / blueprintCount} | {effectSum * 1.0 / blueprintCount}");
                }
            }

            Console.WriteLine();
            var maxWidth = planToWins.Keys.Max(p => p.Length);
            foreach ((var plan, var wins) in planToWins.OrderBy(p => p.Value))
            {
                Console.WriteLine($"{plan.PadLeft(maxWidth)} : {wins}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("Electric pole | Add beacons | Overlap beacons | Pipe count | Pole count | Beacon count | Effect count");
        Console.WriteLine("------------- | ----------- | --------------- | ---------- | ---------- | ------------ | ------------");
        for (int i = 0; i < outputs.Count; i++)
        {
            Console.WriteLine(outputs[i]);
        }
    }
}
