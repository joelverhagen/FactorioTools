using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class AddPipeEntities
{
    public static void Execute(Context context, HashSet<Location> pipes, Dictionary<Location, Direction>? undergroundPipes)
    {
#if USE_SHARED_INSTANCES
        var addedPipes = context.SharedInstances.LocationSetA;
#else
        var addedPipes = new HashSet<Location>();
#endif

        try
        {

            if (undergroundPipes is not null)
            {
                foreach ((var location, var direction) in undergroundPipes)
                {
                    addedPipes.Add(location);
                    context.Grid.AddEntity(location, new UndergroundPipe(direction));
                }
            }

            foreach (var terminals in context.CenterToTerminals.Values)
            {
                if (terminals.Count != 1)
                {
                    throw new InvalidOperationException("Every pumpjack should have a single terminal selected.");
                }

                for (int i = 0; i < terminals.Count; i++)
                {
                    if (addedPipes.Add(terminals[i].Terminal))
                    {
                        context.Grid.AddEntity(terminals[i].Terminal, new Terminal());
                    }
                }
            }

            foreach (var pipe in pipes)
            {
                if (addedPipes.Add(pipe))
                {
                    context.Grid.AddEntity(pipe, new Pipe());
                }
            }

        }
        finally
        {
#if USE_SHARED_INSTANCES
            addedPipes.Clear();
#endif
        }
    }
}
