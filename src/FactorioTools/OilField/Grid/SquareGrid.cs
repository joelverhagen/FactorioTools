using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Knapcode.FactorioTools.OilField;

public abstract class SquareGrid
{
    public const int NeighborCost = 1;

#if ENABLE_GRID_TOSTRING
    private const string EmptyLabel = ".";
#endif

    private readonly Dictionary<int, Location> _entityIdToLocation;
    private readonly ILocationSet _entityLocations;
    private readonly GridEntity?[] _grid;
    private int _nextId;

    public SquareGrid(SquareGrid existing, bool clone)
    {
        Width = existing.Width;
        Height = existing.Height;
        Middle = existing.Middle;

        if (clone)
        {
            _entityIdToLocation = new Dictionary<int, Location>(existing._entityIdToLocation);
#if USE_HASHSETS
            _entityLocations = new LocationHashSet(existing._entityLocations.Count);
#else
            _entityLocations = new LocationIntSet(existing.Width, existing._entityLocations.Count);
#endif
            _entityLocations.UnionWith(existing._entityLocations);

            _grid = (GridEntity?[])existing._grid.Clone();
            _nextId = existing._nextId;
        }
        else
        {
            _entityIdToLocation = existing._entityIdToLocation;
            _entityLocations = existing._entityLocations;
            _grid = existing._grid;
            _nextId = existing._nextId;
        }
    }

    public SquareGrid(int width, int height)
    {
        Width = width;
        Height = height;
        Middle = new Location(Width / 2, Height / 2);
        _entityIdToLocation = new Dictionary<int, Location>();
#if USE_HASHSETS
        _entityLocations = new LocationHashSet();
#else
        _entityLocations = new LocationIntSet(width);
#endif
        _grid = new GridEntity?[width * height];
        _nextId = 1;
    }

    public int Width { get; }
    public int Height { get; }
    public Location Middle { get; }

    public GridEntity? this[Location location]
    {
        get
        {
            return _grid[GetIndex(location)];
        }
    }

    public int GetId()
    {
        var id = _nextId;
        _nextId++;
        return id;
    }

    public IReadOnlyDictionary<int, Location> EntityIdToLocation => _entityIdToLocation;
    public ILocationSet EntityLocations => _entityLocations;

    public bool IsEmpty(Location location)
    {
        return _grid[GetIndex(location)] is null;
    }

    public void AddEntity(Location location, GridEntity entity)
    {
        var index = GetIndex(location);

        if (_grid[index] is not null)
        {
            throw new FactorioToolsException($"There is already an entity at {location}.");
        }

        _grid[index] = entity;
        _entityLocations.Add(location);
        _entityIdToLocation.Add(entity.Id, location);
    }

    public void RemoveEntity(Location location)
    {
        var index = GetIndex(location);
        var entity = _grid[index];
        if (entity is not null)
        {
            _grid[index] = null;
            _entityLocations.Remove(location);
            _entityIdToLocation.Remove(entity.Id);
            entity.Unlink();
        }
    }

    public bool IsEntityType<T>(Location location) where T : GridEntity
    {
        return _grid[GetIndex(location)] is T;
    }

    public bool IsInBounds(Location location)
    {
        return location.X >= 0 && location.X < Width && location.Y >= 0 && location.Y < Height;
    }

    public abstract void GetNeighbors(Span<Location> neighbors, Location location);

    public void GetAdjacent(Span<Location> adjacent, Location location)
    {
        var a = location.Translate(1, 0);
        adjacent[0] = IsInBounds(a) ? a : Location.Invalid;

        var b = location.Translate(0, -1);
        adjacent[1] = IsInBounds(b) ? b : Location.Invalid;

        var c = location.Translate(-1, 0);
        adjacent[2] = IsInBounds(c) ? c : Location.Invalid;

        var d = location.Translate(0, 1);
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
        var maxLabelLength = _entityLocations.EnumerateItems().Max(x => this[x]!.Label.Length) + spacing;
        var minX = _entityLocations.EnumerateItems().Min(l => l.X);
        var minY = _entityLocations.EnumerateItems().Min(l => l.Y);
        var maxX = _entityLocations.EnumerateItems().Max(l => l.X);
        var maxY = _entityLocations.EnumerateItems().Max(l => l.Y);

        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
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
        public static Empty Instance { get; } = new Empty(0);

        public Empty(int id) : base(id)
        {
        }

#if ENABLE_GRID_TOSTRING
        public override string Label => EmptyLabel;
#endif
    }
}
