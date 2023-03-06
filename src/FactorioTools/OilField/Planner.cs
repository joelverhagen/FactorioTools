using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public static class Planner
{
    public static (Context Context, PlanSummary Summary) Execute(OilFieldOptions options, BlueprintRoot inputBlueprint)
    {
        return Execute(options, inputBlueprint, addElectricPolesFirst: false);
    }

    private static (Context Context, PlanSummary Summary) Execute(OilFieldOptions options, BlueprintRoot inputBlueprint, bool addElectricPolesFirst)
    {
        var context = InitializeContext.Execute(options, inputBlueprint);
        var initialPumpjackCount = context.CenterToTerminals.Count;

        if (context.CenterToTerminals.Count == 0)
        {
            throw new InvalidOperationException("The must be at least one pumpjack in the blueprint.");
        }

        HashSet<Location>? poles;
        if (addElectricPolesFirst)
        {
            poles = AddElectricPoles.Execute(context, avoidTerminals: true, allowRetries: false);
            if (poles is null)
            {
                throw new InvalidOperationException("No valid placement for the electric poles could be found.");
            }
        }

        (var selectedPlans, var allPlans) = AddPipes.Execute(context, eliminateStrandedTerminals: addElectricPolesFirst);

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        if (!addElectricPolesFirst || context.Options.AddBeacons)
        {
            poles = AddElectricPoles.Execute(context, avoidTerminals: false, allowRetries: addElectricPolesFirst);
            if (poles is null)
            {
                if (addElectricPolesFirst)
                {
                    // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());
                    throw new InvalidOperationException("No valid placement for the electric poles could be found.");
                }
                else
                {
                    return Execute(options, inputBlueprint, addElectricPolesFirst: true);
                }
            }
        }

        Validate.AllEntitiesHavePower(context);

        var finalPumpjackCount = context.CenterToTerminals.Count;

        var planSummary = new PlanSummary(
            initialPumpjackCount - finalPumpjackCount,
            selectedPlans,
            allPlans);

        return (context, planSummary);
    }
}
