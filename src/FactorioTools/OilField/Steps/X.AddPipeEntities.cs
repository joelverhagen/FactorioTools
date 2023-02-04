using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class AddPipeEntities
{
    public static void Execute(SquareGrid grid, IReadOnlyDictionary<Location, List<TerminalLocation>> centerToTerminals, HashSet<Location> pipes)
    {
        var addedTerminals = new HashSet<Location>();
        foreach (var terminals in centerToTerminals.Values)
        {
            var location = terminals.Single().Terminal;
            if (addedTerminals.Add(location))
            {
                grid.AddEntity(location, new Terminal());
            }
        }

        foreach (var pipe in pipes.Except(addedTerminals))
        {
            grid.AddEntity(pipe, new Pipe());
        }
    }
}
