using System.Globalization;

public struct Location : IEquatable<Location>, IComparable<Location>, IFormattable
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
        var hash = 23;
        hash = hash * 31 + X;
        hash = hash * 31 + Y;
        return hash;
    }

    public int GetManhattanDistance(Location other)
    {
        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    }

    public double GetEuclideanDistance(Location other)
    {
        var deltaX = X - other.X;
        var deltaY = Y - other.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    public int GetEuclideanDistanceSquared(Location other)
    {
        var deltaX = X - other.X;
        var deltaY = Y - other.Y;
        return (deltaX * deltaX) + (deltaY * deltaY);
    }

    public double GetEuclideanDistance(double bX, double bY)
    {
        var deltaX = X - bX;
        var deltaY = Y - bY;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    public int GetEuclideanDistanceSquared(int bX, int bY)
    {
        var deltaX = X - bX;
        var deltaY = Y - bY;
        return (deltaX * deltaX) + (deltaY * deltaY);
    }

    public double GetEuclideanDistanceSquared(double bX, double bY)
    {
        var deltaX = X - bX;
        var deltaY = Y - bY;
        return (deltaX * deltaX) + (deltaY * deltaY);
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
        return ToString("S", formatProvider: null);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return format switch
        {
            "M" => $"({X}, {Y})",

            // Show the real X and Y coordinates as well as line and column numbers (in VS Code format).
            "S" => $"({X}, {Y}) / (Ln {Y + 1}, Col {X + 1})",

            _ => throw new ArgumentException("Format string is not supported. Use 'M' or 'S'.", nameof(format))
        };
    }
}
