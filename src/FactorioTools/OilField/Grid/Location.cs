public struct Location : IEquatable<Location>, IComparable<Location>
{
    public Location(int x, int y)
    {
        X = x;
        Y = y;
        IsValid = X >= 0 && Y >= 0;
    }

    public int X { get; }
    public int Y { get; }
    public bool IsValid { get; }

    public static Location Invalid => new Location(-1, -1);

    public override bool Equals(object? obj)
    {
        return obj is Location location && Equals(location);
    }

    public bool Equals(Location other)
    {
        return X == other.X &&
               Y == other.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public int GetManhattanDistance(Location other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    }

    public double GetEuclideanDistance(Location other)
    {
        return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
    }

    public double GetEuclideanDistance(double bX, double bY)
    {
        return Math.Sqrt(Math.Pow(X - bX, 2) + Math.Pow(Y - bY, 2));
    }

    public Location Translate(int deltaX, int deltaY)
    {
        return new Location(X + deltaX, Y + deltaY);
    }

    public Location Translate((int DeltaX, int DeltaY) translation)
    {
        return new Location(X + translation.DeltaX, Y + translation.DeltaY);
    }

    public int CompareTo(Location other)
    {
        var x = X.CompareTo(other.X);
        if (x != 0)
        {
            return x;
        }

        return Y.CompareTo(other.Y);
    }

    public static bool operator ==(Location left, Location right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Location left, Location right)
    {
        return !(left == right);
    }

    public static bool operator <(Location left, Location right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Location left, Location right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Location left, Location right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Location left, Location right)
    {
        return left.CompareTo(right) >= 0;
    }

    public override string ToString()
    {
        // Show the real X and Y coordinates as well as line and column numbers (in VS Code format).
        return $"({X}, {Y}) / (Ln {Y + 1}, Col {X + 1})";
    }
}
