using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.Benchmark;

internal class Program
{
    private static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<Benchmark>();
    }
}

public class Benchmark
{
    private static string BlueprintString = "0eJyM1MFqhDAQBuB3mXMOTkzcNa9SyuK6oaRdo2gsFfHdGxMPhS34n8QYPx0m/6x0f852GJ0PZFZybe8nMm8rTe7DN899zTedJUPD3A2fTftFgsIy7Csu2I42Qc4/7A8Z3t4FWR9ccDYb6Wa5+bm72zFuEP9YQz/FF3q/fyki5UXQEi+RfbjRtvlRsYkXTQKa5qSpc60ENKWSps81hVSasPoc0wimUK0CNM5N4OKcuyA/l7vAfM5dAU5WmQO6WiPF5kYw0FYukGo17iGJuGauAjgkEqrGPSQUUmYPOHqMxEJnTwKHhTVerwQmCiPZkDlpEqkXCYc8Bp4EPCQdR73lSzricE4D2/yZ+IK+7TgdG7ZfAAAA//8DABy5/JQ=";
    private BlueprintRoot Blueprint { get; }

    public Benchmark()
    {
        Blueprint = ParseBlueprint.Execute(BlueprintString);
    }

    [Benchmark]
    public void OilFieldPlanner()
    {
        var options = Options.ForMediumElectricPole;
        options.UseUndergroundPipes = false;
        var context = Planner.Execute(options, Blueprint!);
    }
}