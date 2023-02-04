using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class GridToBlueprintString
{
    public static string Execute(Context context)
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
                        Neighbours = electricPole.Neighbors.Select(GetEntityNumber).ToArray(),
                    });
                    break;
                case ElectricPoleSide:
                    // Ignore
                    break;
                default:
                    throw new NotImplementedException("Unknown entity type: " + gridEntity.GetType().FullName);
            }
        }

        var root = new BlueprintRoot
        {
            Blueprint = new Blueprint
            {
                Icons = context.InputBlueprint.Blueprint.Icons,
                Version = context.InputBlueprint.Blueprint.Version,
                Item = context.InputBlueprint.Blueprint.Item,
                Entities = entities.ToArray(),
            }
        };

        return SerializeBlueprint(root);
    }

    public static string SerializeBlueprint(BlueprintRoot root)
    {
        var json = JsonSerializer.Serialize(root, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        using var outputStream = new MemoryStream(); ;
        using var zlibStream = new ZLibStream(outputStream, CompressionLevel.Optimal);
        zlibStream.Write(bytes, 0, bytes.Length);
        zlibStream.Flush();
        zlibStream.Dispose();
        var base64 = Convert.ToBase64String(outputStream.ToArray());

        return '0' + base64;
    }
}
