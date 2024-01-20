using System.Text;

namespace Knapcode.FactorioTools.OilField;

public class Score : BasePlannerTest
{
    [Fact]
    public async Task HasExpectedScore()
    {
        var getOptionsAll = new[]
        {
            () => OilFieldOptions.ForSmallElectricPole,
            () => OilFieldOptions.ForMediumElectricPole,
            () => OilFieldOptions.ForSubstation,
            () => OilFieldOptions.ForBigElectricPole
        };
        var addBeaconsAll = new[] { true, false };
        var overlapBeaconsAll = new[] { true, false };

        var results = addBeaconsAll
            .Select(x => new { AddBeacons = x })
            .SelectMany(x => overlapBeaconsAll.Select(y => new { x.AddBeacons, OverlapBeacons = y }))
            .SelectMany(x => getOptionsAll.Select((y, i) => new { x.AddBeacons, x.OverlapBeacons, GetOptions = y, OptionsIndex = i }))
            .SelectMany(x => SmallListBlueprintStrings.Select(y => new { x.AddBeacons, x.OverlapBeacons, x.GetOptions, x.OptionsIndex, BlueprintString = y }))
            .Where(x => x.AddBeacons || !x.OverlapBeacons)
            .AsParallel()
            .Select(input =>
            {
                var inputBlueprint = ParseBlueprint.Execute(input.BlueprintString);

                var options = input.GetOptions();
                options.AddBeacons = input.AddBeacons;
                options.UseUndergroundPipes = true;
                options.OptimizePipes = true;
                options.ValidateSolution = false;
                options.OverlapBeacons = input.OverlapBeacons;

                options.PipeStrategies = OilFieldOptions.AllPipeStrategies.ToTableArray();
                options.BeaconStrategies = OilFieldOptions.AllBeaconStrategies.ToTableArray();

                var (context, summary) = Planner.Execute(options, inputBlueprint);

                return new
                {
                    Input = input,
                    Summary = summary,
                    PoleName = options.ElectricPoleEntityName,
                    summary.SelectedPlans[0].BeaconEffectCount,
                    summary.SelectedPlans[0].BeaconCount,
                    PipeCountWithUnderground = summary.SelectedPlans[0].PipeCount,
                    summary.SelectedPlans[0].PipeCountWithoutUnderground,
                    PoleCount = context.Grid.GetEntities().OfType<ElectricPoleCenter>().Count(),
                };
            });

        var output = new StringBuilder();

        var tableBuilder = new TableBuilder();
        tableBuilder.AddColumns(
            "Electric pole",
            "Add beacons",
            "Overlap beacons",
            "Effects",
            "Beacons",
            "Pipes (UG)",
            "Pipes (no UG)",
            "Poles");

        var addBeaconsGroups = results.GroupBy(x => x.Input.AddBeacons).OrderBy(x => x.Key);
        foreach (var addBeaconsGroup in addBeaconsGroups.OrderByDescending(x => x.Key))
        {
            var overlapBeaconsGroups = addBeaconsGroup.GroupBy(x => x.Input.OverlapBeacons);
            foreach (var overlapBeaconsGroup in overlapBeaconsGroups.OrderByDescending(x => x.Key))
            {
                var poleGroups = overlapBeaconsGroup.GroupBy(x => new { x.PoleName, x.Input.OptionsIndex });
                foreach (var poleGroup in poleGroups.OrderBy(x => x.Key.OptionsIndex))
                {
                    tableBuilder.AddRow(
                        poleGroup.Key.PoleName,
                        addBeaconsGroup.Key ? "yes" : "no",
                        addBeaconsGroup.Key ? (overlapBeaconsGroup.Key ? "yes" : "no") : "N/A",
                        poleGroup.Average(x => x.BeaconEffectCount),
                        poleGroup.Average(x => x.BeaconCount),
                        poleGroup.Average(x => x.PipeCountWithUnderground),
                        poleGroup.Average(x => x.PipeCountWithoutUnderground),
                        poleGroup.Average(x => x.PoleCount));
                }
            }

            var heading = $"Wins {(addBeaconsGroup.Key ? "with" : "without")} beacons";
            output.AppendLine(heading);
            output.AppendLine(new string('-', heading.Length));

            var planToWins = addBeaconsGroup
                .SelectMany(x => x.Summary.SelectedPlans.EnumerateItems().Concat(x.Summary.AlternatePlans.EnumerateItems()))
                .GroupBy(x => x.ToString(includeCounts: false))
                .ToDictionary(x => x.Key, x => x.Count());

            var maxWidth = planToWins.Keys.Max(p => p.Length);
            foreach ((var plan, var wins) in planToWins.OrderByDescending(p => p.Value).ThenBy(p => p.Key))
            {
                output.AppendLine($"{plan.PadLeft(maxWidth)} : {wins}");
            }
            output.AppendLine();
        }

        tableBuilder.Build(output);

        await Verify(output.ToString());
    }

    private record MeasureRow(
        string ElectricPoleEntityName,
        bool AddBeacons,
        string OverlapBeacons,
        double PipeCount,
        double PoleCount,
        double BeaconCount,
        double EffectCount);

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

        public void Build(StringBuilder builder)
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

            AppendRow(builder, columnCount, maxWidths, i => columnHeadings[i]);
            AppendRow(builder, columnCount, maxWidths, i => new string('-', maxWidths[i]));
            foreach (var row in rows)
            {
                AppendRow(builder, columnCount, maxWidths, i => row[i]?.ToString() ?? string.Empty);
            }
        }

        public string Build()
        {
            var builder = new StringBuilder();
            Build(builder);
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

                if (i == columnCount - 1)
                {
                    builder.Append(" |");
                }
            }

            builder.AppendLine();
        }
    }
}
