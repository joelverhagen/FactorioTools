﻿using Knapcode.FactorioTools.Data;
using static Knapcode.FactorioTools.OilField.Helpers;

namespace Knapcode.FactorioTools.OilField;

public abstract class BaseTest
{
    public static ElectricPoleCenter AddElectricPole(Context context, Location center)
    {
        var entity = new ElectricPoleCenter(context.Grid.GetId());

        AddProviderToGrid(
            context.Grid,
            center,
            entity,
            c => new ElectricPoleSide(context.Grid.GetId(), c),
            providerWidth: context.Options.ElectricPoleWidth,
            providerHeight: context.Options.ElectricPoleHeight);

        return entity;
    }

    public static BeaconCenter AddBeacon(Context context, Location center)
    {
        var entity = new BeaconCenter(context.Grid.GetId());

        AddProviderToGrid(
            context.Grid,
            center,
            entity,
            c => new BeaconSide(context.Grid.GetId(),c),
            providerWidth: context.Options.BeaconWidth,
            providerHeight: context.Options.BeaconHeight);

        return entity;
    }

    public static PumpjackCenter AddPumpjack(Context context, Location center, Direction? direction = null)
    {
        var entity = Helpers.AddPumpjack(context.Grid, center);

        var previousCenterToTerminals = context.CenterToTerminals;

        context.CenterToTerminals = GetCenterToTerminals(context, context.Grid, context.CenterToTerminals.Keys.Concat(new[] { center }.Distinct(context)).ToList());
        context.LocationToTerminals = GetLocationToTerminals(context, context.CenterToTerminals);

        foreach ((var otherCenter, var terminals) in context.CenterToTerminals.EnumeratePairs().ToList())
        {
            var selectedDirection = center == otherCenter ? direction.GetValueOrDefault() : previousCenterToTerminals[otherCenter].First().Direction;
            var selectedTerminal = terminals.OrderByDescending(t => t.Direction == selectedDirection).First();
            EliminateOtherTerminals(context, selectedTerminal);

            if (context.Grid[selectedTerminal.Terminal] is Pipe)
            {
                context.Grid.RemoveEntity(selectedTerminal.Terminal);
                context.Grid.AddEntity(selectedTerminal.Terminal, new Terminal(context.Grid.GetId()));
            }
        }

        return entity;
    }

    public static string GetRepositoryRoot()
    {
        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
            if (Directory.EnumerateFiles(current, "FactorioTools.sln").Any())
            {
                return current;
            }

            current = Path.GetDirectoryName(current);
        }

        throw new InvalidOperationException($"Could not find the repository root when starting at {Directory.GetCurrentDirectory()}.");
    }
}