﻿namespace Knapcode.FactorioTools.OilField.Algorithms;

internal class AStarResult
{
    private readonly List<Location>? _path;

    public AStarResult(Location? reachedGoal, List<Location>? path)
    {
        ReachedGoal = reachedGoal;
        _path = path;
    }

    public Location? ReachedGoal { get; }

    public List<Location> Path
    {
        get
        {
            if (_path is null)
            {
                throw new InvalidOperationException("No goal location was reached.");
            }

            return _path;
        }
    }
}