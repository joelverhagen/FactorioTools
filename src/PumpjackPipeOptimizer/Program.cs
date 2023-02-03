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
            UseUndergroundPipes = false,
            ElectricPoleEntityName = EntityNames.SpaceExploration.SmallIronElectricPole,
            ElectricPoleSupplyWidth = 5,
            ElectricPoleSupplyHeight = 5,
            ElectricPoleWireReach = 7.5,
            ElectricPoleWidth = 1,
            ElectricPoleHeight = 1,
        };
        */

        /*
        var options = new Options
        {
            UseUndergroundPipes = false,
            ElectricPoleEntityName = EntityNames.Vanilla.SmallElectricPole,
            ElectricPoleSupplyWidth = 5,
            ElectricPoleSupplyHeight = 5,
            ElectricPoleWireReach = 7.5,
            ElectricPoleWidth = 1,
            ElectricPoleHeight = 1,
        };
        */

        /*
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
        */

        var options = new Options
        {
            UseUndergroundPipes = false,
            ElectricPoleEntityName = EntityNames.Vanilla.BigElectricPole,
            ElectricPoleSupplyWidth = 4,
            ElectricPoleSupplyHeight = 4,
            ElectricPoleWireReach = 30,
            ElectricPoleWidth = 2,
            ElectricPoleHeight = 2,
        };

        /*
        var options = new Options
        {
            UseUndergroundPipes = false,
            ElectricPoleEntityName = EntityNames.Vanilla.Substation,
            ElectricPoleSupplyWidth = 18,
            ElectricPoleSupplyHeight = 18,
            ElectricPoleWireReach = 18,
            ElectricPoleWidth = 2,
            ElectricPoleHeight = 2,
        };
        */

        var pipeSum = 0;
        var blueprintCount = 0;
        var blueprintStringsAll = File.ReadAllLines(DataPath).Select(x => x.Trim()).Where(x => x.Length > 0 && !x.StartsWith("#")).ToArray();
        var blueprintStrings = blueprintStringsAll;
        // var blueprintStrings = new[] { blueprintStringsAll[1] };
        for (int i = 0; i < blueprintStrings.Length; i++)
        {
            string? blueprintString = blueprintStrings[i];
            var inputBlueprint = ParseBlueprint.Execute(blueprintString);

            var addElectricPolesFirst = false;

            while (true)
            {
                var context = InitializeContext.Execute(options, inputBlueprint);

                if (context.CenterToTerminals.Count < 2)
                {
                    throw new InvalidOperationException("The must be at least two pumpjacks in the blueprint.");
                }

                HashSet<Location>? poles = null;
                if (addElectricPolesFirst)
                {
                    poles = AddElectricPoles.Execute(context, avoidTerminals: true);
                    if (poles is null)
                    {
                        throw new InvalidOperationException("No valid placement for the electric poles could be found.");
                    }
                }

                var pipes = PlanPipes.Execute(context);

                AddPipeEntities.Execute(context.Grid, context.CenterToTerminals, pipes);

                // Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());

                if (context.Options.UseUndergroundPipes)
                {
                    // Substitute long stretches of pipes for underground pipes
                    UseUndergroundPipes.Execute(context, pipes);
                }

                // Add electric poles to the grid.
                if (poles is null)
                {
                    poles = AddElectricPoles.Execute(context, avoidTerminals: false);
                    if (poles is null)
                    {
                        addElectricPolesFirst = true;
                        continue;
                    }
                }

                // Console.WriteLine();
                // context.Grid.WriteTo(Console.Out);

                Console.WriteLine($"{pipes.Count} {poles.Count}");

                var newBlueprint = GridToBlueprintString.Execute(context);
                // Console.WriteLine();
                // Console.WriteLine(newBlueprint);

                pipeSum += pipes.Count;
                blueprintCount++;

                break;
            }
        }

        Console.WriteLine(pipeSum * 1.0 / blueprintCount);
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
