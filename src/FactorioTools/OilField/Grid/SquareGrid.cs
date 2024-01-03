using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapcode.FactorioTools.OilField;

public abstract class SquareGrid
{
    public const int NeighborCost = 1;

#if ENABLE_GRID_TOSTRING
    private const string EmptyLabel = ".";
#endif

    private readonly Dictionary<GridEntity, Location> _entityToLocation;
    private readonly GridEntity?[,] _grid;

    public SquareGrid(SquareGrid existing, bool clone)
    {
        Width = existing.Width;
        Height = existing.Height;
        Middle = new Location(Width / 2, Height / 2);
        _entityToLocation = clone ? new Dictionary<GridEntity, Location>(existing._entityToLocation) : existing._entityToLocation;
        _grid = clone ? (GridEntity[,])existing._grid.Clone() : existing._grid;
    }

    public SquareGrid(int width, int height)
    {
        Width = width;
        Height = height;
        Middle = new Location(Width / 2, Height / 2);
        _entityToLocation = new Dictionary<GridEntity, Location>();
        _grid = new GridEntity?[width, height];
    }

    public int Width { get; }
    public int Height { get; }
    public Location Middle { get; }

    public GridEntity? this[Location id]
    {
        get
        {
            return _grid[id.X, id.Y];
        }
    }

    public IReadOnlyDictionary<GridEntity, Location> EntityToLocation => _entityToLocation;

    public bool IsEmpty(Location id)
    {
        return _grid[id.X, id.Y] is null;
    }

    public void AddEntity(Location id, GridEntity entity)
    {
        if (_grid[id.X, id.Y] is not null)
        {
            throw new FactorioToolsException($"There is already an entity at {id}.");
        }

        _grid[id.X, id.Y] = entity;
        _entityToLocation.Add(entity, id);
    }

    public void RemoveEntity(Location id)
    {
        var entity = _grid[id.X, id.Y];
        if (entity is not null)
        {
            _grid[id.X, id.Y] = null;
            _entityToLocation.Remove(entity);
            entity.Unlink();
        }
    }

    public bool IsEntityType<T>(Location id) where T : GridEntity
    {
        return _grid[id.X, id.Y] is T;
    }

    public bool IsInBounds(Location id)
    {
        return id.X >= 0 && id.X < Width && id.Y >= 0 && id.Y < Height;
    }

    public abstract void GetNeighbors(Span<Location> neighbors, Location id);

    public void GetAdjacent(Span<Location> adjacent, Location id)
    {
        var a = id.Translate(1, 0);
        adjacent[0] = IsInBounds(a) ? a : Location.Invalid;

        var b = id.Translate(0, -1);
        adjacent[1] = IsInBounds(b) ? b : Location.Invalid;

        var c = id.Translate(-1, 0);
        adjacent[2] = IsInBounds(c) ? c : Location.Invalid;

        var d = id.Translate(0, 1);
        adjacent[3] = IsInBounds(d) ? d : Location.Invalid;
    }

#if ENABLE_GRID_TOSTRING
    public override string ToString()
    {
        var builder = new StringBuilder();
        ToString(builder, spacing: 1);
        return builder.ToString();
    }

    public void ToString(StringBuilder builder, int spacing)
    {
        var maxLabelLength = _entityToLocation.Keys.Max(x => x.Label.Length) + spacing;

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var location = new Location(x, y);
                var entity = _grid[x, y];
                if (entity is not null)
                {
                    builder.Append(entity.Label.PadRight(maxLabelLength));
                }
                else
                {
                    builder.Append(EmptyLabel.PadRight(maxLabelLength));
                }
            }

            builder.AppendLine();
        }
    }
#else
    public void ToString(StringBuilder builder, int spacing)
    {
    }
#endif

    private class Empty : GridEntity
    {
        public static Empty Instance { get; } = new Empty();

#if ENABLE_GRID_TOSTRING
        public override string Label => EmptyLabel;
#endif
    }
}
