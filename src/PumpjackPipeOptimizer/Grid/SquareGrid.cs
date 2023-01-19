namespace PumpjackPipeOptimizer.Grid;

internal abstract class SquareGrid
{
    private static readonly Location[] Directions = new[]
    {
        new Location(1, 0),
        new Location(0, -1),
        new Location(-1, 0),
        new Location(0, 1)
    };

    private const string EmptyLabel = ".";

    private readonly Dictionary<Location, GridEntity> _locationToEntity = new Dictionary<Location, GridEntity>();
    private readonly Dictionary<GridEntity, Location> _entityToLocation = new Dictionary<GridEntity, Location>();

    public SquareGrid(SquareGrid existing) : this (existing.Width, existing.Height)
    {
        _locationToEntity = new Dictionary<Location, GridEntity>(existing._locationToEntity);
        _entityToLocation = new Dictionary<GridEntity, Location> (existing._entityToLocation);
    }

    public SquareGrid(int width, int height)
    {
        Width = width;
        Height = height;
        Middle = new Location(Width / 2, Height / 2);
    }

    public int Width { get; }
    public int Height { get; }
    public Location Middle { get; }

    public IReadOnlyDictionary<Location, GridEntity> LocationToEntity => _locationToEntity;
    public IReadOnlyDictionary<GridEntity, Location> EntityToLocation => _entityToLocation;

    public bool IsEmpty(Location id)
    {
        return !_locationToEntity.ContainsKey(id);
    }

    public abstract double GetNeighborCost(Location a, Location b);

    public void AddEntity(Location id, GridEntity entity)
    {
        _locationToEntity.Add(id, entity);
        _entityToLocation.Add(entity, id);
    }

    public void RemoveEntity(Location id)
    {
        if (_locationToEntity.TryGetValue(id, out var entity))
        {
            _locationToEntity.Remove(id);
            _entityToLocation.Remove(entity);
        }
    }

    public bool IsEntityType<T>(Location id) where T : GridEntity
    {
        return _locationToEntity.TryGetValue(id, out var entity) && entity is T;
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
            Location next = new Location(id.X + dir.X, id.Y + dir.Y);
            if (IsInBounds(next))
            {
                yield return next;
            }
        }
    }

    public void WriteTo(TextWriter sw, int spacing = 0)
    {
        var maxLabelLength = _locationToEntity.Values.Max(x => x.Label.Length) + spacing;

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var location = new Location(x, y);
                if (_locationToEntity.TryGetValue(location, out var entity))
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
