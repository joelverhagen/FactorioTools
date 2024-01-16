using System.Text;
using Knapcode.FactorioTools.OilField;

public partial class Program
{
    private static readonly string SmallListDataPath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "small-list.txt");
    private static readonly string BigListDataPath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "big-list.txt");

    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "normalize")
        {
            NormalizeBlueprints.Execute(BigListDataPath, SmallListDataPath);
        }
        else if (args.Length > 0 && args[0] == "sample")
        {
            var (context, summary) = Planner.ExecuteSample();
            Console.WriteLine(context.Grid.ToString());
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