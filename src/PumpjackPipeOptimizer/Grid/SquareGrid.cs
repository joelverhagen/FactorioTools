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

    private readonly Dictionary<Location, GridEntity> _entities = new Dictionary<Location, GridEntity>();

    public SquareGrid(SquareGrid existing) : this (existing.Width, existing.Height)
    {
        _entities = new Dictionary<Location, GridEntity>(existing._entities);
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
    public IReadOnlyDictionary<Location, GridEntity> Entities => _entities;

    public bool IsEmpty(Location id)
    {
        return !_entities.ContainsKey(id);
    }

    public abstract double GetNeighborCost(Location a, Location b);

    public void AddEntity(Location id, GridEntity entity)
    {
        _entities.Add(id, entity);
    }

    public void RemoveEntity(Location id)
    {
        _entities.Remove(id);
    }

    public bool IsEntityType<T>(Location id) where T : GridEntity
    {
        return _entities.TryGetValue(id, out var entity) && entity is T;
    }

    public bool IsInBounds(Location id)
    {
        return id.X >= 0 && id.X < Width && id.Y >= 0 && id.Y < Height;
    }

    public abstract IEnumerable<Location> GetNeighbors(Location id);

    protected IEnumerable<Location> GetAdjacent(Location id)
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

    public void WriteTo(TextWriter sw, int spacing = 1)
    {
        var maxLabelLength = _entities.Values.Max(x => x.Label.Length) + spacing;

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var location = new Location(x, y);
                if (_entities.TryGetValue(location, out var entity))
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
