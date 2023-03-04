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
                ElectricPoleEntityName = EntityNames.AaiIndustry.SmallIronElectricPole,
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
    public bool AddBeacons { get; set; } = true;
    public bool OptimizePipes { get; set; } = true;
    public bool OverlapBeacons { get; set; } = true;
    public HashSet<PipeStrategy> PipeStrategies { get; set; } = Enum.GetValues<PipeStrategy>().ToHashSet();
    public HashSet<BeaconStrategy> BeaconStrategies { get; set; } = Enum.GetValues<BeaconStrategy>().ToHashSet();

    public string ElectricPoleEntityName { get; set; } = EntityNames.Vanilla.MediumElectricPole;
    public int ElectricPoleSupplyWidth { get; set; } = 7;
    public int ElectricPoleSupplyHeight { get; set; } = 7;

    private const double DefaultElectricPoleWireReach = 9;
    private double _electricPoleWireReach = DefaultElectricPoleWireReach;

    public double ElectricPoleWireReach
    {
        get => _electricPoleWireReach;
        set
        {
            _electricPoleWireReach = value;
            ElectricPoleWireReachSquared = value * value;
        }
    }

    public double ElectricPoleWireReachSquared { get; private set; } = DefaultElectricPoleWireReach * DefaultElectricPoleWireReach;

    public int ElectricPoleWidth { get; set; } = 1;
    public int ElectricPoleHeight { get; set; } = 1;

    public string BeaconEntityName { get; set; } = EntityNames.Vanilla.Beacon;
    public int BeaconSupplyWidth { get; set; } = 9;
    public int BeaconSupplyHeight { get; set; } = 9;
    public int BeaconWidth { get; set; } = 3;
    public int BeaconHeight { get; set; } = 3;

    public bool ValidateSolution { get; set; } = false;

    public Dictionary<string, int> PumpjackModules { get; set; } = new Dictionary<string, int>
    {
        { ItemNames.Vanilla.ProductivityModule3, 2 },
    };

    public Dictionary<string, int> BeaconModules { get; set; } = new Dictionary<string, int>
    {
        { ItemNames.Vanilla.SpeedModule3, 2 },
    };
}
