using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

internal static class Planner
{
    public static Context Execute(Options options, BlueprintRoot inputBlueprint)
    {
        var addElectricPolesFirst = false;

        while (true)
        {
            var context = InitializeContext.Execute(options, inputBlueprint);

            if (context.CenterToTerminals.Count < 2)
            {
                throw new InvalidOperationException("The must be at least two pumpjacks in the blueprint.");
            }

            HashSet<Location>? poles = null;
            if (addElectricPolesFirst)
            {
                poles = AddElectricPoles.Execute(context, avoidTerminals: true);
                if (poles is null)
                {
                    throw new InvalidOperationException("No valid placement for the electric poles could be found.");
                }
            }

            var pipes = AddPipes.Execute(context, eliminateStrandedTerminals: addElectricPolesFirst);

            // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

            if (context.Options.UseUndergroundPipes)
            {
                // Substitute long stretches of pipes for underground pipes
                UseUndergroundPipes.Execute(context, pipes);
            }

            if (context.Options.AddBeacons)
            {
                AddBeacons.Execute(context);
            }

            if (poles is null)
            {
                poles = AddElectricPoles.Execute(context, avoidTerminals: false);
                if (poles is null)
                {
                    addElectricPolesFirst = true;
                    continue;
                }
            }

            return context;
        }
    }
}
