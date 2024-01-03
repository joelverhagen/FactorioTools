namespace Knapcode.FactorioTools.OilField;

public class OilFieldOptionsTest
{
    [Fact]
    public void AllPipeStrategies()
    {
        Assert.Equal(Enum.GetValues<PipeStrategy>(), OilFieldOptions.AllPipeStrategies);
    }

    [Fact]
    public void AllBeaconStrategies()
    {
        Assert.Equal(Enum.GetValues<BeaconStrategy>(), OilFieldOptions.AllBeaconStrategies);
    }
}
