using System.Runtime.InteropServices;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

internal static class PruneSquares
{
    public static void Execute(Context context, HashSet<Location> pipes)
    {
        bool pruned;
        var allPruned = new HashSet<Location>();
        do
        {
            pruned = false;
            foreach (var goal in pipes)
            {
                if (PruneSquare(context.Grid, goal))
                {
                    allPruned.Add(goal);
                    pruned = true;
                }
            }
        }
        while (pruned);

        pipes.ExceptWith(allPruned);
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
