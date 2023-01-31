using PumpjackPipeOptimizer.Algorithms;
using PumpjackPipeOptimizer.Grid;

namespace PumpjackPipeOptimizer.Steps;

internal static class AddElectricPoles
{
    public static void Execute(Context context)
    {
        // Convert the grid to an electric grid, which has a different neighbor discovery algorithm.
        context.Grid = new ElectricGrid(context.Grid, context.Options);

        var electricPoles = AddElectricPolesAroundPumpjacks(context);

        ConnectElectricPoles(context, electricPoles);
    }

    private static Dictionary<Location, ElectricPoleCenter> AddElectricPolesAroundPumpjacks(Context context)
    {
        var pumpjackArea = (X: 3, Y: 3);
        var offsetX = pumpjackArea.X / 2 + context.Options.ElectricPoleSupplyWidth / 2;
        var offsetY = pumpjackArea.Y / 2 + context.Options.ElectricPoleSupplyHeight / 2;

        // First, find the spots for powerpoles that cover the most pumpjacks.
        var candidateToCovered = new Dictionary<Location, HashSet<Location>>();

        // Generate electric pole locations
        foreach (var center in context.CenterToTerminals.Keys)
        {
            for (var x = center.X - offsetX; x <= center.X + offsetX; x++)
            {
                for (var y = center.Y - offsetY; y <= center.Y + offsetY; y++)
                {
                    var candidate = new Location(x, y);
                    (var fits, _) = GetElectricPoleLocations(context.Grid, context.Options, candidate, populateSides: false);

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

        var candidateToMiddleDistance = candidateToCovered.ToDictionary(
            x => x.Key,
            x => x.Key.GetEuclideanDistance(context.Grid.Middle));

        var coveredPumpjacks = new HashSet<Location>();
        var electricPoles = new Dictionary<Location, ElectricPoleCenter>();

        while (coveredPumpjacks.Count < context.CenterToTerminals.Count)
        {
            var candidateToPumpjackDistance = candidateToCovered.ToDictionary(
                x => x.Key,
                x => x.Value.Sum(y => y.GetEuclideanDistance(x.Key)));

            // Prefer spots that cover the most pumpjacks. Break ties with spots closer to their pumpjack centers.
            // Break further ties with spots closer to the center of the grid.
            var sortedCandidates = candidateToCovered
                .Keys
                .OrderByDescending(x => candidateToCovered[x].Count)
                .ThenBy(x => candidateToPumpjackDistance[x])
                .ThenBy(x => candidateToMiddleDistance[x]);

            foreach (var candidate in sortedCandidates)
            {
                var covered = candidateToCovered[candidate];
                if (covered.IsSubsetOf(coveredPumpjacks))
                {
                    throw new InvalidOperationException($"Candidate {candidate} should have been eliminated.");
                }

                (var fits, var sides) = GetElectricPoleLocations(context.Grid, context.Options, candidate, populateSides: true);

                if (fits)
                {
                    AddElectricPole(context, electricPoles, candidate, sides!);

                    coveredPumpjacks.UnionWith(candidateToCovered[candidate]);

                    // Remove the covered pumpjacks from the canidate data, so that the next candidates are discounted
                    // by the pumpjacks that no longer need power.
                    foreach ((var otherCandidate, var otherCovered) in candidateToCovered.ToList())
                    {
                        otherCovered.ExceptWith(coveredPumpjacks);
                        if (otherCovered.Count == 0)
                        {
                            candidateToCovered.Remove(otherCandidate);
                        }
                    }

                    break;
                }
            }
        }

        return electricPoles;
    }

    private static void AddElectricPole(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles, Location candidate, List<Location> sides)
    {
        var center = new ElectricPoleCenter();
        context.Grid.AddEntity(candidate, center);
        electricPoles.Add(candidate, center);

        foreach (var location in sides)
        {
            context.Grid.AddEntity(location, new ElectricPoleSide(center));
        }
    }

    public static (bool Fits, List<Location>? Sides) GetElectricPoleLocations(SquareGrid grid, Options options, Location center, bool populateSides)
    {

        var fits = true;
        var sides = populateSides ? new List<Location>() : null;

        var offsetX = ((options.ElectricPoleWidth - 1) / 2) * -1;
        var offsetY = ((options.ElectricPoleHeight - 1) / 2) * -1;

        for (var w = 0; w < options.ElectricPoleWidth && fits; w++)
        {
            for (var h = 0; h < options.ElectricPoleHeight && fits; h++)
            {
                var location = center.Translate((offsetX + w, offsetY + h));
                fits = grid.IsEmpty(location) && grid.IsInBounds(location);

                if (fits && location != center && populateSides)
                {
                    sides!.Add(location);
                }
            }
        }

        return (fits, sides);
    }

    public static bool AreElectricPolesConnected(Location a, Location b, Options options)
    {
        return a.GetEuclideanDistance(b) <= options.ElectricPoleWireReach;
    }

    private static void ConnectElectricPoles(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles)
    {
        // Find the two electric poles closest to the middle of the grid and connect them.
        var polesByMiddleDistance = electricPoles
            .OrderBy(x => x.Key.GetEuclideanDistance(context.Grid.Middle))
            .Select(x => x.Key)
            .ToList();

        var connectedPoles = new HashSet<Location> { polesByMiddleDistance[0] };

        for (var i = 1; i < polesByMiddleDistance.Count; i++)
        {
            AddToGrid(context, electricPoles, connectedPoles, polesByMiddleDistance[i]);
        }
    }

    private static void AddToGrid(Context context, Dictionary<Location, ElectricPoleCenter> electricPoles, HashSet<Location> connectedPoles, Location start)
    {
        var result = Dijkstras.GetShortestPaths(context.Grid, start, connectedPoles, stopOnFirstGoal: true);

        var goal = result.ReachedGoals.Single();
        var path = result.GetStraightPaths(goal).First();

        foreach (var pole in path)
        {
            if (connectedPoles.Add(pole) && context.Grid.IsEmpty(pole))
            {
                (var fits, var sides) = GetElectricPoleLocations(context.Grid, context.Options, pole, populateSides: true);
                if (!fits)
                {
                    throw new InvalidOperationException($"Could not add electric pole to the grid at {pole}. It does not fit.");
                }

                AddElectricPole(context, electricPoles, pole, sides!);
            }
        }

        for (var i = 1; i < path.Count; i++)
        {
            electricPoles[path[i - 1]].AddNeighbor(electricPoles[path[i]]);
        }
    }
}
