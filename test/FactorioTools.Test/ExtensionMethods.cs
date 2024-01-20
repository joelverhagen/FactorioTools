using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools;

public static class ExtensionMethods
{
    public static ITableArray<T> ToTableArray<T>(this IEnumerable<T> source)
    {
        return source.ToList().ToTableArray();
    }

    public static IEnumerable<GridEntity> GetEntities(this SquareGrid grid)
    {
        foreach (var location in grid.EntityLocations.EnumerateItems())
        {
            yield return grid[location]!;
        }
    }
}