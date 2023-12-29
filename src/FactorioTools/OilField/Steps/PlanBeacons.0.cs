﻿using System;
using System.Collections;
using System.Collections.Generic;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

public static partial class PlanBeacons
{
    public static List<BeaconSolution> Execute(Context context, LocationSet pipes)
    {
        foreach (var pipe in pipes)
        {
            context.Grid.AddEntity(pipe, new TemporaryEntity());
        }

        var solutions = new List<BeaconSolution>(context.Options.BeaconStrategies.Count);

        var completedStrategies = new BitArray((int)BeaconStrategy.Snug + 1); // max value
        foreach (var strategy in context.Options.BeaconStrategies)
        {
            if (completedStrategies[(int)strategy])
            {
                continue;
            }

            (var beacons, var effects) = strategy switch
            {
                BeaconStrategy.FbeOriginal => AddBeaconsFbe(context, strategy),
                BeaconStrategy.Fbe => AddBeaconsFbe(context, strategy),
                BeaconStrategy.Snug => AddBeaconsSnug(context),
                _ => throw new NotImplementedException(),
            };

            completedStrategies[(int)strategy] = true;

            solutions.Add(new BeaconSolution(strategy, beacons, effects));
        }

        foreach (var pipe in pipes)
        {
            context.Grid.RemoveEntity(pipe);
        }

        if (solutions.Count == 0)
        {
            throw new FactorioToolsException("At least one beacon strategy must be used.");
        }

        Validate.BeaconsDoNotOverlap(context, solutions);

        return solutions;
    }
}
