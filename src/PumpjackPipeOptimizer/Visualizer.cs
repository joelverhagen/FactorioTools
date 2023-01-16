using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using PumpjackPipeOptimizer.Grid;
using DelaunatorSharp;
using SixLabors.ImageSharp.Drawing;

namespace PumpjackPipeOptimizer;

internal static class Visualizer
{
    private const int CellSize = 64;

    public static void Show(Context context, IEnumerable<IPoint> points, IEnumerable<IEdge> edges)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PumpjackPipeOptimizer.bmp");
        Save(context, path, points, edges);

        var p = new Process();
        p.StartInfo = new ProcessStartInfo(path) { UseShellExecute = true };
        p.Start();
    }

    private static void Save(Context context, string path, IEnumerable<IPoint> points, IEnumerable<IEdge> edges)
    {
        var image = new Image<Rgba32>(context.Grid.Width * CellSize, context.Grid.Height * CellSize);

        for (var y = 0; y < context.Grid.Height; y++)
        {
            for (var x = 0; x < context.Grid.Width; x++)
            {
                var location = new Location(x, y);
                Color color;
                if (context.Grid.LocationToEntity.TryGetValue(location, out var entity))
                {
                    color = entity switch
                    {
                        PumpjackSide _ => Color.Green,
                        PumpjackCenter _ => Color.DarkGreen,
                        _ => throw new NotImplementedException(),
                    };
                }
                else
                {
                    color = Color.White;
                }

                var cell = new Rectangle(location.X * CellSize, location.Y * CellSize, CellSize, CellSize);
                image.Mutate(c => c.Fill(Color.Black, cell));

                var entityR = new Rectangle(location.X * CellSize + 1, location.Y * CellSize + 1 , CellSize - 2, CellSize - 2);
                image.Mutate(c => c.Fill(color, entityR));
            }
        }

        foreach (var p in points)
        {
            var point = new EllipsePolygon(ConvertPoint(p), CellSize / 2.0f);
            image.Mutate(c => c.Fill(Color.Red.WithAlpha(0.2f), point));
        }

        foreach (var e in edges)
        {
            image.Mutate(c =>
            {
                c.DrawLines(Color.Red.WithAlpha(1f), thickness: 5, ConvertPoint(e.P), ConvertPoint(e.Q));
            });
        }

        image.SaveAsBmp(path);
    }

    private static PointF ConvertPoint(IPoint p)
    {
        return new PointF((float)((p.X + 0.5f) * CellSize), (float)((p.Y + 0.5f) * CellSize));
    }
}
