using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public static class Planner
{
    public static (Context Context, OilFieldPlanSummary Summary) Execute(OilFieldOptions options, BlueprintRoot inputBlueprint)
    {
        return Execute(options, inputBlueprint, addElectricPolesFirst: false);
    }

    private static (Context Context, OilFieldPlanSummary Summary) Execute(OilFieldOptions options, BlueprintRoot inputBlueprint, bool addElectricPolesFirst)
    {
        var context = InitializeContext.Execute(options, inputBlueprint);
        var initialPumpjackCount = context.CenterToTerminals.Count;

        if (context.CenterToTerminals.Count == 0)
        {
            throw new FactorioToolsException("The must be at least one pumpjack in the blueprint.", badInput: true);
        }

        HashSet<Location>? poles;
        if (addElectricPolesFirst)
        {
            poles = AddElectricPoles.Execute(context, avoidTerminals: true, allowRetries: false);
            if (poles is null)
            {
                throw new FactorioToolsException("No valid placement for the electric poles could be found, while adding electric poles first.");
            }
        }

        (var selectedPlans, var unusedPlans) = AddPipes.Execute(context, eliminateStrandedTerminals: addElectricPolesFirst);

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        if (!addElectricPolesFirst || context.Options.AddBeacons)
        {
            poles = AddElectricPoles.Execute(context, avoidTerminals: false, allowRetries: addElectricPolesFirst);
            if (poles is null)
            {
                if (addElectricPolesFirst)
                {
                    // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());
                    throw new FactorioToolsException("No valid placement for the electric poles could be found, after adding electric poles first.");
                }
                else
                {
                    return Execute(options, inputBlueprint, addElectricPolesFirst: true);
                }
            }
        }

        Validate.AllEntitiesHavePower(context);

        var finalPumpjackCount = context.CenterToTerminals.Count;

        var planSummary = new OilFieldPlanSummary(
            initialPumpjackCount - finalPumpjackCount,
            selectedPlans,
            unusedPlans);

        return (context, planSummary);
    }
}
