using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.OilField.Grid;
using Knapcode.FactorioTools.OilField.Steps;

internal partial class Program
{
    private const string DataPath = "blueprints.txt";

    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[1] == "normalize")
        {
            NormalizeBlueprints.Execute(DataPath);
        }
        else if (args.Length > 0 && args[1] == "measure")
        {
            Measure();
        }
        else
        {
            // Measure();
            Sandbox();
        }
    }

    private static void Sandbox()
    {
        var blueprintStringsAll = ParseBlueprint.ReadBlueprintFile(DataPath).ToArray();
        // var blueprintStrings = blueprintStringsAll;
        var blueprintStrings = new[] { blueprintStringsAll[1] };
        // var blueprintStrings = blueprintStringsAll.Take(5).ToArray();
        // var blueprintStrings = Enumerable.Repeat(blueprintStringsAll[1], 50).ToArray();

        // var optionsAll = new[] { Options.ForSmallElectricPole, Options.ForMediumElectricPole, Options.ForSubstation, Options.ForBigElectricPole };
        // var optionsAll = new[] { Options.ForSmallElectricPole };
        var optionsAll = new[] { Options.ForMediumElectricPole };
        // var optionsAll = new[] { Options.ForBigElectricPole };

        // var addBeaconsAll = new[] { true, false };
        var addBeaconsAll = new[] { true };
        // var addBeaconsAll = new[] { false };

        foreach (var addBeacons in addBeaconsAll)
        {
            foreach (var options in optionsAll)
            {
                for (int i = 0; i < blueprintStrings.Length; i++)
                {
                    // Console.WriteLine("index " + i);
                    string? blueprintString = blueprintStrings[i];
                    var inputBlueprint = ParseBlueprint.Execute(blueprintString);

                    // var options = Options.ForSubstation;
                    // options.ElectricPoleWidth = 3;
                    // options.ElectricPoleHeight = 3;
                    // options.ElectricPoleSupplyWidth = 9;
                    // options.ElectricPoleSupplyHeight = 9;
                    options.AddBeacons = addBeacons;
                    options.UseUndergroundPipes = options.AddBeacons;
                    options.OptimizePipes = true;
                    options.ValidateSolution = true;
                    options.OverlapBeacons = false;
                    // options.BeaconStrategies.Remove(BeaconStrategy.FBE);
                    // options.PipeStrategies = new HashSet<PipeStrategy> { PipeStrategy.FBE };
                    // options.BeaconStrategies = new HashSet<BeaconStrategy> { BeaconStrategy.Snug };

                    var context = Planner.Execute(options, inputBlueprint);

                    if (blueprintStrings.Length == 1)
                    {
                        var newBlueprint = GridToBlueprintString.Execute(context, addOffsetCorrection: false);
                        Console.WriteLine(newBlueprint);
                    }
                }
            }
        }
    }

    private static void Measure()
    {
        var blueprintStringsAll = ParseBlueprint.ReadBlueprintFile(DataPath).ToArray();
        var blueprintStrings = blueprintStringsAll;
        var optionsAll = new[] { Options.ForSmallElectricPole, Options.ForMediumElectricPole, Options.ForSubstation, Options.ForBigElectricPole };

        var outputs = new List<string>();

        var addBeaconsAll = new[] { true, false };
        var overlapBeaconsAll = new[] { true, false };

        foreach (var addBeacons in addBeaconsAll)
        {
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

                        var context = Planner.Execute(options, inputBlueprint);

                        var pipeCount = context.Grid.EntityToLocation.Keys.OfType<Pipe>().Count();
                        var poleCount = context.Grid.EntityToLocation.Keys.OfType<ElectricPoleCenter>().Count();
                        var beaconCount = context.Grid.EntityToLocation.Keys.OfType<BeaconCenter>().Count();

                        Console.WriteLine($"{pipeCount},{poleCount},{beaconCount}");

                        pipeSum += pipeCount;
                        poleSum += poleCount;
                        beaconSum += beaconCount;
                        blueprintCount++;
                    }

                    outputs.Add($"{options.ElectricPoleEntityName} | {options.AddBeacons} | {(options.AddBeacons ? options.OverlapBeacons.ToString() : "N/A")} | {pipeSum * 1.0 / blueprintCount} | {poleSum * 1.0 / blueprintCount} | {beaconSum * 1.0 / blueprintCount}");
                }
            }
        }

        Console.WriteLine("Electric pole | Add beacons | Overlap beacons | Pipe count | Pole count | Beacon count");
        Console.WriteLine("------------- | ----------- | --------------- | ---------- | ---------- | ------------");
        for (int i = 0; i < outputs.Count; i++)
        {
            Console.WriteLine(outputs[i]);
        }
    }
}
