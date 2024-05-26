using System.CommandLine;
using Knapcode.FactorioTools.OilField;

public class Program
{
    private static readonly string SmallListDataPath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "small-list.txt");
    private static readonly string BigListDataPath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "big-list.txt");

    private static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("Factorio Tools command-line tool");

        var oilFieldCommand = new Command("oil-field", "oil field related subcommands");
        rootCommand.Add(oilFieldCommand);

        var normalizeCommand = new Command("normalize", "normalize the small and big list");
        oilFieldCommand.Add(normalizeCommand);

        normalizeCommand.SetHandler(() =>
        {
            NormalizeBlueprints.Execute(BigListDataPath, SmallListDataPath, allowComments: false);
            NormalizeBlueprints.Execute(SmallListDataPath, BigListDataPath, allowComments: true);
        });

        var sampleCommand = new Command("sample", "Execute the oil field planner sample");
        oilFieldCommand.Add(sampleCommand);

        sampleCommand.SetHandler(() =>
        {
            var (context, summary) = Planner.ExecuteSample();
            Console.WriteLine(context.Grid.ToString());
        });

        var sandboxCommand = new Command("sandbox", "Execute the sandbox (random test code that you write)");
        oilFieldCommand.Add(sandboxCommand);

        sandboxCommand.SetHandler(() =>
        {
            Sandbox();
        });

        await rootCommand.InvokeAsync(args);
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
        // var blueprintStrings = blueprintStringsAll;
        var blueprintStrings = new[] { blueprintStringsAll[91] };
        // var blueprintStrings = blueprintStringsAll.Reverse().Take(11).Skip(11).ToArray();
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

                    /*
                    var entityTypes = inputBlueprint
                        .Entities
                        .GroupBy(e => e.Name)
                        .ToDictionary(g => g.Key, g => g.Count())
                        .OrderByDescending(p => p.Value);
                    */

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
                    options.PipeStrategies = OilFieldOptions.AllPipeStrategies.ToList();
                    options.BeaconStrategies = OilFieldOptions.AllBeaconStrategies.ToList();

                    (var context, _) = Planner.Execute(options, inputBlueprint);

                    if (blueprintStrings.Length == 1)
                    {
                        var newBlueprint = GridToBlueprintString.Execute(context, addFbeOffset: false, addAvoidEntities: true);
                        Console.WriteLine(newBlueprint);
                    }
                }
            }
        }
    }
}