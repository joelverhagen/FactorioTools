using System;
using System.Collections.Generic;
using System.Linq;
using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public class OilFieldOptions
{
    public static OilFieldOptions ForSmallIronElectricPole
    {
        get
        {
            return new OilFieldOptions
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

    public static OilFieldOptions ForSmallElectricPole
    {
        get
        {
            return new OilFieldOptions
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

    public static OilFieldOptions ForMediumElectricPole
    {
        get
        {
            return new OilFieldOptions
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

    public static OilFieldOptions ForBigElectricPole
    {
        get
        {
            return new OilFieldOptions
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

    public static OilFieldOptions ForSubstation
    {
        get
        {
            return new OilFieldOptions
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

    public static IReadOnlyList<PipeStrategy> AllPipeStrategies { get; } = new[]
    {
        PipeStrategy.FbeOriginal,
        PipeStrategy.Fbe,
        PipeStrategy.ConnectedCentersDelaunay,
        PipeStrategy.ConnectedCentersDelaunayMst,
        PipeStrategy.ConnectedCentersFlute,
    };

    public static IReadOnlyList<PipeStrategy> DefaultPipeStrategies { get; } = AllPipeStrategies
        .Except(new[] { PipeStrategy.FbeOriginal })
        .Order()
        .ToList();

    public static IReadOnlyList<BeaconStrategy> AllBeaconStrategies { get; } = new[]
    {
        BeaconStrategy.FbeOriginal,
        BeaconStrategy.Fbe,
        BeaconStrategy.Snug,
    };

    public static IReadOnlyList<BeaconStrategy> DefaultBeaconStrategies { get; } = AllBeaconStrategies
        .Except(new[] { BeaconStrategy.FbeOriginal })
        .Order()
        .ToList();

    /// <summary>
    /// Whether or not underground pipes (pipe-to-ground) should be used.
    /// </summary>
    public bool UseUndergroundPipes { get; set; } = true;

    /// <summary>
    /// Whether or not to add beacons around the pumpjacks.
    /// </summary>
    public bool AddBeacons { get; set; } = true;

    /// <summary>
    /// Whether or not to use the pipe optimizer after each pipe strategy is executed. If set to true, the best solution
    /// found will still be used, meaning if the unoptimized pipe plan performs better, it will be preferred over the
    /// corresponding optimized pipe plan.
    /// </summary>
    public bool OptimizePipes { get; set; } = true;

    /// <summary>
    /// Whether or to allow beacon effects to overlap. For Factorio mods like Space Exploration, beacon effects cannot
    /// overlap otherwise pumpjacks will break down with a beacon overload. For vanilla Factorio, this should be true.
    /// </summary>
    public bool OverlapBeacons { get; set; } = true;

    /// <summary>
    /// The pipe planning strategies to attempt.
    /// </summary>
    public List<PipeStrategy> PipeStrategies { get; set; } = new List<PipeStrategy>(DefaultPipeStrategies);

    /// <summary>
    /// The beacon planning strategies to attempt. This will have no affect if <see cref="AddBeacons"/> is false.
    /// </summary>
    public List<BeaconStrategy> BeaconStrategies { get; set; } = new List<BeaconStrategy>(DefaultBeaconStrategies);

    /// <summary>
    /// The internal entity name for the electric pole to use.
    /// </summary>
    public string ElectricPoleEntityName { get; set; } = EntityNames.Vanilla.MediumElectricPole;

    /// <summary>
    /// The supply width (horizontal) for the electric pole. This is the width of the area that the electric pole will
    /// provide power to.
    /// </summary>
    public int ElectricPoleSupplyWidth { get; set; } = 7;

    /// <summary>
    /// The supply height (vertical) for the electric pole. This is the height of the area that the electric pole will
    /// provide power to.
    /// </summary>
    public int ElectricPoleSupplyHeight { get; set; } = 7;

    private const double DefaultElectricPoleWireReach = 9;
    private double _electricPoleWireReach = DefaultElectricPoleWireReach;

    /// <summary>
    /// The wire reach for the electric pole. This is how far apart electric poles can be but still be connected.
    /// </summary>
    public double ElectricPoleWireReach
    {
        get => _electricPoleWireReach;
        set
        {
            _electricPoleWireReach = value;
            ElectricPoleWireReachSquared = value * value;
        }
    }

    internal double ElectricPoleWireReachSquared { get; private set; } = DefaultElectricPoleWireReach * DefaultElectricPoleWireReach;

    /// <summary>
    /// The width of the electric pole entity.
    /// </summary>
    public int ElectricPoleWidth { get; set; } = 1;

    /// <summary>
    /// The height of the electric pole entity.
    /// </summary>
    public int ElectricPoleHeight { get; set; } = 1;

    /// <summary>
    /// The internal entity name for the beacon to use.
    /// </summary>
    public string BeaconEntityName { get; set; } = EntityNames.Vanilla.Beacon;

    /// <summary>
    /// The supply width (horizontal) for the beacon. This is the width of the area that the beacon will provide
    /// module effects to.
    /// </summary>
    public int BeaconSupplyWidth { get; set; } = 9;

    /// <summary>
    /// The supply height (vertical) for the beacon. This is the height of the area that the beacon will provide
    /// module effects to.
    /// </summary>
    public int BeaconSupplyHeight { get; set; } = 9;

    /// <summary>
    /// The width of the beacon entity.
    /// </summary>
    public int BeaconWidth { get; set; } = 3;

    /// <summary>
    /// The height of the beacon entity.
    /// </summary>
    public int BeaconHeight { get; set; } = 3;

    /// <summary>
    /// Whether or not additional validations should be perform on the blueprint correctness. In most cases this should
    /// be false. If you see an invalid blueprint returned, try setting this to true and reporting a bug.
    /// </summary>
    public bool ValidateSolution { get; set; } = false;

    /// <summary>
    /// The modules to add to the pumpjacks. The string key is the internal item name for the module. The value is the
    /// count that kind of module to add to each pumpjack. There can be multiple module types provided.
    /// </summary>
    public Dictionary<string, int> PumpjackModules { get; set; } = new Dictionary<string, int>
    {
        { ItemNames.Vanilla.ProductivityModule3, 2 },
    };

    /// <summary>
    /// The modules to add to the beacons. The string key is the internal item name for the module. The value is the
    /// count that kind of module to add to each beacon. There can be multiple module types provided.
    /// </summary>
    public Dictionary<string, int> BeaconModules { get; set; } = new Dictionary<string, int>
    {
        { ItemNames.Vanilla.SpeedModule3, 2 },
    };
}
