using System.Collections.Generic;
using Knapcode.FactorioTools.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

public static class AddPipeEntities
{
    public static void Execute(Context context, LocationSet pipes, Dictionary<Location, Direction>? undergroundPipes)
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
        IReadOnlyDictionary<Location, List<TerminalLocation>> centerToTerminals,
        LocationSet pipes,
        Dictionary<Location, Direction>? undergroundPipes = null,
        bool allowMultipleTerminals = false)
    {
#if NO_SHARED_INSTANCES
        var addedPipes = new LocationSet();
#else
        var addedPipes = sharedInstances.LocationSetA;
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
                if (terminals.Count != 1 && !allowMultipleTerminals)
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

            foreach (var pipe in pipes.EnumerateItems())
            {
                if (addedPipes.Add(pipe))
                {
                    grid.AddEntity(pipe, new Pipe());
                }
            }

        }
        finally
        {
#if !NO_SHARED_INSTANCES
            addedPipes.Clear();
#endif
        }
    }
}
