using Knapcode.FactorioTools.OilField.Data;

namespace Knapcode.FactorioTools.OilField.Steps;

public static class CleanBlueprint
{
    public static BlueprintRoot Execute(BlueprintRoot root)
    {
        var context = InitializeContext.Execute(new Options(), root);

        var entities = new List<Entity>();

        // Pumpjacks are sorted by their Y coordinate, then they're X coordinate.
        var sortedCenters = context.CenterToTerminals.Keys.OrderBy(c => c.Y).ThenBy(c => c.X);

        foreach (var center in sortedCenters)
        {
            // Pumpjacks are given a direction that doesn't overlap with another pumpjack, preferring the direction
            // starting at the top then proceeding clockwise.
            var terminal = context.CenterToTerminals[center].MinBy(x => x.Direction)!;

            entities.Add(new Entity
            {
                EntityNumber = entities.Count + 1,
                Direction = terminal.Direction,
                Name = EntityNames.Vanilla.Pumpjack,
                Position = new Position
                {
                    X = center.X,
                    Y = center.Y,
                },
            });
        }

        return new BlueprintRoot
        {
            Blueprint = new Blueprint
            {
                Entities = entities.ToArray(),
                Icons = new Icon[]
                {
                    new Icon
                    {
                        Index = 1,
                        Signal = new SignalID
                        {
                            Name = EntityNames.Vanilla.Pumpjack,
                            Type = SignalTypes.Vanilla.Item,
                        }
                    }
                },
                Item = ItemNames.Vanilla.Blueprint,
                Version = 0,
            }
        };
    }
}
