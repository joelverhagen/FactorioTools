﻿using System;
using System.Collections.Generic;
using System.Linq;
using Knapcode.FactorioTools.Data;
using Knapcode.FactorioTools.OilField.Grid;
using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public static class Planner
{
    public static (Context Context, OilFieldPlanSummary Summary) Execute(OilFieldOptions options, Blueprint inputBlueprint)
    {
        return Execute(options, inputBlueprint, electricPolesAvoid: Array.Empty<Location>(), EletricPolesMode.AddLast);
    }

    private static (Context Context, OilFieldPlanSummary Summary) Execute(
        OilFieldOptions options,
        Blueprint blueprint,
        IReadOnlyCollection<Location> electricPolesAvoid,
        EletricPolesMode eletricPolesMode)
    {
        var context = InitializeContext.Execute(options, blueprint);
        var initialPumpjackCount = context.CenterToTerminals.Count;
        var addElectricPolesFirst = eletricPolesMode != EletricPolesMode.AddLast;

        if (context.CenterToTerminals.Count == 0)
        {
            throw new FactorioToolsException("The must be at least one pumpjack in the blueprint.", badInput: true);
        }

        LocationSet? poles;
        if (addElectricPolesFirst)
        {
            if (eletricPolesMode == EletricPolesMode.AddFirstAndAvoidAllTerminals)
            {
                electricPolesAvoid = context
                    .CenterToTerminals
                    .Values
                    .SelectMany(t => t)
                    .Select(t => t.Terminal)
                    .ToLocationSet();
            }

            poles = AddElectricPoles.Execute(context, electricPolesAvoid, allowRetries: false);

            if (poles is null)
            {
                if (eletricPolesMode == EletricPolesMode.AddFirstAndAvoidAllTerminals)
                {
                    throw new FactorioToolsException(
                        "No valid placement for the electric poles could be found, while adding electric poles first. " +
                        "Try removing some pumpjacks or using a different electric pole.",
                        badInput: true);
                }

                return Execute(options, blueprint, Array.Empty<Location>(), EletricPolesMode.AddFirstAndAvoidAllTerminals);
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
            poles = AddElectricPoles.Execute(context, avoid: Array.Empty<Location>(), allowRetries: addElectricPolesFirst);
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
                    electricPolesAvoid = context.CenterToTerminals.SelectMany(t => t.Value.Select(l => l.Terminal)).ToLocationSet();
                    return Execute(options, blueprint, electricPolesAvoid, EletricPolesMode.AddFirstAndAvoidSpecificTerminals);
                }
            }
        }

        Validate.AllEntitiesHavePower(context);

        var finalPumpjackCount = context.CenterToTerminals.Count;

        if (finalPumpjackCount != initialPumpjackCount)
        {
            throw new FactorioToolsException("The initial number of pumpjacks does not match the final pumpjack count.");
        }

        var planSummary = new OilFieldPlanSummary(
            initialPumpjackCount - finalPumpjackCount,
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
