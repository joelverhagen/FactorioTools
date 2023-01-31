using DelaunatorSharp;
using PumpjackPipeOptimizer.Data;
using PumpjackPipeOptimizer.Steps;

namespace PumpjackPipeOptimizer;

internal partial class Program
{
    private const string DataPath = "blueprints.txt";

    private static void Main(string[] args)
    {
        /*
        var options = new Options
        {
            ElectricPoleEntityName = EntityNames.Vanilla.Substation,
            ElectricPoleSupplyWidth = 18,
            ElectricPoleSupplyHeight = 18,
            ElectricPoleWireReach = 18,
            ElectricPoleWidth = 2,
            ElectricPoleHeight = 2,
        };
        */
        /*
        var options = new Options
        {
            ElectricPoleEntityName = EntityNames.SpaceExploration.SmallIronElectricPole,
            ElectricPoleSupplyWidth = 5,
            ElectricPoleSupplyHeight = 5,
            ElectricPoleWireReach = 7.5,
            ElectricPoleWidth = 1,
            ElectricPoleHeight = 1,
        };
        */

        var options = new Options
        {
            UseUndergroundPipes = false,
            ElectricPoleEntityName = EntityNames.Vanilla.MediumElectricPole,
            ElectricPoleSupplyWidth = 7,
            ElectricPoleSupplyHeight = 7,
            ElectricPoleWireReach = 9,
            ElectricPoleWidth = 1,
            ElectricPoleHeight = 1,
        };

        /*
        var options = new Options
        {
            ElectricPoleEntityName = EntityNames.Vanilla.BigElectricPole,
            ElectricPoleSupplyWidth = 4,
            ElectricPoleSupplyHeight = 4,
            ElectricPoleWireReach = 30,
            ElectricPoleWidth = 2,
            ElectricPoleHeight = 2,
        };
        */

        var sum = 0;
        var count = 0;
        var blueprintStringsAll = File.ReadAllLines(DataPath).Select(x => x.Trim()).Where(x => x.Length > 0 && !x.StartsWith("#")).ToList();
        // var blueprintStrings = blueprintStringsAll;
        var blueprintStrings = new[] { blueprintStringsAll[1] };
        // var blueprintStrings = new[] { blueprintStringsAll[54] };
        foreach (var blueprintString in blueprintStrings)
        {
            var inputBlueprint = ParseBlueprint.Execute(blueprintString);

            var context = InitializeContext.Execute(options, inputBlueprint);

            if (context.CenterToTerminals.Count < 2)
            {
                throw new InvalidOperationException("The must be at least two pumpjacks in the blueprint.");
            }

            // context.Grid.WriteTo(Console.Out);

            // Use Dijksta's algorithm to add good connecting pipes to the grid.
            var pipes = PlanPipes.Execute(context);

            AddPipeEntities.Execute(context.Grid, context.CenterToTerminals, pipes);

            // Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());

            sum += pipes.Count;
            count++;
            Console.WriteLine(pipes.Count);

            // Find pipe "squares" (four pipes forming a square) and try to remove one from the square.
            // PruneSquares.Execute(context, pipes);

            /*
            if (context.Options.UseUndergroundPipes)
            {
                // Substitute long stretches of pipes for underground pipes
                UseUndergroundPipes.Execute(context, pipes);
            }

            // Add electric poles to the grid.
            AddElectricPoles.Execute(context);

            Console.WriteLine();
            context.Grid.WriteTo(Console.Out);
            */
            var newBlueprint = GridToBlueprintString.Execute(context);
            // Console.WriteLine();
            // Console.WriteLine(newBlueprint);
        }

        Console.WriteLine(sum * 1.0 / count);
    }

    private static void NormalizeBlueprints()
    {
        var lines = new List<string>();
        foreach (var blueprintString in File.ReadAllLines(DataPath))
        {
            var trimmed = blueprintString.Trim();
            var output = blueprintString;
            if (trimmed.Length > 0 && !trimmed.StartsWith("#"))
            {
                var blueprint = ParseBlueprint.Execute(trimmed);
                var clean = CleanBlueprint.Execute(blueprint);
                output = GridToBlueprintString.SerializeBlueprint(clean);
            }

            lines.Add(output);
        }

        File.WriteAllLines(DataPath, lines.ToArray());
    }
}
