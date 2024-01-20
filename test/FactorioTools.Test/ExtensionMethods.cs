using Knapcode.FactorioTools.OilField;

namespace Knapcode.FactorioTools;

public static class ExtensionMethods
{
    public static ITableList<T> ToTableArray<T>(this IEnumerable<T> source)
    {
        return source.ToList().ToTableList();
    }

    public static IEnumerable<GridEntity> GetEntities(this SquareGrid grid)
    {
        foreach (var location in grid.EntityLocations.EnumerateItems())
        {
            yield return grid[location]!;
        }
    }
}