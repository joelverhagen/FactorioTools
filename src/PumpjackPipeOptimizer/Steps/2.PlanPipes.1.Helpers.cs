using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

internal static partial class PlanPipes
{
    private static bool AreLocationsCollinear(List<Location> locations)
    {
        double lastSlope = 0;
        for (var i = 0; i < locations.Count; i++)
        {
            if (i == locations.Count - 1)
            {
                return true;
            }

            var node = locations[i];
            var next = locations[i + 1];
            double dX = Math.Abs(node.X - next.X);
            double dY = Math.Abs(node.Y - next.Y);
            if (i == 0)
            {
                lastSlope = dY / dX;
            }
            else if (lastSlope != dY / dX)
            {
                break;
            }
        }

        return false;
    }

    private static int CountTurns(List<Location> path)
    {
        var previousDirection = -1;
        var turns = 0;
        for (var i = 1; i < path.Count; i++)
        {
            var currentDirection = path[i].X == path[i - 1].X ? 0 : 1;
            if (previousDirection != -1)
            {
                if (previousDirection != currentDirection)
                {
                    turns++;
                }
            }

            previousDirection = currentDirection;
        }

        return turns;
    }

    private static double GetEuclideanDistance(Location a, double bX, double bY)
    {
        return Math.Sqrt(Math.Pow(a.X - bX, 2) + Math.Pow(a.Y - bY, 2));
    }

    private static List<Location> MakeStraightLine(Location a, Location b)
    {
        if (a.X == b.X)
        {
            return Enumerable
                .Range(Math.Min(a.Y, b.Y), Math.Abs(a.Y - b.Y) + 1)
                .Select(y => new Location(a.X, y))
                .ToList();
        }

        if (a.Y == b.Y)
        {
            return Enumerable
                .Range(Math.Min(a.X, b.X), Math.Abs(a.X - b.X) + 1)
                .Select(x => new Location(x, a.Y))
                .ToList();
        }

        throw new ArgumentException("The two points must be one the same line either horizontally or vertically.");
    }

    private static void EliminateOtherTerminals(Context context, TerminalLocation selectedTerminal)
    {
        var terminalOptions = context.CenterToTerminals[selectedTerminal.Center];

        if (terminalOptions.Count == 1)
        {
            return;
        }

        terminalOptions.Clear();
        terminalOptions.Add(selectedTerminal);
    }
}
