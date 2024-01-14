using System;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public class UndergroundPipe : Pipe
{
    public UndergroundPipe(int id, Direction direction) : base(id)
    {
        Direction = direction;
    }

    public Direction Direction { get; }

#if ENABLE_GRID_TOSTRING
    public override string Label => Direction switch
    {
        Direction.Up => "^",
        Direction.Right => ">",
        Direction.Down => "v",
        Direction.Left => "<",
        _ => throw new NotImplementedException(),
    };
#endif
}
