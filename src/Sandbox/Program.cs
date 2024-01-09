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
        else if (args.Length > 0 && args[0] == "measure")
        {
            Measure();
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
                        var newBlueprint = GridToBlueprintString.Execute(context, addFbeOffset: false);
                        Console.WriteLine(newBlueprint);
                    }
                }
            }
        }
    }

    private static void Measure()
    {
        var blueprintStringsAll = ParseBlueprint.ReadBlueprintFile(SmallListDataPath).ToArray();
        var blueprintStrings = blueprintStringsAll;
        var optionsAll = new[] { OilFieldOptions.ForSmallElectricPole, OilFieldOptions.ForMediumElectricPole, OilFieldOptions.ForSubstation, OilFieldOptions.ForBigElectricPole };

        var outputs = new List<MeasureResult>();

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
                        Console.WriteLine("Index " + i);
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

                        foreach (var plan in summary.SelectedPlans.Concat(summary.AlternatePlans).Select(p => p.ToString()))
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

                    outputs.Add(new MeasureResult(
                        options.ElectricPoleEntityName,
                        options.AddBeacons,
                        options.AddBeacons ? (options.OverlapBeacons ? "yes" : "no") : "N/A",
                        pipeSum * 1.0 / blueprintCount,
                        poleSum * 1.0 / blueprintCount,
                        beaconSum * 1.0 / blueprintCount,
                        effectSum * 1.0 / blueprintCount));
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

        var tableBuilder = new TableBuilder();
        tableBuilder.AddColumns(
            "Electric pole",
            "Add beacons",
            "Overlap beacons",
            "Pipe count",
            "Pole count",
            "Beacon count",
            "Effect count");

        foreach (var output in outputs)
        {
            tableBuilder.AddRow(
                output.ElectricPoleEntityName,
                output.AddBeacons ? "yes" : "no",
                output.OverlapBeacons,
                output.PipeCount,
                output.PoleCount,
                output.BeaconCount,
                output.EffectCount);
        }

        Console.WriteLine(tableBuilder.Build());
    }

    public class TableBuilder
    {
        private readonly List<string> _columns = new();
        private readonly List<IEnumerable<object>> _rows = new();

        public void AddColumn(string label)
        {
            _columns.Add(label);
        }

        public void AddColumns(params string[] labels)
        {
            for (var i = 0; i < labels.Length; i++)
            {
                AddColumn(labels[i]);
            }
        }

        public void AddRow(params object[] row)
        {
            _rows.Add(row);
        }

        public void AddRow<T>(IEnumerable<T> row)
        {
            _rows.Add(row.Cast<object>());
        }

        public string Build()
        {
            var rows = _rows.Select(x => x.ToList()).ToList();
            var columnCount = Math.Max(_columns.Count, rows.Select(x => x.Count).DefaultIfEmpty(0).Max());

            var maxWidths = new int[columnCount];
            var columnHeadings = new string[columnCount];

            for (var i = 0; i < columnCount; i++)
            {
                columnHeadings[i] = _columns.ElementAtOrDefault(i) ?? $"(column {i + 1})";
                maxWidths[i] = _rows
                    .Select(x => x.ElementAtOrDefault(i)?.ToString() ?? string.Empty)
                    .Append(columnHeadings[i])
                    .Max(x => x.Length);
            }

            var builder = new StringBuilder();
            AppendRow(builder, columnCount, maxWidths, i => columnHeadings[i]);
            AppendRow(builder, columnCount, maxWidths, i => new string('-', maxWidths[i]));
            foreach (var row in rows)
            {
                AppendRow(builder, columnCount, maxWidths, i => row[i]?.ToString() ?? string.Empty);
            }

            return builder.ToString();
        }

        private static void AppendRow(StringBuilder builder, int columnCount, int[] maxWidths, Func<int, string> getValue)
        {
            for (var i = 0; i < columnCount; i++)
            {
                if (i == 0)
                {
                    builder.Append("| ");
                }
                else
                {
                    builder.Append(" | ");
                }

                builder.Append(getValue(i).PadRight(maxWidths[i]));

                if (i ==  columnCount - 1)
                {
                    builder.Append(" |");
                }
            }

            builder.AppendLine();
        }
    }

    private record MeasureResult(
        string ElectricPoleEntityName,
        bool AddBeacons,
        string OverlapBeacons,
        double PipeCount,
        double PoleCount,
        double BeaconCount,
        double EffectCount);
}