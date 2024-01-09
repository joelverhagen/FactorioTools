namespace Knapcode.FactorioTools.OilField;

public class OilFieldOptionsTest
{
    [Fact]
    public void AllPipeStrategies()
    {
        Assert.Equal(Enum.GetValues<PipeStrategy>(), OilFieldOptions.AllPipeStrategies);
    }

    [Fact]
    public void DefaultPipeStrategies()
    {
        Assert.Equal(Enum.GetValues<PipeStrategy>().Except(new[] { PipeStrategy.FbeOriginal }).Order(), OilFieldOptions.DefaultPipeStrategies);
    }

    [Fact]
    public void AllBeaconStrategies()
    {
        Assert.Equal(Enum.GetValues<BeaconStrategy>(), OilFieldOptions.AllBeaconStrategies);
    }

    [Fact]
    public void DefaultBeaconStrategies()
    {
        Assert.Equal(Enum.GetValues<BeaconStrategy>().Except(new[] { BeaconStrategy.FbeOriginal }).Order(), OilFieldOptions.DefaultBeaconStrategies);
    }
}
