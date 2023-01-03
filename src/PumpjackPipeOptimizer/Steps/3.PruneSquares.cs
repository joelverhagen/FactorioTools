using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

internal static class PruneSquares
{
    public static void Execute(Context context, IEnumerable<Location> pipes)
    {
        bool pruned;
        do
        {
            pruned = false;
            foreach (var goal in pipes)
            {
                pruned |= PruneSquare(context.Grid, goal);
            }
        }
        while (pruned);
    }

    private static bool PruneSquare(SquareGrid grid, Location l)
    {
        var isSquare = grid.IsEntityType<Pipe>(l)
            && grid.IsEntityType<Pipe>(l.Translate((1, 0)))
            && grid.IsEntityType<Pipe>(l.Translate((0, 1)))
            && grid.IsEntityType<Pipe>(l.Translate((1, 1)));

        var wouldDisconnectTerminal = grid.IsEntityType<Terminal>(l)
            || grid.IsEntityType<Terminal>(l.Translate((-1, 0)))
            || grid.IsEntityType<Terminal>(l.Translate((0, -1)));

        if (isSquare && !wouldDisconnectTerminal)
        {
            grid.RemoveEntity(l);
            return true;
        }

        return false;
    }
}
