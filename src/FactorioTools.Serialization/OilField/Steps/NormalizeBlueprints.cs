using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Knapcode.FactorioTools.OilField;

public static class NormalizeBlueprints
{
    public static void Execute(string inputPath, string excludePath)
    {
        var exclude = NormalizeFile(excludePath).ToLookup(x => x.Normalized);

        var input = NormalizeFile(inputPath);

        var output = new List<string>();
#if USE_HASHSETS
        var added = new HashSet<string>();
#else
        var added = new Dictionary<string, bool>();
#endif
        var orderedInput = input
            .Concat(exclude.SelectMany(g => g))
            .Where(x => x.Valid)
            .Where(x => !exclude[x.Normalized].Any())
            .OrderBy(x => x.Index);

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
                Console.WriteLine();
                Console.WriteLine(ex);
                Console.WriteLine();

                string response;
                do
                {
                    Console.Write("A blueprint could not be parsed. See the error above. Keep in file? (y/N) ");
                    response = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
                }
                while (response != "y" && response != "n");

                if (response == "y")
                {
                    lines.Add((blueprintString, blueprintString, index, true));
                }
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
            if (clean.Entities.Length == 0)
            {
                throw new InvalidOperationException("There are no pumpjacks in the blueprint.");
            }

            normalized = GridToBlueprintString.SerializeBlueprint(clean, includeFbeOffset);
            return true;
        }

        normalized = null;
        return false;
    }
}
