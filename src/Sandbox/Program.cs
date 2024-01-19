﻿using System.Text;
using Knapcode.FactorioTools.OilField;

public partial class Program
{
    private static readonly string SmallListDataPath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "small-list.txt");
    private static readonly string BigListDataPath = Path.Combine(GetRepositoryRoot(), "test", "FactorioTools.Test", "OilField", "big-list.txt");

    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "normalize")
        {
            NormalizeBlueprints.Execute(SmallListDataPath, BigListDataPath, allowComments: true);
            NormalizeBlueprints.Execute(BigListDataPath, SmallListDataPath, allowComments: false);
        }
        else if (args.Length > 0 && args[0] == "sample")
        {
            var (context, summary) = Planner.ExecuteSample();
            Console.WriteLine(context.Grid.ToString());
        }
        else
        {
            Sandbox();
        }
    }

    private static string GetRepositoryRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir is not null)
        {
            if (Directory.GetFiles(dir).Select(Path.GetFileName).Contains("FactorioTools.sln"))
            {
                return dir;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException("Could not find the repository root. Current directory: " + Directory.GetCurrentDirectory());
    }

    private static void Sandbox()
    {
        for (var i = 0; i < 100; i++)
        {
            Console.Write(".");
            Planner.ExecuteSample();
        }
    }
}