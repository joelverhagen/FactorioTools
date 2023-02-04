namespace Knapcode.FactorioTools.OilField.Algorithms;

internal class AStarResult
{
    public AStarResult(Location start, Location? reachedGoal, HashSet<Location> goals, Dictionary<Location, Location> cameFrom, Dictionary<Location, double> costSoFar)
    {
        Start = start;
        ReachedGoal = reachedGoal;
        Goals = goals;
        CameFrom = cameFrom;
        CostSoFar = costSoFar;
    }

    public Location Start { get; }
    public Location? ReachedGoal { get; }
    public HashSet<Location> Goals { get; }
    public Dictionary<Location, Location> CameFrom { get; }
    public Dictionary<Location, double> CostSoFar { get; }

    public List<Location> GetPath()
    {
        if (!ReachedGoal.HasValue)
        {
            throw new InvalidOperationException("No goal location was reached.");
        }

        var path = new List<Location>();
        var current = ReachedGoal.Value;
        while (true)
        {
            var next = CameFrom[current];
            path.Add(current);
            if (next == current)
            {
                break;
            }

            current = next;
        }

        return path;
    }
}