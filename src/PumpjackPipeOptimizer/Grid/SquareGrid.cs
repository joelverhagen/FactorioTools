namespace PumpjackPipeOptimizer.Grid;

internal abstract class SquareGrid
{
    public static readonly IReadOnlyList<(int DeltaX, int DeltaY)> Directions = new[]
    {
        (1, 0),
        (0, -1),
        (-1, 0),
        (0, 1),
    };

    private const string EmptyLabel = ".";

    private readonly Dictionary<GridEntity, Location> _entityToLocation;
    private readonly GridEntity?[,] _grid;

    public SquareGrid(SquareGrid existing)
    {
        Width = existing.Width;
        Height = existing.Height;
        Middle = new Location(Width / 2, Height / 2);
        _entityToLocation = new Dictionary<GridEntity, Location>(existing._entityToLocation);
        _grid = (GridEntity[,])existing._grid.Clone();
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

    public abstract double GetNeighborCost(Location a, Location b);

    public void AddEntity(Location id, GridEntity entity)
    {
        if (_grid[id.X, id.Y] is not null)
        {
            throw new InvalidOperationException($"There is already an entity at {id}.");
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

    public abstract IEnumerable<Location> GetNeighbors(Location id);

    public IEnumerable<Location> GetAdjacent(Location id)
    {
        foreach (var dir in Directions)
        {
            Location next = id.Translate(dir);
            if (IsInBounds(next))
            {
                yield return next;
            }
        }
    }

    public void WriteTo(TextWriter sw, int spacing = 0)
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
                    sw.Write(entity.Label.PadRight(maxLabelLength));
                }
                else
                {
                    sw.Write(EmptyLabel.PadRight(maxLabelLength));
                }
            }

            sw.WriteLine();
        }
    }

    private class Empty : GridEntity
    {
        public static Empty Instance { get; } = new Empty();

        public override string Label => EmptyLabel;
    }
}
