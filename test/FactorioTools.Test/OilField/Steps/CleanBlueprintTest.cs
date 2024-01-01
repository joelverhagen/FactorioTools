using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knapcode.FactorioTools.OilField.Steps;

public class CleanBlueprintTest : PlannerFacts
{
    [Theory]
    [MemberData(nameof(BigListIndexTestData))]
    public void BigListBlueprintsAreNormalized(int blueprintIndex)
    {
        // Arrange
        var expected = BigListBlueprintStrings[blueprintIndex];
        string actual = Clean(expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    private static string Clean(string expected)
    {
        var blueprint = ParseBlueprint.Execute(expected);

        // Act
        var clean = CleanBlueprint.Execute(blueprint);
        var actual = GridToBlueprintString.SerializeBlueprint(clean, addFbeOffset: false);
        return actual;
    }

    [Theory]
    [MemberData(nameof(SmallListIndexTestData))]
    public void SmallListBlueprintsAreNormalized(int blueprintIndex)
    {
        // Arrange
        var expected = SmallListBlueprintStrings[blueprintIndex];
        var blueprint = ParseBlueprint.Execute(expected);

        // Act
        var clean = CleanBlueprint.Execute(blueprint);
        var actual = GridToBlueprintString.SerializeBlueprint(clean, addFbeOffset: false);

        // Assert
        Assert.Equal(expected, actual);
    }
}

