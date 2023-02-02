namespace PumpjackPipeOptimizer.Steps;

internal static partial class PlanPipes
{
    public static HashSet<Location> Execute(Context context)
    {
        HashSet<Location>? bestPipes = null;

        foreach (var strategy in Enum.GetValues<PlanPipesStrategy>())
        {
            var pipes = strategy switch
            {
                PlanPipesStrategy.FBE => ExecuteWithFBE(context),
                PlanPipesStrategy.ConnectedCenters => ExecuteWithConnectedCenters(context),
                _ => throw new NotImplementedException(),
            };

            if (bestPipes is null || pipes.Count < bestPipes.Count)
            {
                bestPipes = pipes;
            }
        }

        return bestPipes!;
    }

    private enum PlanPipesStrategy
    {
        FBE,
        ConnectedCenters,
    }
}
