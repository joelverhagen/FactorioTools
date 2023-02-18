using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;
using Knapcode.FactorioTools.OilField.Steps;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField;

public class Facts
{
    internal static ElectricPoleCenter AddElectricPole(Context context, Location center)
    {
        var entity = new ElectricPoleCenter();

        AddProvider(
            context.Grid,
            center,
            entity,
            c => new ElectricPoleSide(c),
            providerWidth: context.Options.ElectricPoleWidth,
            providerHeight: context.Options.ElectricPoleHeight);

        return entity;
    }

    internal static BeaconCenter AddBeacon(Context context, Location center)
    {
        var entity = new BeaconCenter();

        AddProvider(
            context.Grid,
            center,
            entity,
            c => new BeaconSide(c),
            providerWidth: context.Options.BeaconWidth,
            providerHeight: context.Options.BeaconHeight);

        return entity;
    }

    internal static PumpjackCenter AddPumpjack(Context context, Location center, Direction? direction = null)
    {
        var entity = Helpers.AddPumpjack(context.Grid, center);

        var previousCenterToTerminals = context.CenterToTerminals;

        context.CenterToTerminals = GetCenterToTerminals(context.Grid, context.CenterToTerminals.Keys.Concat(new[] { center }.Distinct()));
        context.LocationToTerminals = GetLocationToTerminals(context.CenterToTerminals);

        foreach ((var otherCenter, var terminals) in context.CenterToTerminals.ToList())
        {
            var selectedDirection = center == otherCenter ? direction.GetValueOrDefault() : previousCenterToTerminals[otherCenter].First().Direction;
            var selectedTerminal = terminals.OrderByDescending(t => t.Direction == selectedDirection).First();
            EliminateOtherTerminals(context, selectedTerminal);

            if (context.Grid[selectedTerminal.Terminal] is Pipe)
            {
                context.Grid.RemoveEntity(selectedTerminal.Terminal);
                context.Grid.AddEntity(selectedTerminal.Terminal, new Terminal());
            }
        }

        return entity;
    }

    internal static string GetRepositoryRoot()
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