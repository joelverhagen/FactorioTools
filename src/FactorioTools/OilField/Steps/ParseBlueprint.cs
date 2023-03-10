using System.IO.Compression;
using System.Text.Json;
using Knapcode.FactorioTools.OilField.Data;

namespace Knapcode.FactorioTools.OilField.Steps;

public static class ParseBlueprint
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
            throw new FactorioToolsException("Input blueprint does not have the expected version byte of '0'.", badInput: true);
        }

        BlueprintRoot? root;
        try
        {
            var bytes = Convert.FromBase64String(blueprintString.Substring(1)); // skip the version byte

            using var inputStream = new MemoryStream(bytes);
            using var zlibStream = new ZLibStream(inputStream, CompressionMode.Decompress);

            var context = BlueprintSerializationContext.Default;
            root = JsonSerializer.Deserialize(
                zlibStream,
                typeof(BlueprintRoot),
                context) as BlueprintRoot;
        }
        catch (Exception ex)
        {
            throw new FactorioToolsException("Input blueprint string could not be decoded.", ex, badInput: true);
        }

        if (root == null)
        {
            throw new FactorioToolsException("The blueprint JSON deserialized as null.", badInput: true);
        }

        return root;
    }
}
