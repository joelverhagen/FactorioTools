﻿using PumpjackPipeOptimizer.Data;

namespace PumpjackPipeOptimizer.Grid;

internal class UndergroundPipe : Pipe
{
    public UndergroundPipe(Direction direction)
    {
        Direction = direction;
    }

    public Direction Direction { get; set; }
    public override string Label => Direction switch
    {
        Direction.Up => "^",
        Direction.Right => ">",
        Direction.Down => "v",
        Direction.Left => "<",
        _ => throw new NotImplementedException(),
    };
}
