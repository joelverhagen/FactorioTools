using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using Knapcode.FactorioTools.Data;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

namespace Knapcode.FactorioTools.OilField;

public static class GridToBlueprintString
{
    private static readonly IReadOnlyDictionary<string, (float Width, float Height)> EntityNameToSize = new Dictionary<string, (float Width, float Height)>()
    {
        { EntityNames.Vanilla.Beacon, (3, 3) },
        { EntityNames.Vanilla.BigElectricPole, (2, 2) },
        { EntityNames.Vanilla.MediumElectricPole, (1, 1) },
        { EntityNames.Vanilla.Pipe, (1, 1) },
        { EntityNames.Vanilla.PipeToGround, (1, 1) },
        { EntityNames.Vanilla.Pumpjack, (3, 3) },
        { EntityNames.Vanilla.SmallElectricPole, (1, 1) },
        { EntityNames.Vanilla.Substation, (2, 2) },

        { EntityNames.AaiIndustry.SmallIronElectricPole, (1, 1) },
    };

    public static string Execute(Context context, bool addFbeOffset)
    {
        var entities = new List<Entity>();
        var nextEntityNumber = 1;

        var electricPoleToEntityNumber = new Dictionary<ElectricPoleCenter, int>();

        int GetEntityNumber(ElectricPoleCenter pole)
        {
            if (!electricPoleToEntityNumber!.TryGetValue(pole, out var entityNumber))
            {
                entityNumber = nextEntityNumber++;
                electricPoleToEntityNumber.Add(pole, entityNumber);
            }

            return entityNumber;
        }

        foreach ((var gridEntity, var location) in context.Grid.EntityToLocation)
        {
            var position = new Position
            {
                X = location.X,
                Y = location.Y,
            };

            switch (gridEntity)
            {
                case PumpjackCenter:
                    entities.Add(new Entity
                    {
                        EntityNumber = nextEntityNumber++,
                        Direction = context.CenterToTerminals[location].Single().Direction,
                        Name = EntityNames.Vanilla.Pumpjack,
                        Position = position,
                        Items = context.Options.PumpjackModules,
                    });
                    break;
                case PumpjackSide:
                    // Ignore
                    break;
                case UndergroundPipe undergroundPipe:
                    entities.Add(new Entity
                    {
                        EntityNumber = nextEntityNumber++,
                        Direction = undergroundPipe.Direction,
                        Name = EntityNames.Vanilla.PipeToGround,
                        Position = position,
                    });
                    break;
                case Pipe:
                    entities.Add(new Entity
                    {
                        EntityNumber = nextEntityNumber++,
                        Name = EntityNames.Vanilla.Pipe,
                        Position = position,
                    });
                    break;
                case ElectricPoleCenter electricPole:
                    if (context.Options.ElectricPoleWidth % 2 == 0)
                    {
                        position.X += 0.5f;
                    }

                    if (context.Options.ElectricPoleHeight % 2 == 0)
                    {
                        position.Y += 0.5f;
                    }

                    entities.Add(new Entity
                    {
                        EntityNumber = GetEntityNumber(electricPole),
                        Name = context.Options.ElectricPoleEntityName,
                        Position = position,
                        Neighbours = electricPole.Neighbors.EnumerateItems().Select(GetEntityNumber).ToArray(),
                    });
                    break;
                case ElectricPoleSide:
                    // Ignore
                    break;
                case BeaconCenter beacon:
                    if (context.Options.BeaconWidth % 2 == 0)
                    {
                        position.X += 0.5f;
                    }

                    if (context.Options.BeaconHeight % 2 == 0)
                    {
                        position.Y += 0.5f;
                    }

                    entities.Add(new Entity
                    {
                        EntityNumber = nextEntityNumber++,
                        Name = context.Options.BeaconEntityName,
                        Position = position,
                        Items = context.Options.BeaconModules,
                    });
                    break;
                case BeaconSide:
                case TemporaryEntity:
                    // Ignore
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        var blueprint = new Blueprint
        {
            Icons = context.InputBlueprint.Icons,
            Version = context.InputBlueprint.Version,
            Item = context.InputBlueprint.Item,
            Entities = entities.ToArray(),
        };

        return SerializeBlueprint(blueprint, addFbeOffset);
    }

    public static string SerializeBlueprint(Blueprint blueprint, bool addFbeOffset)
    {
        // FBE applies some offset to the blueprint coordinates. This makes it hard to compare the grid used in memory
        // with the rendered blueprint in FBE. To account for this, we can add an entity to the corner of the
        // blueprint with a position that makes FBE keep the original entity positions used by the grid.
        if (addFbeOffset && blueprint.Entities.Length > 0)
        {
            var maxX = float.MinValue;
            var maxY = float.MinValue;
            var maxEntityNumber = int.MinValue;

            foreach (var entity in blueprint.Entities)
            {
                (var width, var height) = EntityNameToSize[entity.Name];
                maxX = Math.Max(maxX, entity.Position.X + width / 2);
                maxY = Math.Max(maxY, entity.Position.Y + height / 2);
                maxEntityNumber = Math.Max(maxEntityNumber, entity.EntityNumber);
            }

            blueprint.Entities = blueprint.Entities.Append(new Entity
            {
                EntityNumber = maxEntityNumber + 1,
                Name = EntityNames.Vanilla.Wall,
                Position = new Position
                {
                    X = (float)-Math.Ceiling(maxX),
                    Y = (float)-Math.Ceiling(maxY),
                },
            }).ToArray();
        }

        var root = new BlueprintRoot { Blueprint = blueprint };

        var json = JsonSerializer.Serialize(root, typeof(BlueprintRoot), new BlueprintSerializationContext(new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }));

        var bytes = Encoding.UTF8.GetBytes(json);
        using var outputStream = new MemoryStream();

        using var zlibStream = new ZLibStream(outputStream, CompressionLevel.Optimal);
        zlibStream.Write(bytes, 0, bytes.Length);
        zlibStream.Flush();
        zlibStream.Dispose();
        var base64 = Convert.ToBase64String(outputStream.ToArray());

        return '0' + base64;
    }
}
