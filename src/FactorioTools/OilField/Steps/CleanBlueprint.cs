using System;
using System.Collections.Generic;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public static class CleanBlueprint
{
    public static Blueprint Execute(Blueprint blueprint)
    {
        var context = InitializeContext.Execute(new OilFieldOptions(), blueprint, TableArray.Empty<AvoidLocation>());

        var entities = TableArray.New<Entity>();

        for (var i = 0; i < context.Centers.Count; i++)
        {
            var center = context.Centers[i];

            // Pumpjacks are given a direction that doesn't overlap with another pumpjack, preferring the direction
            // starting at the top then proceeding clockwise.
            var terminal = context.CenterToTerminals[center].EnumerateItems().MinBy(x => x.Direction)!;

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

        return new Blueprint
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
        };
    }
}
