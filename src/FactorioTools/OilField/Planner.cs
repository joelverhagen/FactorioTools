﻿using System.Linq;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public static class Planner
{
#if USE_HASHSETS
    private static readonly LocationSet EmptyLocationSet = new();
#else
    private static readonly LocationSet EmptyLocationSet = new LocationSet(0, 0);
#endif

    public static (Context Context, OilFieldPlanSummary Summary) ExecuteSample()
    {
        var options = OilFieldOptions.ForMediumElectricPole;

        options.PipeStrategies = OilFieldOptions.AllPipeStrategies.ToList();
        options.BeaconStrategies = OilFieldOptions.AllBeaconStrategies.ToList();
        options.ValidateSolution = true;

        var inputBlueprint = new Blueprint
        {
            Entities = new[]
            {
                new Entity
                {
                    EntityNumber = 1,
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position
                    {
                        X = -3,
                        Y = -5,
                    },
                },
                new Entity
                {
                    EntityNumber = 2,
                    Direction = Direction.Down,
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position
                    {
                        X = 4,
                        Y = 5,
                    },
                },
                new Entity
                {
                    EntityNumber = 3,
                    Direction = Direction.Right,
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position
                    {
                        X = 12,
                        Y = -2,
                    },
                },
                new Entity
                {
                    EntityNumber = 4,
                    Direction = Direction.Down,
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position
                    {
                        X = -8,
                        Y = 7,
                    },
                },
            },
            Icons = new[]
            {
                new Icon
                {
                    Index = 1,
                    Signal = new SignalID
                    {
                        Name = EntityNames.Vanilla.Pumpjack,
                        Type = SignalTypes.Vanilla.Item,
                    }
                }
            },
            Item = ItemNames.Vanilla.Blueprint,
            Version = 0,
        };

        return Execute(options, inputBlueprint);
    }

    public static (Context Context, OilFieldPlanSummary Summary) Execute(OilFieldOptions options, Blueprint inputBlueprint)
    {
        return Execute(options, inputBlueprint, electricPolesAvoid: EmptyLocationSet, EletricPolesMode.AddLast);
    }

    private static (Context Context, OilFieldPlanSummary Summary) Execute(
        OilFieldOptions options,
        Blueprint blueprint,
        LocationSet electricPolesAvoid,
        EletricPolesMode electricPolesMode)
    {
        var context = InitializeContext.Execute(options, blueprint);
        var initialPumpjackCount = context.CenterToTerminals.Count;
        var addElectricPolesFirst = electricPolesMode != EletricPolesMode.AddLast;

        if (context.CenterToTerminals.Count == 0)
        {
            throw new FactorioToolsException("The must be at least one pumpjack in the blueprint.", badInput: true);
        }

        LocationSet? poles;
        if (addElectricPolesFirst)
        {
            if (electricPolesMode == EletricPolesMode.AddFirstAndAvoidAllTerminals)
            {
                electricPolesAvoid = context
                    .CenterToTerminals
                    .Values
                    .SelectMany(t => t)
                    .Select(t => t.Terminal)
                    .ToSet(context);
            }

            poles = AddElectricPoles.Execute(context, electricPolesAvoid, allowRetries: false);

            if (poles is null)
            {
                if (electricPolesMode == EletricPolesMode.AddFirstAndAvoidAllTerminals)
                {
                    throw new FactorioToolsException(
                        "No valid placement for the electric poles could be found, while adding electric poles first. " +
                        "Try removing some pumpjacks or using a different electric pole.",
                        badInput: true);
                }

                return Execute(options, blueprint, EmptyLocationSet, EletricPolesMode.AddFirstAndAvoidAllTerminals);
            }
            else
            {
                // remove terminals overlapping with the added poles
                foreach (var terminals in context.CenterToTerminals.Values)
                {
                    for (int i = 0; i < terminals.Count; i++)
                    {
                        var terminal = terminals[i];
                        if (context.Grid.IsEmpty(terminal.Terminal))
                        {
                            continue;
                        }

                        terminals.RemoveAt(i);
                        context.LocationToTerminals[terminal.Terminal].Remove(terminal);
                        i--;
                    }
                }
            }
        }

        (var selectedPlans, var alternatePlans, var unusedPlans) = AddPipes.Execute(context, eliminateStrandedTerminals: addElectricPolesFirst);

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        if (!addElectricPolesFirst || context.Options.AddBeacons)
        {
            poles = AddElectricPoles.Execute(context, avoid: EmptyLocationSet, allowRetries: addElectricPolesFirst);
            if (poles is null)
            {
                if (addElectricPolesFirst)
                {
                    // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());
                    throw new FactorioToolsException(
                        "No valid placement for the electric poles could be found, after adding electric poles first. " +
                        "Try removing some pumpjacks or using a different electric pole.",
                        badInput: true);
                }
                else
                {
                    electricPolesAvoid = context.CenterToTerminals.SelectMany(t => t.Value.Select(l => l.Terminal)).ToSet(context);
                    return Execute(options, blueprint, electricPolesAvoid, EletricPolesMode.AddFirstAndAvoidSpecificTerminals);
                }
            }
        }

        Validate.AllEntitiesHavePower(context);

        var missingPumpjacks = initialPumpjackCount - context.CenterToTerminals.Count;
        if (missingPumpjacks > 0)
        {
            throw new FactorioToolsException("The initial number of pumpjacks does not match the final pumpjack count.");
        }

        var rotatedPumpjacks = 0;
        foreach ((var location, var originalDirection) in context.CenterToOriginalDirection)
        {
            var finalDirection = context.CenterToTerminals[location].Single().Direction;
            if (originalDirection != finalDirection)
            {
                rotatedPumpjacks++;
            }
        }

        var planSummary = new OilFieldPlanSummary(
            missingPumpjacks,
            rotatedPumpjacks,
            selectedPlans,
            alternatePlans,
            unusedPlans);

        return (context, planSummary);
    }

    private enum EletricPolesMode
    {
        AddLast,
        AddFirstAndAvoidSpecificTerminals,
        AddFirstAndAvoidAllTerminals,
    }
}
