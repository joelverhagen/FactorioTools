using System.Collections.Generic;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public static class AddPipeEntities
{
    public static void Execute(Context context, ILocationSet pipes, ILocationDictionary<Direction>? undergroundPipes)
    {
        Execute(context, context.Grid, pipes, undergroundPipes);
    }

    public static void Execute(
        Context context,
        SquareGrid grid,
        ILocationSet pipes,
        ILocationDictionary<Direction>? undergroundPipes = null,
        bool allowMultipleTerminals = false)
    {
#if USE_SHARED_INSTANCES
        var addedPipes = context.SharedInstances.LocationSetA;
#else
        var addedPipes = context.GetLocationSet();
#endif

        try
        {

            if (undergroundPipes is not null)
            {
                foreach ((var location, var direction) in undergroundPipes.EnumeratePairs())
                {
                    addedPipes.Add(location);
                    grid.AddEntity(location, new UndergroundPipe(grid.GetId(), direction));
                }
            }

            foreach (var terminals in context.CenterToTerminals.Values)
            {
                if (terminals.Count != 1 && !allowMultipleTerminals)
                {
                    throw new FactorioToolsException("Every pumpjack should have a single terminal selected.");
                }

                for (int i = 0; i < terminals.Count; i++)
                {
                    var terminal = terminals[i];
                    if (addedPipes.Add(terminal.Terminal))
                    {
                        grid.AddEntity(terminal.Terminal, new Terminal(grid.GetId()));
                    }
                }
            }

            foreach (var pipe in pipes.EnumerateItems())
            {
                if (addedPipes.Add(pipe))
                {
                    grid.AddEntity(pipe, new Pipe(grid.GetId()));
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
