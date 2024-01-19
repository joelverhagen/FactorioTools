﻿using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public record BeaconPlannerResult(ITableList<Location> Beacons, int Effects);

public static class PlanBeacons
{
    public static ITableList<BeaconSolution> Execute(Context context, ILocationSet pipes)
    {
        foreach (var pipe in pipes.EnumerateItems())
        {
            context.Grid.AddEntity(pipe, new TemporaryEntity(context.Grid.GetId()));
        }

        var solutions = TableList.New<BeaconSolution>(context.Options.BeaconStrategies.Count);

        var completedStrategies = new CountedBitArray((int)BeaconStrategy.Snug + 1); // max value
        for (var i = 0; i < context.Options.BeaconStrategies.Count; i++)
        {
            var strategy = context.Options.BeaconStrategies[i];

            if (completedStrategies[(int)strategy])
            {
                continue;
            }

            (var beacons, var effects) = strategy switch
            {
                BeaconStrategy.FbeOriginal => PlanBeaconsFbe.Execute(context, strategy),
                BeaconStrategy.Fbe => PlanBeaconsFbe.Execute(context, strategy),
                BeaconStrategy.Snug => PlanBeaconsSnug.Execute(context),
                _ => throw new NotImplementedException(),
            };

            completedStrategies[(int)strategy] = true;

            solutions.Add(new BeaconSolution(strategy, beacons, effects));
        }

        foreach (var pipe in pipes.EnumerateItems())
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
