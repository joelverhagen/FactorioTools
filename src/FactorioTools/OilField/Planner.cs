using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

internal static class Planner
{
    public static Context Execute(Options options, BlueprintRoot inputBlueprint)
    {
        return Execute(options, inputBlueprint, addElectricPolesFirst: false);
    }

    private static Context Execute(Options options, BlueprintRoot inputBlueprint, bool addElectricPolesFirst)
    {
        var context = InitializeContext.Execute(options, inputBlueprint);

        if (context.CenterToTerminals.Count < 2)
        {
            throw new InvalidOperationException("The must be at least two pumpjacks in the blueprint.");
        }

        HashSet<Location>? poles;
        if (addElectricPolesFirst)
        {
            poles = AddElectricPoles.Execute(context, avoidTerminals: true, retryWithUncovered: false);
            if (poles is null)
            {
                throw new InvalidOperationException("No valid placement for the electric poles could be found.");
            }
        }

        AddPipes.Execute(context, eliminateStrandedTerminals: addElectricPolesFirst);

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        if (!addElectricPolesFirst || context.Options.AddBeacons)
        {
            poles = AddElectricPoles.Execute(context, avoidTerminals: false, retryWithUncovered: addElectricPolesFirst);
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

        if (context.Options.ValidateSolution)
        {
            AddElectricPoles.VerifyAllEntitiesHasPower(context);
        }

        return context;
    }
}
