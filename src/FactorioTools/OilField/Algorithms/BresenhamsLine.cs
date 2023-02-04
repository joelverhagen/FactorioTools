namespace Knapcode.FactorioTools.OilField.Algorithms;

internal static class BresenhamsLine
{
    public static List<Location> GetPath(Location a, Location b)
    {
        var line = new List<Location>();
        var dx = Math.Abs(b.X - a.X);
        var sx = a.X < b.X ? 1 : -1;
        var dy = -1 * Math.Abs(b.Y - a.Y);
        var sy = a.Y < b.Y ? 1 : -1;
        var error = dx + dy;

        var x0 = a.X;
        var y0 = a.Y;

        while (true)
        {
            line.Add(new Location(x0, y0));

            if (x0 == b.X && y0 == b.Y)
            {
                break;
            }

            var e2 = 2 * error;
            if (e2 >= dy)
            {
                if (x0 == b.X)
                {
                    break;
                }
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                if (y0 == b.Y)
                {
                    break;
                }

                error += dx;
                y0 += sy;
            }
        }

        return line;
    }
}
