using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.OilField.Steps;

Console.WriteLine("Hello, Browser!");

public partial class MyClass
{
    [JSExport]
    internal static string Greeting()
    {
        var sw = Stopwatch.StartNew();
        var options = Options.ForMediumElectricPole;
        var blueprintString = "0eJyM1MFqhDAQBuB3mXMOTkzcNa9SyuK6oaRdo2gsFfHdGxMPhS34n8QYPx0m/6x0f852GJ0PZFZybe8nMm8rTe7DN899zTedJUPD3A2fTftFgsIy7Csu2I42Qc4/7A8Z3t4FWR9ccDYb6Wa5+bm72zFuEP9YQz/FF3q/fyki5UXQEi+RfbjRtvlRsYkXTQKa5qSpc60ENKWSps81hVSasPoc0wimUK0CNM5N4OKcuyA/l7vAfM5dAU5WmQO6WiPF5kYw0FYukGo17iGJuGauAjgkEqrGPSQUUmYPOHqMxEJnTwKHhTVerwQmCiPZkDlpEqkXCYc8Bp4EPCQdR73lSzricE4D2/yZ+IK+7TgdG7ZfAAAA//8DABy5/JQ=";
        var blueprint = ParseBlueprint.Execute(blueprintString);
        var context = Planner.Execute(options, blueprint);
        var outputBlueprintString = GridToBlueprintString.Execute(context, addOffsetCorrection: false);
        return $"Elapsed: {sw.Elapsed}, blueprint: {outputBlueprintString}";
    }

    [JSImport("window.location.href", "main.js")]
    internal static partial string GetHRef();
}
