﻿using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class AStarResult
{
    private readonly List<Location>? _path;

    public AStarResult(bool success, Location reachedGoal, List<Location>? path)
    {
        Success = success;
        ReachedGoal = reachedGoal;
        _path = path;
    }

    public bool Success { get; }
    public Location ReachedGoal { get; }

    public List<Location> Path
    {
        get
        {
            if (_path is null)
            {
                throw new FactorioToolsException("No goal location was reached.");
            }

            return _path;
        }
    }
}