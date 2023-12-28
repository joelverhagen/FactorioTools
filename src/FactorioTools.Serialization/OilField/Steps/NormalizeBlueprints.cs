using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Knapcode.FactorioTools.OilField.Steps;

public static class NormalizeBlueprints
{
    public static void Execute(string inputPath, string existingPath)
    {
        var existing = NormalizeFile(existingPath).ToLookup(x => x.Normalized);

        var input = NormalizeFile(inputPath);

        var output = new List<string>();
        var added = new HashSet<string>();
        var orderedInput = input
            .Concat(existing.SelectMany(g => g))
            .Where(x => x.Valid)
            .OrderBy(x => existing[x.Normalized].Select(x => x.Index).DefaultIfEmpty(int.MaxValue).Min())
            .ThenBy(x => x.Index);

        foreach (var line in orderedInput)
        {
            if (line.Normalized is null)
            {
                continue;
            }

            if (added.Add(line.Normalized))
            {
                output.Add(line.Normalized);
            }
        }


        File.WriteAllLines(inputPath, output);
    }

    private static List<(string Original, string? Normalized, int Index, bool Valid)> NormalizeFile(string inputPath)
    {
        var lines = new List<(string Original, string? Normalized, int Index, bool Valid)>();
        var index = 0;
        foreach (var blueprintString in File.ReadLines(inputPath))
        {
            try
            {
                var valid = TryNormalize(blueprintString, includeFbeOffset: false, out var output);
                lines.Add((blueprintString, output, index, valid));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            index++;
        }

        return lines;
    }

    private static bool TryNormalize(string blueprintString, bool includeFbeOffset, out string? normalized)
    {
        var trimmed = blueprintString.Trim();

        if (trimmed.Length > 0 && !trimmed.StartsWith("#"))
        {
            var blueprint = ParseBlueprint.Execute(trimmed);
            var clean = CleanBlueprint.Execute(blueprint);
            if (clean.Blueprint.Entities.Length == 0)
            {
                throw new InvalidOperationException("There are not pumpjacks in the blueprint.");
            }

            normalized = GridToBlueprintString.SerializeBlueprint(clean, includeFbeOffset);
            return true;
        }

        normalized = null;
        return false;
    }
}
