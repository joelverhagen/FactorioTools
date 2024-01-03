using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

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

    public static Blueprint Execute(string blueprintString)
    {
        if (blueprintString[0] != '0')
        {
            throw new FactorioToolsException("Input blueprint does not have the expected version byte of '0' at the beginning.", badInput: true);
        }

        BlueprintRoot? root;
        bool hadMissingPadding = false;
        bool looksLikeJson = true;
        try
        {
            var base64 = blueprintString.Substring(1); // skip the version byte
            var missingPadding = (4 - (base64.Length % 4)) % 4;
            if (missingPadding > 0)
            {
                base64 += new string('=', missingPadding);
                hadMissingPadding = true;
            }

            var bytes = Convert.FromBase64String(base64);

            using var inputStream = new MemoryStream(bytes);
            using var zlibStream = new ZLibStream(inputStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(zlibStream);
            var json = streamReader.ReadToEnd();

            looksLikeJson = json.StartsWith('{');

            var context = BlueprintSerializationContext.Default;
            root = JsonSerializer.Deserialize(
                json,
                typeof(BlueprintRoot),
                context) as BlueprintRoot;
        }
        catch (Exception ex) when (ex is FormatException || ex is JsonException || hadMissingPadding || looksLikeJson)
        {
            throw new FactorioToolsException("Input blueprint string could not be fully decoded. Are you sure you copied the whole blueprint?", ex, badInput: true);
        }
        catch (Exception ex)
        {
            throw new FactorioToolsException("Input blueprint string could not be decoded.", ex, badInput: true);
        }

        if (root == null)
        {
            throw new FactorioToolsException("The blueprint JSON deserialized as null.", badInput: true);
        }

        if (root.BlueprintBook is not null)
        {
            throw new FactorioToolsException("The blueprint provided contains a blueprint book, not an individual blueprint.", badInput: true);
        }

        if (root.Blueprint is null)
        {
            throw new FactorioToolsException("No blueprint was found in the deserialized JSON.", badInput: true);
        }

        return root.Blueprint;
    }
}
