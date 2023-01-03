using PumpjackPipeOptimizer.Algorithms;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

internal static class AddElectricPoles
{
    public static void Execute(Context context)
    {
        // Convert the grid to an electric grid, which has a different neighbor discovery algorithm.
        context.Grid = new ElectricGrid(context.Grid, context.Options.ElectricPoleWireReach);

        var electricPoles = AddElectricPolesAroundPumpjacks(context);

        ConnectElectricPoles(context.Grid, electricPoles, context.Options.ElectricPoleWireReach);
    }

    private static Dictionary<Location, ElectricPole> AddElectricPolesAroundPumpjacks(Context context)
    {
        var pumpjackArea = (X: 3, Y: 3);
        var offsetX = pumpjackArea.X / 2 + context.Options.ElectricPoleSupplyWidth / 2;
        var offsetY = pumpjackArea.Y / 2 + context.Options.ElectricPoleSupplyHeight / 2;

        // First, find the spots for powerpoles that cover the most pumpjacks.
        var candidateToCovered = new Dictionary<Location, HashSet<Location>>();

        // Generate electric pole locations
        foreach (var center in context.Centers)
        {
            for (var x = center.X - offsetX; x <= center.X + offsetX; x++)
            {
                for (var y = center.Y - offsetY; y <= center.Y + offsetY; y++)
                {
                    var candidate = new Location(x, y);

                    var fits = true;
                    for (var w = 0; w < context.Options.ElectricPoleWidth && fits; w++)
                    {
                        for (var h = 0; h < context.Options.ElectricPoleHeight && fits; h++)
                        {
                            fits = context.Grid.IsEmpty(candidate.Translate((w, h))) && context.Grid.IsInBounds(candidate.Translate((w, h)));
                        }
                    }

                    if (!fits)
                    {
                        continue;
                    }

                    if (!candidateToCovered.TryGetValue(candidate, out var covered))
                    {
                        candidateToCovered.Add(candidate, new HashSet<Location> { center });
                    }
                    else
                    {
                        covered.Add(center);
                    }
                }
            }
        }

        var candidateToPumpjackDistance = candidateToCovered.ToDictionary(
            x => x.Key,
            x => x.Value.Sum(y => y.GetEuclideanDistance(x.Key)));
        var candidateToMiddleDistance = candidateToCovered.ToDictionary(
            x => x.Key,
            x => x.Key.GetEuclideanDistance(context.Grid.Middle));

        // Prefer spots that cover the most pumpjacks. Break ties with spots closer to their pumpjack centers.
        // Break further ties with spots closer to the center of the grid.
        var sortedCandidates = candidateToCovered
            .Keys
            .OrderByDescending(x => candidateToCovered[x].Count)
            .ThenBy(x => candidateToPumpjackDistance[x])
            .ThenBy(x => candidateToMiddleDistance[x]);
        var coveredPumpjacks = new HashSet<Location>();
        var electricPoles = new Dictionary<Location, ElectricPole>();
        foreach (var candidate in sortedCandidates)
        {
            var covered = candidateToCovered[candidate];
            if (covered.IsSubsetOf(coveredPumpjacks))
            {
                continue;
            }

            electricPoles.Add(candidate, new ElectricPole());
            coveredPumpjacks.UnionWith(candidateToCovered[candidate]);
        }

        foreach ((var location, var entity) in electricPoles)
        {
            context.Grid.AddEntity(location, entity);
        }

        return electricPoles;
    }

    public static bool AreElectricPolesConnected(Location a, Location b, double wireReach)
    {
        return a.GetEuclideanDistance(b) <= wireReach;
    }

    private static void ConnectElectricPoles(SquareGrid grid, Dictionary<Location, ElectricPole> electricPoles, double wireReach)
    {
        // Find the two electric poles closest to the middle of the grid and connect them.
        var polesByMiddleDistance = electricPoles
            .OrderBy(x => x.Key.GetEuclideanDistance(grid.Middle))
            .Select(x => x.Key)
            .ToList();

        var connectedPoles = new HashSet<Location> { polesByMiddleDistance[0] };

        for (var i = 1; i < polesByMiddleDistance.Count; i++)
        {
            AddToGrid(grid, electricPoles, connectedPoles, polesByMiddleDistance[i]);
        }
    }

    private static void AddToGrid(SquareGrid grid, Dictionary<Location, ElectricPole> electricPoles, HashSet<Location> connectedPoles, Location start)
    {
        var result = Dijkstras.GetShortestPaths(grid, start, connectedPoles, stopOnFirstGoal: true);

        var goal = result.ReachedGoals.Single();
        var path = result.GetStraightPaths(goal).First();

        foreach (var pole in path)
        {
            if (connectedPoles.Add(pole) && grid.IsEmpty(pole))
            {
                var entity = new ElectricPole();
                electricPoles.Add(pole, entity);
                grid.AddEntity(pole, entity);
            }
        }

        for (var i = 1; i < path.Count; i++)
        {
            electricPoles[path[i - 1]].AddNeighbor(electricPoles[path[i]]);
        }
    }
}
