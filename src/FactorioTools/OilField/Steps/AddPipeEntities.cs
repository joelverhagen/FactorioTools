using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

public static class AddPipeEntities
{
    public static void Execute(Context context, HashSet<Location> pipes, Dictionary<Location, Direction>? undergroundPipes)
    {
        Execute(
            context.Grid,
            context.SharedInstances,
            context.CenterToTerminals,
            pipes,
            undergroundPipes);
    }

    public static void Execute(
        SquareGrid grid,
        SharedInstances sharedInstances,
        Dictionary<Location, List<TerminalLocation>> centerToTerminals,
        HashSet<Location> pipes,
        Dictionary<Location, Direction>? undergroundPipes)
    {
#if USE_SHARED_INSTANCES
        var addedPipes = sharedInstances.LocationSetA;
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
                    grid.AddEntity(location, new UndergroundPipe(direction));
                }
            }

            foreach (var terminals in centerToTerminals.Values)
            {
                if (terminals.Count != 1)
                {
                    throw new FactorioToolsException("Every pumpjack should have a single terminal selected.");
                }

                for (int i = 0; i < terminals.Count; i++)
                {
                    if (addedPipes.Add(terminals[i].Terminal))
                    {
                        grid.AddEntity(terminals[i].Terminal, new Terminal());
                    }
                }
            }

            foreach (var pipe in pipes)
            {
                if (addedPipes.Add(pipe))
                {
                    grid.AddEntity(pipe, new Pipe());
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
