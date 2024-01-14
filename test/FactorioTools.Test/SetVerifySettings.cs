using System.Runtime.CompilerServices;
using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools;

public static class SetVerifySettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize();
        VerifierSettings.AutoVerify(includeBuildServer: false);
    }
}

public static class ExtensionMethods
{
    public static IEnumerable<GridEntity> GetEntities(this SquareGrid grid)
    {
        foreach (var location in grid.EntityLocations.EnumerateItems())
        {
            yield return grid[location]!;
        }
    }
}