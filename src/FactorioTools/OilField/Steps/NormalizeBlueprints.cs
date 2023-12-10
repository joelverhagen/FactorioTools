namespace Knapcode.FactorioTools.OilField.Steps;

public static class NormalizeBlueprints
{
    public static void Execute(string inputPath, string existingPath)
    {
        var existing = NormalizeFile(existingPath).Select(x => x.Normalized).ToHashSet();

        var input = NormalizeFile(inputPath).Where(b => !existing.Contains(b.Normalized)).ToArray();

        File.WriteAllLines(inputPath, input.Select(b => b.Normalized).Distinct().Order().ToArray());
    }

    private static List<(string Original, string Normalized)> NormalizeFile(string inputPath)
    {
        var lines = new List<(string Original, string Normalized)>();
        foreach (var blueprintString in File.ReadAllLines(inputPath))
        {
            try
            {
                string output = Normalize(blueprintString, includeFbeOffset: false);
                lines.Add((blueprintString, output));
            }
            catch
            {
            }

        }

        return lines;
    }

    public static string Normalize(string blueprintString, bool includeFbeOffset)
    {
        var trimmed = blueprintString.Trim();
        var output = blueprintString;
        if (trimmed.Length > 0 && !trimmed.StartsWith("#"))
        {
            var blueprint = ParseBlueprint.Execute(trimmed);
            var clean = CleanBlueprint.Execute(blueprint);
            if (clean.Blueprint.Entities.Length == 0)
            {
                throw new InvalidOperationException("There are not pumpjacks in the blueprint.");
            }

            output = GridToBlueprintString.SerializeBlueprint(clean, includeFbeOffset);
        }

        return output;
    }
}
