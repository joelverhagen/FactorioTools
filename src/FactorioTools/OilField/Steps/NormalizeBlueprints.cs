namespace Knapcode.FactorioTools.OilField.Steps;

public static class NormalizeBlueprints
{
    public static void Execute(string dataPath)
    {
        var lines = new List<string>();
        foreach (var blueprintString in File.ReadAllLines(dataPath))
        {
            string output = Normalize(blueprintString);

            lines.Add(output);
        }

        File.WriteAllLines(dataPath, lines.ToArray());
    }

    public static string Normalize(string blueprintString)
    {
        var trimmed = blueprintString.Trim();
        var output = blueprintString;
        if (trimmed.Length > 0 && !trimmed.StartsWith("#"))
        {
            var blueprint = ParseBlueprint.Execute(trimmed);
            var clean = CleanBlueprint.Execute(blueprint);
            output = GridToBlueprintString.SerializeBlueprint(clean);
        }

        return output;
    }
}
