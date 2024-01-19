#if ENABLE_VISUALIZER
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using SixLabors.ImageSharp.Drawing;
using DelaunatorSharp;
using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public static class Visualizer
{
    private const int CellSize = 64;

    public static void Show(SquareGrid grid)
    {
        Show(grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());
    }

    public static void Show(SquareGrid grid, IReadOnlyCollection<IPoint> points, IReadOnlyCollection<IEdge> edges)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Knapcode.FactorioTools.OilField.png");
        Save(grid, path, points, edges);

        var p = new Process();
        p.StartInfo = new ProcessStartInfo(path) { UseShellExecute = true };
        p.Start();
    }

    private static void Save(SquareGrid grid, string path, IReadOnlyCollection<IPoint> points, IReadOnlyCollection<IEdge> edges)
    {
        var image = new Image<Rgba32>(grid.Width * CellSize, grid.Height * CellSize);

        image.Mutate(c =>
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var location = new Location(x, y);
                    Color color;
                    GridEntity? entity;
                    if ((entity = grid[location]) is not null)
                    {
                        color = entity switch
                        {
                            PumpjackSide _ => Color.Green,
                            PumpjackCenter _ => Color.DarkGreen,
                            Terminal _ => Color.DarkGray,
                            Pipe _ => Color.Gray,
                            ElectricPoleSide _ => Color.Yellow,
                            ElectricPoleCenter _ => Color.Gold,
                            BeaconSide _ => Color.MediumPurple,
                            BeaconCenter _ => Color.Purple,
                            TemporaryEntity _ => Color.Pink,
                            _ => throw new NotImplementedException(),
                        };
                    }
                    else
                    {
                        color = Color.White;
                    }

                    const int gridLineWidth = 1;
                    var cell = new Rectangle(location.X * CellSize, location.Y * CellSize, CellSize, CellSize);
                    c.Fill(Color.Gray, cell);

                    var entityR = new Rectangle(location.X * CellSize + gridLineWidth, location.Y * CellSize + gridLineWidth, CellSize - 2 * gridLineWidth, CellSize - 2 * gridLineWidth);
                    c.Fill(color, entityR);
                }
            }

            var pairs = new HashSet<(Location A, Location B)>();
            foreach (var location in grid.EntityLocations.EnumerateItems())
            {
                var center = grid[location] as ElectricPoleCenter;
                if (center is null)
                {
                    continue;
                }

                foreach (var neighbor in center.Neighbors)
                {
                    var (a, b) = (location, grid.EntityIdToLocation[neighbor]);
                    if (a > b)
                    {
                        (a, b) = (b, a);
                    }

                    if (pairs.Add((a, b)))
                    {
                        c.DrawLines(Color.Chocolate, thickness: 4, new PointF(a.X * CellSize + CellSize / 2, a.Y * CellSize + CellSize / 2), new PointF(b.X * CellSize + CellSize / 2, b.Y * CellSize + CellSize / 2));
                    }
                }
            }

            foreach (var p in points)
            {
                var point = new EllipsePolygon(ConvertPoint(p), CellSize / 4.0f);
                c.Fill(Color.Red.WithAlpha(1f), point);
            }

            foreach (var e in edges)
            {
                c.DrawLines(Color.Red.WithAlpha(1f), thickness: 5, ConvertPoint(e.P), ConvertPoint(e.Q));
            }

            // Draw the grid
            var gridColor = Color.Red.WithAlpha(0.5f);

            const int thickness = 4;
            const int thickness2 = 0;

            c.DrawLines(gridColor, thickness, new PointF(grid.Width * CellSize - 1, 0), new PointF(grid.Width * CellSize - 1, grid.Height * CellSize - 1));
            c.DrawLines(gridColor, thickness, new PointF(grid.Width * CellSize - 1, grid.Height * CellSize - 1), new PointF(0, grid.Height * CellSize - 1));

            for (var x = 0; x < grid.Width * CellSize; x += 10 * CellSize)
            {
                c.DrawLines(gridColor, thickness, new PointF(x - thickness2, 0 - thickness2), new PointF(x - thickness2, grid.Height * CellSize - 1 - thickness2));
            }

            for (var y = 0; y < grid.Height * CellSize; y += 10 * CellSize)
            {
                c.DrawLines(gridColor, thickness, new PointF(grid.Width * CellSize - 1 - thickness2, y - thickness2), new PointF(0 - thickness2, y - thickness2));
            }
        });

        image.SaveAsPng(path);
    }

    private static PointF ConvertPoint(IPoint p)
    {
        return new PointF((float)((p.X + 0.5f) * CellSize), (float)((p.Y + 0.5f) * CellSize));
    }
}
#endif