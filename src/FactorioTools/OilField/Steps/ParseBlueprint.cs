using System.IO.Compression;
using System.Text.Json;
using Knapcode.FactorioTools.OilField.Data;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class ParseBlueprint
{
    public static List<string> ReadBlueprintFile(string fileName)
    {
        return File
            .ReadAllLines(fileName)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0 && !x.StartsWith("#"))
            .ToList();
    }

    public static BlueprintRoot Execute(string blueprintString)
    {
        if (blueprintString[0] != '0')
        {
            throw new NotSupportedException("Input blueprint does not have the expected version byte of '0'.");
        }

        var bytes = Convert.FromBase64String(blueprintString.Substring(1)); // skip the version byte
        using var inputStream = new MemoryStream(bytes);
        using var zlibStream = new ZLibStream(inputStream, CompressionMode.Decompress);
        using var streamReader = new StreamReader(zlibStream);
        var json = streamReader.ReadToEnd();

        var root = JsonSerializer.Deserialize<BlueprintRoot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        if (root == null)
        {
            throw new InvalidDataException("The blueprint JSON deserialized as null.");
        }

        return root;
    }
}
