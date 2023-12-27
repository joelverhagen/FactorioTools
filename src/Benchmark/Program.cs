using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.Data;
using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.Benchmark;

public class Program
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
    public void MediumElectricPole_NoBeacon_NoUnderground()
    {
        var options = OilFieldOptions.ForMediumElectricPole;
        options.AddBeacons = false;
        options.UseUndergroundPipes = false;
        Planner.Execute(options, Blueprint);
    }

    [Benchmark]
    public void SmallElectricPole_Beacon_Underground()
    {
        var options = OilFieldOptions.ForSmallElectricPole;
        options.AddBeacons = true;
        options.UseUndergroundPipes = true;
        Planner.Execute(options, Blueprint);
    }

    [Benchmark]
    public void MediumElectricPole_Beacon_Underground()
    {
        var options = OilFieldOptions.ForMediumElectricPole;
        options.AddBeacons = true;
        options.UseUndergroundPipes = true;
        Planner.Execute(options, Blueprint);
    }

    [Benchmark]
    public void BigElectricPole_Beacon_Underground()
    {
        var options = OilFieldOptions.ForBigElectricPole;
        options.AddBeacons = true;
        options.UseUndergroundPipes = true;
        Planner.Execute(options, Blueprint);
    }

    [Benchmark]
    public void Substation_Beacon_Underground()
    {
        var options = OilFieldOptions.ForSubstation;
        options.AddBeacons = true;
        options.UseUndergroundPipes = true;
        Planner.Execute(options, Blueprint);
    }
}