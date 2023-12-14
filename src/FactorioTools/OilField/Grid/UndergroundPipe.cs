using System;
using Knapcode.FactorioTools.OilField.Data;

namespace Knapcode.FactorioTools.OilField.Grid;

public class UndergroundPipe : Pipe
{
    public UndergroundPipe(Direction direction)
    {
        Direction = direction;
    }

    public Direction Direction { get; }

    public override string Label => Direction switch
    {
        Direction.Up => "^",
        Direction.Right => ">",
        Direction.Down => "v",
        Direction.Left => "<",
        _ => throw new NotImplementedException(),
    };
}
