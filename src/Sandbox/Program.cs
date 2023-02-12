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
        var blueprintStringsAll = File.ReadAllLines(DataPath).Select(x => x.Trim()).Where(x => x.Length > 0 && !x.StartsWith("#")).ToArray();
        var blueprintStrings = blueprintStringsAll;
        // var blueprintStrings = new[] { blueprintStringsAll[1] };
        // var blueprintStrings = blueprintStringsAll.Take(20).ToArray();
        // var blueprintStrings = Enumerable.Repeat(blueprintStringsAll[1], 20).ToArray();

        var pipeSum = 0;
        var poleSum = 0;
        var beaconSum = 0;
        var blueprintCount = 0;
        for (int i = 0; i < blueprintStrings.Length; i++)
        {
            string? blueprintString = blueprintStrings[i];
            var inputBlueprint = ParseBlueprint.Execute(blueprintString);

            var options = Options.ForMediumElectricPole;
            // var options = Options.ForSubstation;
            // options.ElectricPoleWidth = 3;
            // options.ElectricPoleHeight = 3;
            // options.ElectricPoleSupplyWidth = 9;
            // options.ElectricPoleSupplyHeight = 9;
            options.AddBeacons = false;
            options.UseUndergroundPipes = false;
            // options.ValidateSolution = true;

            var context = Planner.Execute(options, inputBlueprint);

            var pipeCount = context.Grid.EntityToLocation.Keys.OfType<Pipe>().Count();
            var poleCount = context.Grid.EntityToLocation.Keys.OfType<ElectricPoleCenter>().Count();
            var beaconCount = context.Grid.EntityToLocation.Keys.OfType<BeaconCenter>().Count();

            // Console.WriteLine($"{pipeCount} {poleCount}");
            Console.WriteLine($"{pipeCount}");

            pipeSum += pipeCount;
            poleSum += poleCount;
            beaconSum += beaconCount;
            blueprintCount++;

            if (blueprintStrings.Length == 1)
            {
                var newBlueprint = GridToBlueprintString.Execute(context);
                Console.WriteLine(newBlueprint);
            }
        }

        Console.WriteLine($"{pipeSum * 1.0 / blueprintCount} {poleSum * 1.0 / blueprintCount} {beaconSum * 1.0 / blueprintCount}");
        // Console.WriteLine($"{pipeSum * 1.0 / blueprintCount}");
    }
}
