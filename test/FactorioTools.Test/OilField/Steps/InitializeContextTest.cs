using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public class InitializeContextTest : BasePlannerTest
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(5, 3)]
    public void Empty(int width, int height)
    {
        Context context = InitializeContext.GetEmpty(SmallPowerNoBeacon, width, height);
        Assert.Equal(width, context.Grid.Width);
        Assert.Equal(height, context.Grid.Height);
        Assert.Equal(0f, context.DeltaX);
        Assert.Equal(0f, context.DeltaY);
    }

    [Fact]
    public void OnePumpjack_SmallPower_NoBeacon()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f));
        Context context = InitializeContext.Execute(SmallPowerNoBeacon, blueprint, NoAvoid);
        Assert.Equal(7, context.Grid.Width);
        Assert.Equal(7, context.Grid.Height);
        Assert.Equal(-22.5, context.DeltaX);
        Assert.Equal(17.5, context.DeltaY);
        Assert.Equal(new Location(3, 3), Assert.Single(context.Centers.EnumerateItems()));
    }

    [Fact]
    public void OnePumpjack_BigPower_NoBeacon()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f));
        Context context = InitializeContext.Execute(BigPowerNoBeacon, blueprint, NoAvoid);
        Assert.Equal(9, context.Grid.Width);
        Assert.Equal(9, context.Grid.Height);
        Assert.Equal(-21.5, context.DeltaX);
        Assert.Equal(18.5, context.DeltaY);
        Assert.Equal(new Location(4, 4), Assert.Single(context.Centers.EnumerateItems()));
    }

    [Fact]
    public void OnePumpjack_SmallPower_WithBeacon()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f));
        Context context = InitializeContext.Execute(SmallPowerWithBeacon, blueprint, NoAvoid);
        Assert.Equal(17, context.Grid.Width);
        Assert.Equal(17, context.Grid.Height);
        Assert.Equal(-17.5, context.DeltaX);
        Assert.Equal(22.5, context.DeltaY);
        Assert.Equal(new Location(8, 8), Assert.Single(context.Centers.EnumerateItems()));
    }

    [Fact]
    public void OnePumpjack_BigPower_WithBeacon()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f));
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, NoAvoid);
        Assert.Equal(19, context.Grid.Width);
        Assert.Equal(19, context.Grid.Height);
        Assert.Equal(-16.5, context.DeltaX);
        Assert.Equal(23.5, context.DeltaY);
        Assert.Equal(new Location(9, 9), Assert.Single(context.Centers.EnumerateItems()));
    }

    [Fact]
    public void OnePumpjack_SmallPower_WithNonStandardBeacon()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f));
        Context context = InitializeContext.Execute(SmallPowerWithNonStandardBeacon, blueprint, NoAvoid);
        Assert.Equal(11, context.Grid.Width);
        Assert.Equal(15, context.Grid.Height);
        Assert.Equal(-20.5, context.DeltaX);
        Assert.Equal(21.5, context.DeltaY);
        Assert.Equal(new Location(5, 7), Assert.Single(context.Centers.EnumerateItems()));
    }

    [Fact]
    public void OnePumpjack_BigPower_WithNonStandardBeacon()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f));
        Context context = InitializeContext.Execute(BigPowerWithNonStandardBeacon, blueprint, NoAvoid);
        Assert.Equal(13, context.Grid.Width);
        Assert.Equal(17, context.Grid.Height);
        Assert.Equal(-19.5, context.DeltaX);
        Assert.Equal(22.5, context.DeltaY);
        Assert.Equal(new Location(6, 8), Assert.Single(context.Centers.EnumerateItems()));
    }

    [Fact]
    public void TwoPumpjacks_SmallPower_WithBeacon()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        Context context = InitializeContext.Execute(SmallPowerWithBeacon, blueprint, NoAvoid);
        Assert.Equal(25, context.Grid.Width);
        Assert.Equal(19, context.Grid.Height);
        Assert.Equal(-17.5, context.DeltaX);
        Assert.Equal(22.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(8, 8), context.Centers[0]);
        Assert.Equal(new Location(16, 10), context.Centers[1]);
    }

    [Fact]
    public void TwoPumpjacks_BigPower_WithBeacon()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, NoAvoid);
        Assert.Equal(27, context.Grid.Width);
        Assert.Equal(21, context.Grid.Height);
        Assert.Equal(-16.5, context.DeltaX);
        Assert.Equal(23.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(9, 9), context.Centers[0]);
        Assert.Equal(new Location(17, 11), context.Centers[1]);
    }

    [Fact]
    public void AvoidAtMaxXMaxYBeaconBound()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        var avoid = new[] { new AvoidLocation(39.5f, -6.5f) }.ToTableArray();
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, avoid);
        Assert.Equal(27, context.Grid.Width);
        Assert.Equal(21, context.Grid.Height);
        Assert.Equal(-16.5, context.DeltaX);
        Assert.Equal(23.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(9, 9), context.Centers[0]);
        Assert.Equal(new Location(17, 11), context.Centers[1]);
    }

    [Fact]
    public void AvoidLessThanMinY()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        var avoid = new[] { new AvoidLocation(29.5f, -21.5f) }.ToTableArray();
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, avoid);
        var g = context.Grid.ToString();
        Assert.Equal(27, context.Grid.Width);
        Assert.Equal(22, context.Grid.Height);
        Assert.Equal(-16.5, context.DeltaX);
        Assert.Equal(24.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(9, 10), context.Centers[0]);
        Assert.Equal(new Location(17, 12), context.Centers[1]);
    }

    [Fact]
    public void AvoidGreaterThanMaxY()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        var avoid = new[] { new AvoidLocation(29.5f, -5.5f) }.ToTableArray();
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, avoid);
        Assert.Equal(27, context.Grid.Width);
        Assert.Equal(22, context.Grid.Height);
        Assert.Equal(-16.5, context.DeltaX);
        Assert.Equal(23.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(9, 9), context.Centers[0]);
        Assert.Equal(new Location(17, 11), context.Centers[1]);
    }

    [Fact]
    public void AvoidLessThanMinX()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        var avoid = new[] { new AvoidLocation(18.5f, -13.5f) }.ToTableArray();
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, avoid);
        Assert.Equal(28, context.Grid.Width);
        Assert.Equal(21, context.Grid.Height);
        Assert.Equal(-15.5, context.DeltaX);
        Assert.Equal(23.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(10, 9), context.Centers[0]);
        Assert.Equal(new Location(18, 11), context.Centers[1]);
    }

    [Fact]
    public void AvoidGreaterThanMaxX()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        var avoid = new[] { new AvoidLocation(40.5f, -13.5f) }.ToTableArray();
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, avoid);
        Assert.Equal(28, context.Grid.Width);
        Assert.Equal(21, context.Grid.Height);
        Assert.Equal(-16.5, context.DeltaX);
        Assert.Equal(23.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(9, 9), context.Centers[0]);
        Assert.Equal(new Location(17, 11), context.Centers[1]);
    }

    [Fact]
    public void AvoidGreaterThanAllBeaconBounds()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        var avoid = new[]
        {
            new AvoidLocation(29.5f, -24.5f),
            new AvoidLocation(43.5f, -13.5f),
            new AvoidLocation(29.5f, -2.5f),
            new AvoidLocation(15.5f, -13.5f),
        }.ToTableArray();
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, avoid);
        Assert.Equal(35, context.Grid.Width);
        Assert.Equal(29, context.Grid.Height);
        Assert.Equal(-12.5, context.DeltaX);
        Assert.Equal(27.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(13, 13), context.Centers[0]);
        Assert.Equal(new Location(21, 15), context.Centers[1]);
    }

    [Fact]
    public void LargerMinWithNoAvoid()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, NoAvoid, minWidth: 45, minHeight: 0);
        Assert.Equal(45, context.Grid.Width);
        Assert.Equal(21, context.Grid.Height);
        Assert.Equal(-7.5, context.DeltaX);
        Assert.Equal(23.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(18, 9), context.Centers[0]);
        Assert.Equal(new Location(26, 11), context.Centers[1]);
    }

    [Fact]
    public void LargerMinWidthAndMinHeightWithAvoid()
    {
        var blueprint = BlueprintWithCenters((25.5f, -14.5f), (33.5f, -12.5f));
        var avoid = new[]
        {
            new AvoidLocation(29.5f, -24.5f),
            new AvoidLocation(43.5f, -13.5f),
            new AvoidLocation(29.5f, -2.5f),
            new AvoidLocation(15.5f, -13.5f),
        }.ToTableArray();
        Context context = InitializeContext.Execute(BigPowerWithBeacon, blueprint, avoid, minWidth: 50, minHeight: 40);
        Assert.Equal(50, context.Grid.Width);
        Assert.Equal(40, context.Grid.Height);
        Assert.Equal(-5.5, context.DeltaX);
        Assert.Equal(32.5, context.DeltaY);
        Assert.Equal(2, context.Centers.Count);
        Assert.Equal(new Location(20, 18), context.Centers[0]);
        Assert.Equal(new Location(28, 20), context.Centers[1]);
    }

    public IReadOnlyTableList<AvoidLocation> NoAvoid { get; }
    public OilFieldOptions SmallPowerNoBeacon { get; }
    public OilFieldOptions BigPowerNoBeacon { get; }
    public OilFieldOptions SmallPowerWithBeacon { get; }
    public OilFieldOptions BigPowerWithBeacon { get; set; }
    public OilFieldOptions SmallPowerWithNonStandardBeacon { get; }
    public OilFieldOptions BigPowerWithNonStandardBeacon { get; }

    public InitializeContextTest()
    {
        NoAvoid = TableArray.Empty<AvoidLocation>();
        SmallPowerNoBeacon = OilFieldOptions.ForMediumElectricPole;
        SmallPowerNoBeacon.AddBeacons = false;
        BigPowerNoBeacon = OilFieldOptions.ForSubstation;
        BigPowerNoBeacon.AddBeacons = false;
        SmallPowerWithBeacon = OilFieldOptions.ForMediumElectricPole;
        SmallPowerWithBeacon.AddBeacons = true;
        BigPowerWithBeacon = OilFieldOptions.ForSubstation;
        BigPowerWithBeacon.AddBeacons = true;
        SmallPowerWithNonStandardBeacon = OilFieldOptions.ForMediumElectricPole;
        SmallPowerWithNonStandardBeacon.AddBeacons = true;
        SmallPowerWithNonStandardBeacon.BeaconWidth = 2;
        SmallPowerWithNonStandardBeacon.BeaconHeight = 4;
        SmallPowerWithNonStandardBeacon.BeaconSupplyWidth = 4;
        SmallPowerWithNonStandardBeacon.BeaconSupplyHeight = 6;
        BigPowerWithNonStandardBeacon = OilFieldOptions.ForSubstation;
        BigPowerWithNonStandardBeacon.AddBeacons = true;
        BigPowerWithNonStandardBeacon.BeaconWidth = 2;
        BigPowerWithNonStandardBeacon.BeaconHeight = 4;
        BigPowerWithNonStandardBeacon.BeaconSupplyWidth = 4;
        BigPowerWithNonStandardBeacon.BeaconSupplyHeight = 6;
    }

    private Blueprint BlueprintWithCenters(params (float X, float Y)[] centers)
    {
        Blueprint blueprint = new Blueprint();
        blueprint.Entities = centers
            .Select(p => new Entity { Name = "pumpjack", Position = new Position { X = p.X, Y = p.Y } })
            .ToArray();
        return blueprint;
    }
}
