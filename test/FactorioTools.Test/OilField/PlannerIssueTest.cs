using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public class PlannerIssueTest : BasePlannerTest
{
    [Fact]
    public async Task CanPlanCollinearConnectedCenters()
    {
        // Arrange
        var options = OilFieldOptions.ForSmallElectricPole;
        options.AddBeacons = false;
        options.ValidateSolution = true;
        var blueprint = new Blueprint
        {
            Entities = new[]
            {
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = -23.5f, Y = -37.5f },
                },
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = -21.5f, Y = -34.5f },
                },
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = -19.5f, Y = -31.5f },
                },
            }
        };

        // Act
        var result = Planner.Execute(options, blueprint);

        // Assert
#if USE_VERIFY
        await Verify(GetGridString(result));
#else
        await Task.Yield();
#endif
    }

    [Fact]
    public async Task AllowsManyTurnsAroundAvoidLocations()
    {
        // Arrange
        var options = OilFieldOptions.ForSmallElectricPole;
        options.ValidateSolution = true;
        var blueprint = new Blueprint
        {
            Entities = new[]
            {
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = 35.5f, Y = -21.5f },
                },
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = 35.5f, Y = -13.5f },
                },
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = 40.5f, Y = -33.5f },
                },
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = 48.5f, Y = -38.5f },
                },
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = 51.5f, Y = -27.5f },
                },

            }
        };
        var avoid = new[]
        {
              new AvoidLocation(33.5f, -10.5f),
              new AvoidLocation(34.5f, -10.5f),
              new AvoidLocation(35.5f, -10.5f),
              new AvoidLocation(36.5f, -11.5f),
              new AvoidLocation(36.5f, -10.5f),
              new AvoidLocation(37.5f, -11.5f),
              new AvoidLocation(37.5f, -10.5f),
              new AvoidLocation(38.5f, -17.5f),
              new AvoidLocation(38.5f, -16.5f),
              new AvoidLocation(38.5f, -15.5f),
              new AvoidLocation(38.5f, -14.5f),
              new AvoidLocation(38.5f, -11.5f),
              new AvoidLocation(38.5f, -10.5f),
              new AvoidLocation(39.5f, -18.5f),
              new AvoidLocation(39.5f, -17.5f),
              new AvoidLocation(39.5f, -16.5f),
              new AvoidLocation(39.5f, -15.5f),
              new AvoidLocation(39.5f, -14.5f),
              new AvoidLocation(39.5f, -13.5f),
              new AvoidLocation(39.5f, -10.5f),
              new AvoidLocation(40.5f, -20.5f),
              new AvoidLocation(40.5f, -19.5f),
              new AvoidLocation(40.5f, -18.5f),
              new AvoidLocation(40.5f, -17.5f),
              new AvoidLocation(40.5f, -16.5f),
              new AvoidLocation(40.5f, -15.5f),
              new AvoidLocation(40.5f, -14.5f),
              new AvoidLocation(40.5f, -13.5f),
              new AvoidLocation(40.5f, -12.5f),
              new AvoidLocation(40.5f, -11.5f),
              new AvoidLocation(40.5f, -10.5f),
              new AvoidLocation(41.5f, -26.5f),
              new AvoidLocation(41.5f, -25.5f),
              new AvoidLocation(41.5f, -24.5f),
              new AvoidLocation(41.5f, -23.5f),
              new AvoidLocation(41.5f, -22.5f),
              new AvoidLocation(41.5f, -21.5f),
              new AvoidLocation(41.5f, -20.5f),
              new AvoidLocation(41.5f, -19.5f),
              new AvoidLocation(41.5f, -18.5f),
              new AvoidLocation(41.5f, -17.5f),
              new AvoidLocation(41.5f, -16.5f),
              new AvoidLocation(41.5f, -15.5f),
              new AvoidLocation(41.5f, -14.5f),
              new AvoidLocation(41.5f, -13.5f),
              new AvoidLocation(41.5f, -12.5f),
              new AvoidLocation(41.5f, -11.5f),
              new AvoidLocation(41.5f, -10.5f),
              new AvoidLocation(42.5f, -39.5f),
              new AvoidLocation(42.5f, -38.5f),
              new AvoidLocation(42.5f, -37.5f),
              new AvoidLocation(42.5f, -27.5f),
              new AvoidLocation(42.5f, -26.5f),
              new AvoidLocation(42.5f, -25.5f),
              new AvoidLocation(42.5f, -24.5f),
              new AvoidLocation(42.5f, -23.5f),
              new AvoidLocation(42.5f, -22.5f),
              new AvoidLocation(42.5f, -21.5f),
              new AvoidLocation(42.5f, -20.5f),
              new AvoidLocation(42.5f, -19.5f),
              new AvoidLocation(42.5f, -18.5f),
              new AvoidLocation(42.5f, -17.5f),
              new AvoidLocation(42.5f, -16.5f),
              new AvoidLocation(42.5f, -15.5f),
              new AvoidLocation(42.5f, -14.5f),
              new AvoidLocation(42.5f, -13.5f),
              new AvoidLocation(42.5f, -11.5f),
              new AvoidLocation(42.5f, -10.5f),
              new AvoidLocation(43.5f, -41.5f),
              new AvoidLocation(43.5f, -40.5f),
              new AvoidLocation(43.5f, -39.5f),
              new AvoidLocation(43.5f, -38.5f),
              new AvoidLocation(43.5f, -37.5f),
              new AvoidLocation(43.5f, -36.5f),
              new AvoidLocation(43.5f, -28.5f),
              new AvoidLocation(43.5f, -27.5f),
              new AvoidLocation(43.5f, -26.5f),
              new AvoidLocation(43.5f, -25.5f),
              new AvoidLocation(43.5f, -24.5f),
              new AvoidLocation(43.5f, -23.5f),
              new AvoidLocation(43.5f, -22.5f),
              new AvoidLocation(43.5f, -21.5f),
              new AvoidLocation(43.5f, -20.5f),
              new AvoidLocation(43.5f, -19.5f),
              new AvoidLocation(43.5f, -18.5f),
              new AvoidLocation(43.5f, -17.5f),
              new AvoidLocation(43.5f, -16.5f),
              new AvoidLocation(43.5f, -15.5f),
              new AvoidLocation(43.5f, -14.5f),
              new AvoidLocation(43.5f, -13.5f),
              new AvoidLocation(43.5f, -12.5f),
              new AvoidLocation(43.5f, -11.5f),
              new AvoidLocation(43.5f, -10.5f),
              new AvoidLocation(44.5f, -41.5f),
              new AvoidLocation(44.5f, -40.5f),
              new AvoidLocation(44.5f, -39.5f),
              new AvoidLocation(44.5f, -38.5f),
              new AvoidLocation(44.5f, -37.5f),
              new AvoidLocation(44.5f, -36.5f),
              new AvoidLocation(44.5f, -30.5f),
              new AvoidLocation(44.5f, -29.5f),
              new AvoidLocation(44.5f, -28.5f),
              new AvoidLocation(44.5f, -27.5f),
              new AvoidLocation(44.5f, -26.5f),
              new AvoidLocation(44.5f, -25.5f),
              new AvoidLocation(44.5f, -24.5f),
              new AvoidLocation(44.5f, -23.5f),
              new AvoidLocation(44.5f, -22.5f),
              new AvoidLocation(44.5f, -21.5f),
              new AvoidLocation(44.5f, -20.5f),
              new AvoidLocation(44.5f, -19.5f),
              new AvoidLocation(44.5f, -18.5f),
              new AvoidLocation(44.5f, -17.5f),
              new AvoidLocation(44.5f, -16.5f),
              new AvoidLocation(44.5f, -15.5f),
              new AvoidLocation(44.5f, -14.5f),
              new AvoidLocation(44.5f, -13.5f),
              new AvoidLocation(44.5f, -12.5f),
              new AvoidLocation(44.5f, -11.5f),
              new AvoidLocation(44.5f, -10.5f),
              new AvoidLocation(45.5f, -41.5f),
              new AvoidLocation(45.5f, -40.5f),
              new AvoidLocation(45.5f, -39.5f),
              new AvoidLocation(45.5f, -38.5f),
              new AvoidLocation(45.5f, -37.5f),
              new AvoidLocation(45.5f, -36.5f),
              new AvoidLocation(45.5f, -33.5f),
              new AvoidLocation(45.5f, -32.5f),
              new AvoidLocation(45.5f, -31.5f),
              new AvoidLocation(45.5f, -30.5f),
              new AvoidLocation(45.5f, -29.5f),
              new AvoidLocation(45.5f, -28.5f),
              new AvoidLocation(45.5f, -27.5f),
              new AvoidLocation(45.5f, -26.5f),
              new AvoidLocation(45.5f, -25.5f),
              new AvoidLocation(45.5f, -24.5f),
              new AvoidLocation(45.5f, -23.5f),
              new AvoidLocation(45.5f, -22.5f),
              new AvoidLocation(45.5f, -21.5f),
              new AvoidLocation(45.5f, -20.5f),
              new AvoidLocation(45.5f, -19.5f),
              new AvoidLocation(45.5f, -18.5f),
              new AvoidLocation(45.5f, -17.5f),
              new AvoidLocation(45.5f, -16.5f),
              new AvoidLocation(45.5f, -15.5f),
              new AvoidLocation(45.5f, -14.5f),
              new AvoidLocation(45.5f, -13.5f),
              new AvoidLocation(45.5f, -12.5f),
              new AvoidLocation(45.5f, -11.5f),
              new AvoidLocation(45.5f, -10.5f),
              new AvoidLocation(46.5f, -33.5f),
              new AvoidLocation(46.5f, -32.5f),
              new AvoidLocation(46.5f, -31.5f),
              new AvoidLocation(46.5f, -30.5f),
              new AvoidLocation(46.5f, -29.5f),
              new AvoidLocation(46.5f, -28.5f),
              new AvoidLocation(46.5f, -27.5f),
              new AvoidLocation(46.5f, -26.5f),
              new AvoidLocation(46.5f, -25.5f),
              new AvoidLocation(46.5f, -24.5f),
              new AvoidLocation(46.5f, -23.5f),
              new AvoidLocation(46.5f, -22.5f),
              new AvoidLocation(46.5f, -21.5f),
              new AvoidLocation(46.5f, -20.5f),
              new AvoidLocation(46.5f, -19.5f),
              new AvoidLocation(46.5f, -18.5f),
              new AvoidLocation(46.5f, -17.5f),
              new AvoidLocation(46.5f, -16.5f),
              new AvoidLocation(46.5f, -15.5f),
              new AvoidLocation(46.5f, -14.5f),
              new AvoidLocation(46.5f, -13.5f),
              new AvoidLocation(46.5f, -12.5f),
              new AvoidLocation(46.5f, -11.5f),
              new AvoidLocation(46.5f, -10.5f),
              new AvoidLocation(47.5f, -31.5f),
              new AvoidLocation(47.5f, -30.5f),
              new AvoidLocation(47.5f, -29.5f),
              new AvoidLocation(47.5f, -25.5f),
              new AvoidLocation(47.5f, -24.5f),
              new AvoidLocation(47.5f, -23.5f),
              new AvoidLocation(47.5f, -22.5f),
              new AvoidLocation(47.5f, -21.5f),
              new AvoidLocation(47.5f, -20.5f),
              new AvoidLocation(47.5f, -19.5f),
              new AvoidLocation(47.5f, -18.5f),
              new AvoidLocation(47.5f, -17.5f),
              new AvoidLocation(47.5f, -16.5f),
              new AvoidLocation(47.5f, -15.5f),
              new AvoidLocation(47.5f, -14.5f),
              new AvoidLocation(47.5f, -13.5f),
              new AvoidLocation(47.5f, -12.5f),
              new AvoidLocation(47.5f, -11.5f),
              new AvoidLocation(47.5f, -10.5f),
              new AvoidLocation(48.5f, -24.5f),
              new AvoidLocation(48.5f, -23.5f),
              new AvoidLocation(48.5f, -22.5f),
              new AvoidLocation(48.5f, -21.5f),
              new AvoidLocation(48.5f, -20.5f),
              new AvoidLocation(48.5f, -19.5f),
              new AvoidLocation(48.5f, -18.5f),
              new AvoidLocation(48.5f, -17.5f),
              new AvoidLocation(48.5f, -16.5f),
              new AvoidLocation(48.5f, -15.5f),
              new AvoidLocation(48.5f, -14.5f),
              new AvoidLocation(48.5f, -13.5f),
              new AvoidLocation(48.5f, -12.5f),
              new AvoidLocation(48.5f, -11.5f),
              new AvoidLocation(48.5f, -10.5f),
              new AvoidLocation(49.5f, -24.5f),
              new AvoidLocation(49.5f, -23.5f),
              new AvoidLocation(49.5f, -22.5f),
              new AvoidLocation(49.5f, -21.5f),
              new AvoidLocation(49.5f, -20.5f),
              new AvoidLocation(49.5f, -19.5f),
              new AvoidLocation(49.5f, -18.5f),
              new AvoidLocation(49.5f, -17.5f),
              new AvoidLocation(49.5f, -16.5f),
              new AvoidLocation(49.5f, -15.5f),
              new AvoidLocation(49.5f, -14.5f),
              new AvoidLocation(49.5f, -13.5f),
              new AvoidLocation(49.5f, -12.5f),
              new AvoidLocation(49.5f, -11.5f),
              new AvoidLocation(49.5f, -10.5f),
              new AvoidLocation(50.5f, -24.5f),
              new AvoidLocation(50.5f, -23.5f),
              new AvoidLocation(50.5f, -22.5f),
              new AvoidLocation(50.5f, -21.5f),
              new AvoidLocation(50.5f, -20.5f),
              new AvoidLocation(50.5f, -19.5f),
              new AvoidLocation(50.5f, -18.5f),
              new AvoidLocation(50.5f, -17.5f),
              new AvoidLocation(50.5f, -16.5f),
              new AvoidLocation(50.5f, -15.5f),
              new AvoidLocation(50.5f, -14.5f),
              new AvoidLocation(50.5f, -13.5f),
              new AvoidLocation(50.5f, -12.5f),
              new AvoidLocation(50.5f, -11.5f),
              new AvoidLocation(50.5f, -10.5f),
              new AvoidLocation(51.5f, -24.5f),
              new AvoidLocation(51.5f, -23.5f),
              new AvoidLocation(51.5f, -22.5f),
              new AvoidLocation(51.5f, -21.5f),
              new AvoidLocation(51.5f, -20.5f),
              new AvoidLocation(51.5f, -19.5f),
              new AvoidLocation(51.5f, -18.5f),
              new AvoidLocation(51.5f, -17.5f),
              new AvoidLocation(51.5f, -14.5f),
              new AvoidLocation(51.5f, -13.5f),
              new AvoidLocation(51.5f, -12.5f),
              new AvoidLocation(51.5f, -11.5f),
              new AvoidLocation(51.5f, -10.5f),
              new AvoidLocation(52.5f, -24.5f),
              new AvoidLocation(52.5f, -23.5f),
              new AvoidLocation(52.5f, -22.5f),
              new AvoidLocation(52.5f, -21.5f),
              new AvoidLocation(52.5f, -20.5f),
              new AvoidLocation(52.5f, -19.5f),
              new AvoidLocation(52.5f, -18.5f),
              new AvoidLocation(53.5f, -24.5f),
              new AvoidLocation(53.5f, -23.5f),
              new AvoidLocation(53.5f, -22.5f),
              new AvoidLocation(53.5f, -21.5f),
              new AvoidLocation(53.5f, -20.5f),
              new AvoidLocation(53.5f, -19.5f),
              new AvoidLocation(53.5f, -18.5f),
              new AvoidLocation(53.5f, -17.5f),
              new AvoidLocation(54.5f, -29.5f),
              new AvoidLocation(54.5f, -28.5f),
              new AvoidLocation(54.5f, -27.5f),
              new AvoidLocation(54.5f, -25.5f),
              new AvoidLocation(54.5f, -24.5f),
              new AvoidLocation(54.5f, -23.5f),
              new AvoidLocation(54.5f, -22.5f),
              new AvoidLocation(54.5f, -21.5f),
              new AvoidLocation(54.5f, -20.5f),
              new AvoidLocation(54.5f, -19.5f),
              new AvoidLocation(54.5f, -18.5f),
              new AvoidLocation(54.5f, -17.5f),
        }.ToTableArray();

        // Act
        var result = Planner.Execute(options, blueprint, avoid);

        // Assert
#if USE_VERIFY
        await Verify(GetGridString(result));
#else
        await Task.Yield();
#endif
    }

    [Fact]
    public async Task CanPlanSinglePumpjackSurrounded()
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        var blueprint = new Blueprint
        {
            Entities = new[]
            {
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = 43.5f, Y = -3.5f },
                }
            }
        };
        var avoid = new AvoidLocation[]
        {
            new AvoidLocation(40.5f, -2.5f),
            new AvoidLocation(40.5f, -3.5f),
            new AvoidLocation(40.5f, -4.5f),
            new AvoidLocation(40.5f, -5.5f),
            new AvoidLocation(40.5f, -6.5f),
            new AvoidLocation(41.5f, -2.5f),
            new AvoidLocation(41.5f, -3.5f),
            new AvoidLocation(41.5f, -4.5f),
            new AvoidLocation(41.5f, -5.5f),
            new AvoidLocation(41.5f, -6.5f),
            new AvoidLocation(42.5f, -6.5f),
            new AvoidLocation(43.5f, -6.5f),
            new AvoidLocation(44.5f, -5.5f),
            new AvoidLocation(44.5f, -6.5f),
            new AvoidLocation(45.5f, -4.5f),
            new AvoidLocation(45.5f, -5.5f),
            new AvoidLocation(45.5f, -6.5f),
            new AvoidLocation(46.5f, -0.5f),
            new AvoidLocation(46.5f, -3.5f),
            new AvoidLocation(46.5f, -4.5f),
            new AvoidLocation(46.5f, -5.5f),
            new AvoidLocation(46.5f, -6.5f),
        }.ToTableArray();

        // Act
        var result = Planner.Execute(options, blueprint, avoid);

        // Assert
#if USE_VERIFY
        await Verify(GetGridString(result));
#else
        await Task.Yield();
#endif
    }

    [Fact]
    public async Task CanPlanTwoPumpjacksWithLotsOfAvoids()
    {
        // Arrange
        var options = OilFieldOptions.ForSmallElectricPole;
        options.AddBeacons = false;
        options.ValidateSolution = true;
        var blueprint = new Blueprint
        {
            Entities = new[]
            {
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = 35.5f, Y = -13.5f },
                },
                new Entity
                {
                    Name = EntityNames.Vanilla.Pumpjack,
                    Position = new Position { X = 43.5f, Y = -3.5f },
                }
            }
        };
        var avoid = new AvoidLocation[]
        {
            new AvoidLocation(32.5f, -9.5f),
            new AvoidLocation(32.5f, -8.5f),
            new AvoidLocation(33.5f, -10.5f),
            new AvoidLocation(33.5f, -9.5f),
            new AvoidLocation(33.5f, -8.5f),
            new AvoidLocation(33.5f, -7.5f),
            new AvoidLocation(33.5f, -6.5f),
            new AvoidLocation(34.5f, -10.5f),
            new AvoidLocation(34.5f, -9.5f),
            new AvoidLocation(34.5f, -8.5f),
            new AvoidLocation(34.5f, -7.5f),
            new AvoidLocation(34.5f, -6.5f),
            new AvoidLocation(34.5f, -5.5f),
            new AvoidLocation(34.5f, -4.5f),
            new AvoidLocation(34.5f, -3.5f),
            new AvoidLocation(35.5f, -10.5f),
            new AvoidLocation(35.5f, -9.5f),
            new AvoidLocation(35.5f, -8.5f),
            new AvoidLocation(35.5f, -7.5f),
            new AvoidLocation(35.5f, -6.5f),
            new AvoidLocation(35.5f, -5.5f),
            new AvoidLocation(35.5f, -4.5f),
            new AvoidLocation(35.5f, -3.5f),
            new AvoidLocation(35.5f, -2.5f),
            new AvoidLocation(36.5f, -11.5f),
            new AvoidLocation(36.5f, -10.5f),
            new AvoidLocation(36.5f, -9.5f),
            new AvoidLocation(36.5f, -8.5f),
            new AvoidLocation(36.5f, -7.5f),
            new AvoidLocation(36.5f, -6.5f),
            new AvoidLocation(36.5f, -5.5f),
            new AvoidLocation(36.5f, -4.5f),
            new AvoidLocation(36.5f, -3.5f),
            new AvoidLocation(36.5f, -2.5f),
            new AvoidLocation(37.5f, -11.5f),
            new AvoidLocation(37.5f, -10.5f),
            new AvoidLocation(37.5f, -9.5f),
            new AvoidLocation(37.5f, -8.5f),
            new AvoidLocation(37.5f, -7.5f),
            new AvoidLocation(37.5f, -6.5f),
            new AvoidLocation(37.5f, -5.5f),
            new AvoidLocation(37.5f, -4.5f),
            new AvoidLocation(37.5f, -3.5f),
            new AvoidLocation(37.5f, -1.5f),
            new AvoidLocation(37.5f, -0.5f),
            new AvoidLocation(38.5f, -16.5f),
            new AvoidLocation(38.5f, -15.5f),
            new AvoidLocation(38.5f, -14.5f),
            new AvoidLocation(38.5f, -11.5f),
            new AvoidLocation(38.5f, -10.5f),
            new AvoidLocation(38.5f, -9.5f),
            new AvoidLocation(38.5f, -8.5f),
            new AvoidLocation(38.5f, -7.5f),
            new AvoidLocation(38.5f, -6.5f),
            new AvoidLocation(38.5f, -5.5f),
            new AvoidLocation(38.5f, -4.5f),
            new AvoidLocation(38.5f, -3.5f),
            new AvoidLocation(38.5f, -1.5f),
            new AvoidLocation(38.5f, -0.5f),
            new AvoidLocation(39.5f, -16.5f),
            new AvoidLocation(39.5f, -15.5f),
            new AvoidLocation(39.5f, -14.5f),
            new AvoidLocation(39.5f, -13.5f),
            new AvoidLocation(39.5f, -10.5f),
            new AvoidLocation(39.5f, -9.5f),
            new AvoidLocation(39.5f, -8.5f),
            new AvoidLocation(39.5f, -7.5f),
            new AvoidLocation(39.5f, -6.5f),
            new AvoidLocation(39.5f, -5.5f),
            new AvoidLocation(39.5f, -4.5f),
            new AvoidLocation(39.5f, -3.5f),
            new AvoidLocation(39.5f, -1.5f),
            new AvoidLocation(39.5f, -0.5f),
            new AvoidLocation(40.5f, -16.5f),
            new AvoidLocation(40.5f, -15.5f),
            new AvoidLocation(40.5f, -14.5f),
            new AvoidLocation(40.5f, -13.5f),
            new AvoidLocation(40.5f, -12.5f),
            new AvoidLocation(40.5f, -11.5f),
            new AvoidLocation(40.5f, -10.5f),
            new AvoidLocation(40.5f, -9.5f),
            new AvoidLocation(40.5f, -8.5f),
            new AvoidLocation(40.5f, -7.5f),
            new AvoidLocation(40.5f, -6.5f),
            new AvoidLocation(40.5f, -5.5f),
            new AvoidLocation(40.5f, -4.5f),
            new AvoidLocation(40.5f, -3.5f),
            new AvoidLocation(40.5f, -2.5f),
            new AvoidLocation(41.5f, -16.5f),
            new AvoidLocation(41.5f, -15.5f),
            new AvoidLocation(41.5f, -14.5f),
            new AvoidLocation(41.5f, -13.5f),
            new AvoidLocation(41.5f, -12.5f),
            new AvoidLocation(41.5f, -11.5f),
            new AvoidLocation(41.5f, -10.5f),
            new AvoidLocation(41.5f, -9.5f),
            new AvoidLocation(41.5f, -8.5f),
            new AvoidLocation(41.5f, -7.5f),
            new AvoidLocation(41.5f, -6.5f),
            new AvoidLocation(41.5f, -5.5f),
            new AvoidLocation(41.5f, -4.5f),
            new AvoidLocation(41.5f, -3.5f),
            new AvoidLocation(41.5f, -2.5f),
            new AvoidLocation(42.5f, -16.5f),
            new AvoidLocation(42.5f, -15.5f),
            new AvoidLocation(42.5f, -14.5f),
            new AvoidLocation(42.5f, -13.5f),
            new AvoidLocation(42.5f, -11.5f),
            new AvoidLocation(42.5f, -10.5f),
            new AvoidLocation(42.5f, -9.5f),
            new AvoidLocation(42.5f, -8.5f),
            new AvoidLocation(42.5f, -7.5f),
            new AvoidLocation(42.5f, -6.5f),
            new AvoidLocation(43.5f, -16.5f),
            new AvoidLocation(43.5f, -15.5f),
            new AvoidLocation(43.5f, -14.5f),
            new AvoidLocation(43.5f, -13.5f),
            new AvoidLocation(43.5f, -12.5f),
            new AvoidLocation(43.5f, -11.5f),
            new AvoidLocation(43.5f, -10.5f),
            new AvoidLocation(43.5f, -9.5f),
            new AvoidLocation(43.5f, -8.5f),
            new AvoidLocation(43.5f, -7.5f),
            new AvoidLocation(43.5f, -6.5f),
            new AvoidLocation(44.5f, -16.5f),
            new AvoidLocation(44.5f, -15.5f),
            new AvoidLocation(44.5f, -14.5f),
            new AvoidLocation(44.5f, -13.5f),
            new AvoidLocation(44.5f, -12.5f),
            new AvoidLocation(44.5f, -11.5f),
            new AvoidLocation(44.5f, -10.5f),
            new AvoidLocation(44.5f, -9.5f),
            new AvoidLocation(44.5f, -8.5f),
            new AvoidLocation(44.5f, -7.5f),
            new AvoidLocation(44.5f, -6.5f),
            new AvoidLocation(44.5f, -5.5f),
            new AvoidLocation(45.5f, -16.5f),
            new AvoidLocation(45.5f, -15.5f),
            new AvoidLocation(45.5f, -14.5f),
            new AvoidLocation(45.5f, -13.5f),
            new AvoidLocation(45.5f, -12.5f),
            new AvoidLocation(45.5f, -11.5f),
            new AvoidLocation(45.5f, -10.5f),
            new AvoidLocation(45.5f, -9.5f),
            new AvoidLocation(45.5f, -8.5f),
            new AvoidLocation(45.5f, -7.5f),
            new AvoidLocation(45.5f, -6.5f),
            new AvoidLocation(45.5f, -5.5f),
            new AvoidLocation(45.5f, -4.5f),
            new AvoidLocation(46.5f, -16.5f),
            new AvoidLocation(46.5f, -15.5f),
            new AvoidLocation(46.5f, -14.5f),
            new AvoidLocation(46.5f, -13.5f),
            new AvoidLocation(46.5f, -12.5f),
            new AvoidLocation(46.5f, -11.5f),
            new AvoidLocation(46.5f, -10.5f),
            new AvoidLocation(46.5f, -9.5f),
            new AvoidLocation(46.5f, -8.5f),
            new AvoidLocation(46.5f, -7.5f),
            new AvoidLocation(46.5f, -6.5f),
            new AvoidLocation(46.5f, -5.5f),
            new AvoidLocation(46.5f, -4.5f),
            new AvoidLocation(46.5f, -3.5f),
            new AvoidLocation(46.5f, -0.5f),
        }.ToTableArray();

        // Act
        var result = Planner.Execute(options, blueprint, avoid);

        // Assert
#if USE_VERIFY
        await Verify(GetGridString(result));
#else
        await Task.Yield();
#endif
    }

    /// <summary>
    /// This blueprint found a bug in the SortedBatches class.
    /// </summary>
    [Fact]
    public void PlansElectricPoles()
    {
        // Arrange
        var options = OilFieldOptions.ForSubstation;
        options.ValidateSolution = true;
        var blueprintString = "0eJyM00sOgyAQANC7zJqFgp+WqzRN42fS0CoSwabGePeidNHEJsySYXjM8Fmg7iY0o9IO5AKqGbQFeVnAqruuui2mqx5Bgpl686iaJzBws9kiymEPKwOlW3yDTNcrA9ROOYXB2AfzTU99jaNPYH8sM1i/YNDbTh4RGYPZpwrvtmrEJswlKztwnMDxPHBZnBMELi0CV8S5jFJdErgyzuWU6sTO8TTOFQQuS8hcSWk2nB0n3OyJ0my4WZ7HuTO9WXGozr/p/Z3Ln4/C4IWj/SasHwAAAP//AwCxgxNI";
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (context, _) = Planner.Execute(options, blueprint);

        // Assert
        Assert.NotEmpty(context.Grid.GetEntities().OfType<ElectricPoleCenter>());
        Assert.NotEmpty(context.Grid.GetEntities().OfType<ElectricPoleSide>());
    }

    /// <summary>
    /// https://github.com/teoxoy/factorio-blueprint-editor/issues/253
    /// </summary>
    [Fact]
    public void FbeOriginalFallsBackToFbeWhenLeftoverPumpsCannotConnect()
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        options.PipeStrategies = TableArray.New(PipeStrategy.FbeOriginal);
        var blueprintString = "0eJyM1ctuhSAQBuB3mTULGby/StM0Hg9paI9ovDQ1xncviou2h4R/KeLH4PCHjW6PRQ+jsTPVG5m2txPVLxtN5t02j2PMNp2mmoalGz6a9pMEzetwjJhZd7QLMvauv6mW+6sgbWczG+2N82F9s0t306ObIALW0E/ug94eKzlEsaDVTVXOvZtRt/5dsosnjgFO5p7L45wCOC48V8a5FOFSz1VxLkM2W50cJ3EuR1rhq2MZ5wpks76zzH85DnAlwlVBLlRdhfy70nMqXp1MEC/zXhYvTyK5uI4K5CHB4MR7QDAkkgwlvVcAHhKNY9HDK4F+INngywOiJpFwcIl7SDqu86KAsEkoHn6/6l880pCH5IPzoBfqByP5uLz0Kb7uDjnvlfrXxSToS4/TNWH/AQAA//8DANANMhY=";

        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (_, summary) = Planner.Execute(options, blueprint);

        // Assert
        var plan = Assert.Single(summary.SelectedPlans.EnumerateItems());
        Assert.Equal(PipeStrategy.Fbe, plan.PipeStrategy);
    }

    /// <summary>
    /// https://github.com/teoxoy/factorio-blueprint-editor/issues/254
    /// </summary>
    [Fact]
    public void FbeOriginalFallsBackToFbeWhenAloneGroupRemains()
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        options.PipeStrategies = TableArray.New(PipeStrategy.FbeOriginal);
        var blueprintString = "0eJyUlttuwyAMht/F11wEDDm9yjRNPaCJraFRkk6rqrz70ppKU8eUv5dNyReD/dlcaHs4+X4IcaL2QmF3jCO1Lxcaw3vcHK7P4qbz1FJ/6vqPze6TFE3n/vokTL6jWVGIe/9NrZ5fFfk4hSl4Ydx+nN/iqdv6YVmgMqz+OC4vHOP1SwuEa0XnZSkv3H0Y/E7+K2b1B2cAnCsE59ZxjERXCq5ex1kAV6bomnWcA3BW33CmWMeVyNkZwel1XIXgKsGZdVyNpMIJDiiUBjm7CsbpAgmvEZ4FeIgXmoUHFLJGxDApvhLgIWZYl+WZHA9Rg9N+KyA+yI0C5z0jB8JD7OBUf0Bn0YgeNp1fA+QD8qPO8rJ9+Qk/GGguBpobUn8MdBeD+JH6wSMvd34GmhxSf4zMNcQPa3Ae4od7Ij7Ej3t+H/qfzfEQP+77LQEe5IfL8rL7RfxgKzygHzDkR8oH0A8Y8eNez4C/jPiR+rMF/GXID6kXC9QfQ/ND8muR/ULzw/7DW+68t3tw++sirejLD2NaMP8AAAD//wMA5MG5Zw==";

        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (_, summary) = Planner.Execute(options, blueprint);

        // Assert
        var plan = Assert.Single(summary.SelectedPlans.EnumerateItems());
        Assert.Equal(PipeStrategy.Fbe, plan.PipeStrategy);
    }
}
