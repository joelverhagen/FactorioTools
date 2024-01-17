using System.Text.Json;

namespace Knapcode.FactorioTools.OilField;

public class CleanBlueprintTest : BasePlannerTest
{
    [Theory]
    [MemberData(nameof(BigListIndexTestData))]
    public void BigListBlueprintsAreNormalized(int blueprintIndex)
    {
        VerifySameBlueprint(BigListBlueprintStrings[blueprintIndex]);
    }

    [Theory]
    [MemberData(nameof(SmallListIndexTestData))]
    public void SmallListBlueprintsAreNormalized(int blueprintIndex)
    {
        VerifySameBlueprint(SmallListBlueprintStrings[blueprintIndex]);
    }

    private static void VerifySameBlueprint(string input)
    {
        var blueprint = ParseBlueprint.Execute(input);
     
        var clean = CleanBlueprint.Execute(blueprint);
        
        Assert.Equal(JsonSerializer.Serialize(blueprint), JsonSerializer.Serialize(clean));
    }
}

