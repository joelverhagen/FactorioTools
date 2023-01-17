﻿using SixLabors.ImageSharp;
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

    public static void Show(SquareGrid grid, IEnumerable<IPoint> points, IEnumerable<IEdge> edges)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PumpjackPipeOptimizer.bmp");
        Save(grid, path, points, edges);

        var p = new Process();
        p.StartInfo = new ProcessStartInfo(path) { UseShellExecute = true };
        p.Start();
    }

    private static void Save(SquareGrid grid, string path, IEnumerable<IPoint> points, IEnumerable<IEdge> edges)
    {
        var image = new Image<Rgba32>(grid.Width * CellSize, grid.Height * CellSize);

        for (var y = 0; y < grid.Height; y++)
        {
            for (var x = 0; x < grid.Width; x++)
            {
                var location = new Location(x, y);
                Color color;
                if (grid.LocationToEntity.TryGetValue(location, out var entity))
                {
                    color = entity switch
                    {
                        PumpjackSide _ => Color.Green,
                        PumpjackCenter _ => Color.DarkGreen,
                        Terminal _ => Color.DarkGray,
                        Pipe _ => Color.Gray,
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
            var point = new EllipsePolygon(ConvertPoint(p), CellSize / 4.0f);
            image.Mutate(c => c.Fill(Color.Red.WithAlpha(1f), point));
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