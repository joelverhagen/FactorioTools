using System.Collections.Generic;
using System.Linq;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public class DijkstrasResult
{
    public DijkstrasResult(Dictionary<Location, ILocationSet> locationToPrevious, ILocationSet reachedGoals)
    {
        LocationToPrevious = locationToPrevious;
        ReachedGoals = reachedGoals;
    }

    public Dictionary<Location, ILocationSet> LocationToPrevious { get; }
    public ILocationSet ReachedGoals { get; }

    public List<List<Location>> GetStraightPaths(Location goal)
    {
        var paths = new List<List<Location>>();

        if (LocationToPrevious.TryGetValue(goal, out var previousLocations))
        {
            if (previousLocations.Count == 0)
            {
                // This is a special case when the goal matches the starting point.
                paths.Add(new List<Location> { goal });
            }

            foreach (var beforeGoal in previousLocations.EnumerateItems())
            {
                // This is the final direction used in the path. We'll start with preferring this direction as we reconstruct
                // the full path.
                var preferredDirection = GetDirection(beforeGoal, goal);

                // Track the number of times each direction was used so when we have to switch directions, we can prefer a
                // direction that's been used the most.
                var directionHits = new Dictionary<Direction, int>
                {
                    { Direction.Up, 0 },
                    { Direction.Right, 0 },
                    { Direction.Down, 0 },
                    { Direction.Left, 0 },
                };

                var current = goal;
                var path = new List<Location>();
                while (true)
                {
                    path.Add(current);

                    var allPrevious = LocationToPrevious[current];
                    if (allPrevious.Count == 0)
                    {
                        break;
                    }

                    var directionToPrevious = new Dictionary<Direction, Location>();
                    foreach (var candidate in allPrevious.EnumerateItems())
                    {
                        directionToPrevious[GetDirection(candidate, current)] = candidate;
                    }

                    if (directionToPrevious.TryGetValue(preferredDirection, out var previous))
                    {
                        current = previous;
                        directionHits[preferredDirection]++;
                    }
                    else
                    {
                        var nextBest = directionToPrevious.MaxBy(x => directionHits[x.Key]);
                        directionHits[nextBest.Key]++;
                        current = nextBest.Value;
                    }
                }

                paths.Add(path);
            }
        }

        return paths;
    }

    private static Direction GetDirection(Location from, Location to)
    {
        var deltaX = to.X - from.X;
        var deltaY = to.Y - from.Y;

        if (deltaX > 0)
        {
            return Direction.Right;
        }
        else if (deltaX < 0)
        {
            return Direction.Left;
        }
        else if (deltaY > 0)
        {
            return Direction.Down;
        }
        else
        {
            return Direction.Up;
        }
    }
}
