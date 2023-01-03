using PumpjackPipeOptimizer.Data;

namespace PumpjackPipeOptimizer;

class Options
{
    public bool UseUndergroundPipes { get; set; } = true;

    public string ElectricPoleEntityName { get; set; } = EntityNames.SpaceExploration.SmallIronElectricPole;
    public int ElectricPoleSupplyWidth { get; set; } = 3;
    public int ElectricPoleSupplyHeight { get; set; } = 3;
    public double ElectricPoleWireReach { get; set; } = 7.5;
    public int ElectricPoleWidth { get;set; } = 1;
    public int ElectricPoleHeight { get; set; } = 1;

    public Dictionary<string, int> PumpjackModules { get; set; } = new Dictionary<string, int>
    {
        { ItemNames.Vanilla.EffectivityModule3, 2 },
    };
}
