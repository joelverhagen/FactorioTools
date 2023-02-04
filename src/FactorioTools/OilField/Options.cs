using Knapcode.FactorioTools.OilField.Data;

namespace Knapcode.FactorioTools.OilField;

class Options
{
    public static Options ForSmallIronElectricPole
    {
        get
        {
            return new Options
            {
                ElectricPoleEntityName = EntityNames.SpaceExploration.SmallIronElectricPole,
                ElectricPoleSupplyWidth = 5,
                ElectricPoleSupplyHeight = 5,
                ElectricPoleWireReach = 7.5,
                ElectricPoleWidth = 1,
                ElectricPoleHeight = 1,
            };
        }
    }

    public static Options ForSmallElectricPole
    {
        get
        {
            return new Options
            {
                ElectricPoleEntityName = EntityNames.Vanilla.SmallElectricPole,
                ElectricPoleSupplyWidth = 5,
                ElectricPoleSupplyHeight = 5,
                ElectricPoleWireReach = 7.5,
                ElectricPoleWidth = 1,
                ElectricPoleHeight = 1,
            };
        }
    }

    public static Options ForMediumElectricPole
    {
        get
        {
            return new Options
            {
                ElectricPoleEntityName = EntityNames.Vanilla.MediumElectricPole,
                ElectricPoleSupplyWidth = 7,
                ElectricPoleSupplyHeight = 7,
                ElectricPoleWireReach = 9,
                ElectricPoleWidth = 1,
                ElectricPoleHeight = 1,
            };
        }
    }

    public static Options ForBigElectricPole
    {
        get
        {
            return new Options
            {
                ElectricPoleEntityName = EntityNames.Vanilla.BigElectricPole,
                ElectricPoleSupplyWidth = 4,
                ElectricPoleSupplyHeight = 4,
                ElectricPoleWireReach = 30,
                ElectricPoleWidth = 2,
                ElectricPoleHeight = 2,
            };
        }
    }

    public static Options ForSubstation
    {
        get
        {
            return new Options
            {
                ElectricPoleEntityName = EntityNames.Vanilla.Substation,
                ElectricPoleSupplyWidth = 18,
                ElectricPoleSupplyHeight = 18,
                ElectricPoleWireReach = 18,
                ElectricPoleWidth = 2,
                ElectricPoleHeight = 2,
            };
        }
    }

    public bool UseUndergroundPipes { get; set; } = true;

    public string ElectricPoleEntityName { get; set; } = EntityNames.SpaceExploration.SmallIronElectricPole;
    public int ElectricPoleSupplyWidth { get; set; } = 3;
    public int ElectricPoleSupplyHeight { get; set; } = 3;
    public double ElectricPoleWireReach { get; set; } = 7.5;
    public int ElectricPoleWidth { get; set; } = 1;
    public int ElectricPoleHeight { get; set; } = 1;

    public Dictionary<string, int> PumpjackModules { get; set; } = new Dictionary<string, int>
    {
        { ItemNames.Vanilla.EffectivityModule3, 2 },
    };
}
