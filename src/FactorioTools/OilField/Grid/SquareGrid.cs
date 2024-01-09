using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Knapcode.FactorioTools.OilField;

public abstract class SquareGrid
{
    public const int NeighborCost = 1;

#if ENABLE_GRID_TOSTRING
    private const string EmptyLabel = ".";
#endif

    private readonly Dictionary<GridEntity, Location> _entityToLocation;
    private readonly GridEntity?[] _grid;

    public SquareGrid(SquareGrid existing, bool clone)
    {
        Width = existing.Width;
        Height = existing.Height;
        Middle = new Location(Width / 2, Height / 2);
        _entityToLocation = clone ? new Dictionary<GridEntity, Location>(existing._entityToLocation) : existing._entityToLocation;

        if (clone)
        {
            _grid = (GridEntity?[])existing._grid.Clone();
        }
        else
        {
            _grid = existing._grid;
        }
    }

    public SquareGrid(int width, int height)
    {
        Width = width;
        Height = height;
        Middle = new Location(Width / 2, Height / 2);
        _entityToLocation = new Dictionary<GridEntity, Location>();
        _grid = new GridEntity?[width * height];
    }

    public int Width { get; }
    public int Height { get; }
    public Location Middle { get; }

    public GridEntity? this[Location id]
    {
        get
        {
            return _grid[GetIndex(id)];
        }
    }

    public IReadOnlyDictionary<GridEntity, Location> EntityToLocation => _entityToLocation;

    public bool IsEmpty(Location id)
    {
        return _grid[GetIndex(id)] is null;
    }

    public void AddEntity(Location id, GridEntity entity)
    {
        var index = GetIndex(id);

        if (_grid[index] is not null)
        {
            throw new FactorioToolsException($"There is already an entity at {id}.");
        }

        _grid[index] = entity;
        _entityToLocation.Add(entity, id);
    }

    public void RemoveEntity(Location id)
    {
        var index = GetIndex(id);
        var entity = _grid[index];
        if (entity is not null)
        {
            _grid[index] = null;
            _entityToLocation.Remove(entity);
            entity.Unlink();
        }
    }

    public bool IsEntityType<T>(Location id) where T : GridEntity
    {
        return _grid[GetIndex(id)] is T;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(Location location)
    {
        return location.Y * Width + location.X;
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
                var entity = _grid[y * Width + x];
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
