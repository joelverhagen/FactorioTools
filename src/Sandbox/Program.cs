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
        else
        {
            Measure();
        }
    }

    private static void Measure()
    {
        var blueprintStringsAll = ParseBlueprint.ReadBlueprintFile(DataPath).ToArray();
        var blueprintStrings = blueprintStringsAll;
        // var blueprintStrings = new[] { blueprintStringsAll[42] };
        // var blueprintStrings = blueprintStringsAll.Take(20).ToArray();
        // var blueprintStrings = Enumerable.Repeat(blueprintStringsAll[1], 20).ToArray();

        // var optionsAll = new[] { Options.ForSmallElectricPole, Options.ForMediumElectricPole, Options.ForSubstation, Options.ForBigElectricPole };
        // var optionsAll = new[] { Options.ForSmallElectricPole };
        var optionsAll = new[] { Options.ForMediumElectricPole };
        // var optionsAll = new[] { Options.ForBigElectricPole };
        var outputs = new List<string>();

        foreach (var options in optionsAll)
        {
            var pipeSum = 0;
            var poleSum = 0;
            var beaconSum = 0;
            var blueprintCount = 0;
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
                options.AddBeacons = true;
                options.UseUndergroundPipes = true;
                options.OptimizePipes = true;
                options.ValidateSolution = false;
                // options.BeaconStrategies.Remove(BeaconStrategy.FBE);
                // options.PipeStrategies = new HashSet<PipeStrategy> { PipeStrategy.FBE };
                // options.BeaconStrategies = new HashSet<BeaconStrategy> { BeaconStrategy.FBE };

                var context = Planner.Execute(options, inputBlueprint);

                var pipeCount = context.Grid.EntityToLocation.Keys.OfType<Pipe>().Count();
                var poleCount = context.Grid.EntityToLocation.Keys.OfType<ElectricPoleCenter>().Count();
                var beaconCount = context.Grid.EntityToLocation.Keys.OfType<BeaconCenter>().Count();

                Console.WriteLine($"{pipeCount},{poleCount},{beaconCount}");
                // Console.WriteLine($"{pipeCount}");

                pipeSum += pipeCount;
                poleSum += poleCount;
                beaconSum += beaconCount;
                blueprintCount++;

                if (blueprintStrings.Length == 1)
                {
                    var newBlueprint = GridToBlueprintString.Execute(context, addOffsetCorrection: false);
                    Console.WriteLine(newBlueprint);
                }
            }

            outputs.Add($"{pipeSum * 1.0 / blueprintCount},{poleSum * 1.0 / blueprintCount},{beaconSum * 1.0 / blueprintCount}");
            // Console.WriteLine($"{pipeSum * 1.0 / blueprintCount}");
        }

        var maxWidth = optionsAll.Max(o => o.ElectricPoleEntityName.Length);
        for (int i = 0; i < outputs.Count; i++)
        {
            Console.WriteLine($"{optionsAll[i].ElectricPoleEntityName.PadRight(maxWidth)}: {outputs[i]}");
        }
    }
}
