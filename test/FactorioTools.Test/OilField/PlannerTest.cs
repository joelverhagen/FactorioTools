using Knapcode.FactorioTools.Data;

namespace Knapcode.FactorioTools.OilField;

public class PlannerTest : BasePlannerTest
{
    public static IReadOnlyList<string> BlueprintsWithIsolatedAreas = new[]
    {
        "0eJyU1ctugzAQheF3mbUXeGwg8CpVVRFiVW6Dg7hURYh3L2BX6sUSh2WI82Xi8OOZrvfRtJ11A5Uz2frheiqfZurtq6vu2zVXNYZKasemfavqdxI0TO12xQ6moUWQdTfzSaVcngUZN9jBGm/sL6YXNzZX060LRMRqH/36gYfbvmlFWAua1qVqdW+2M7V/L1nEP44BTjHMKYTLYU4jXOK57DfHES49MR3AZQAnA5cf/9gc4HTmucsxd0FulDBdccwVyN4pmJMJsnne4wTwTnTBEvCgMBLcO1EGM+AhaXCYDyhNIm2E24U14CFxfO+fPm5NInUojc+H5KHj80U9pA+Nz8dIHxz6AB4ujPShiqgX+z8Y6YPT3VNAHwz1wbgHHR3+aarSv956Bu/ncvnjYBf0Ybo+LFi+AAAA//8DAEf3mj4=",
        "0eJyM1ctuhCAUBuB3OWsWcvD+Kk0zcRzS0I6M8dLUGN+9KCzaGRL/pYifh8sPK13vs+4HYyeqVzLtw45Uv600mg/b3Pc223Saaurnrv9s2i8SNC393mIm3dEmyNib/qFabu+CtJ3MZLQ3joflYufuqgfXQUSs/jG6Dx52/5NDFAtaXFfl3JsZdOvfJZt44RjgZO65/JxTAMeF58pzLkW41HPVOZchg60OjpNzLkeWwlfH8pwrkMH6lWX+z6URrkS4LMrFqqsQroI5mSBrUXrvaR9zzENyIcNwM6A+JBhh6z170fqgZCTeA4ImoWgUuIdkQ0nvFYCHhGOflN0rgfmD0hE84CiQUDxK3EPyEfafAg4DRvIRxquA04CRfHAe9WLrwVA+vJe+nAfujjvuvfrPxSnoWw9j6LD9AgAA//8DAGH8ZjU=",
        "0eJyUl11vgjAUQP9Ln3mgvS0If2VZFj+ahW0iEVxmDP99aGsyJwmnjyIe29577r29qM3XyXfHph1UfVHN9tD2qn65qL55b9df12fteu9VrbrTvvtYbz9VpoZzd33SDH6vxkw17c7/qFqPr5ny7dAMjQ+M24fzW3vab/xxeiGbYXWHfvrBob3+0wSRIlPn6VWZuLvm6Lfhu3zMnnCG40y+jBOC0wGnl3EW4EzEmWWcIzgbcODsCoIrA84u48qEUADcKuHs3CPOzuCqhMi65dXpHPB03G0BeMSLezDKR56Z4xExjATeCqyPmGGqG0+AaJqoIcJ5yI0QX9Hg/JAcLvCAa5rYYUzgkXwhesR8RjzkR8g/KUFdJn7E/JMK8BL6hs2X42sS/LCkDyE/Qr5Ysl/kh+Y84keMrwO+GeJHrH8OdEqD/NCcR/ywkUfiS/yIvdeB/iHIjxXnIT8S1kf80HF9oL5IwmTlQH2RhNHKgf4mqH+Us7y5+iLED6n4+pAfMb6gHgjxQ4d6VZDBGfWPfJY3d36W+BHzmazPJsxXxb/6MjdO2oT+UYD6YokfOvLA9GyRH6E+F8BfS/y45wvw16L5Ku73yY/pjnm7d9Z/Lq6Z+vbHPr4w/gIAAP//AwAiyNgo",
    };

    public static IEnumerable<object[]> BlueprintsWithIsolatedAreasIndexes = Enumerable
        .Range(0, BlueprintsWithIsolatedAreas.Count)
        .Select(i => new object[] { i });

    [Theory]
    [MemberData(nameof(BlueprintsWithIsolatedAreasIndexes))]
    public void RejectsBlueprintWithBlockingIsolatedArea(int index)
    {
        var options = OilFieldOptions.ForMediumElectricPole;

        // this has a pumpjack that has it's top and right terminal blocked by other pumpjacks and the bottom and
        // left terminals pointed into an isolated area. There is probably a solution if you place underground pipes
        // from the beginning, but that's not supported today. Underground pipes are only optimized from a fully
        // connected system of above ground pipes.
        var blueprintString = BlueprintsWithIsolatedAreas[index];

        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var ex = Assert.Throws<NoPathBetweenTerminalsException>(() => Planner.Execute(options, blueprint));
    }

    [Fact]
    public void AllowsPumpjackWithDefaultDirection()
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        var blueprintString = "0eNqV1OtqgzAUAOB3Ob9DMTHHJL7KKMN2YWSrqXgZE8m7Tx1mgxp6/KmYz3PNBJfbYJvW+R7KCdz17jsoXybo3Luvbsu7fmwslOB6WwMDX9XLUzPUzUd1/YTAwPk3+w0lD2cG1veud/bXWB/GVz/UF9vOHzyeZtDcu/nA3S9/mhGp+QkZjFCKPDthCOyBERTG5M+YnMSojRFmn5EEBjOM0Yh9BknR/DHFPlOQShwZmYhGkZLKIpPvM5qUlIhMIilzrMQphmckJ3YcE4PD+bFeIU84lEFGHvcBE1XmlElGEbuFKuFIUl5FdBIbwQ/OciETDmmYjd4cpROOOlZnlaqPJtUZn8ZjSEtqNkcn+i5o8xzXS6/xzHf0epOX/y5+Bl+27dZDQnOpzBy7yDmiCOEHgSfwqQ==";
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (_, result) = Planner.Execute(options, blueprint);

        // Assert
        Assert.Equal(16, result.RotatedPumpjacks);
    }

    [Fact]
    public void AllowsElectricPolesToNotBePlanned()
    {
        // Arrange
        var options = OilFieldOptions.ForBigElectricPole;
        options.ValidateSolution = true;
        options.AddElectricPoles = false;
        var blueprintString = SmallListBlueprintStrings[0];
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (context, _) = Planner.Execute(options, blueprint);

        // Assert
        Assert.Empty(context.Grid.GetEntities().OfType<ElectricPoleCenter>());
        Assert.Empty(context.Grid.GetEntities().OfType<ElectricPoleSide>());
    }

    [Fact]
    public void AllowsBeaconsToNotBePlanned()
    {
        // Arrange
        var options = OilFieldOptions.ForBigElectricPole;
        options.ValidateSolution = true;
        options.AddBeacons = false;
        var blueprintString = SmallListBlueprintStrings[0];
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (context, _) = Planner.Execute(options, blueprint);

        // Assert
        Assert.Empty(context.Grid.GetEntities().OfType<BeaconCenter>());
        Assert.Empty(context.Grid.GetEntities().OfType<BeaconSide>());
    }

    [Fact]
    public async Task AllowsLocationsToBeAvoided()
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        options.AddBeacons = true;
        var blueprint = new Blueprint
        {
            Entities = new[]
            {
                new Entity { Name = EntityNames.Vanilla.Pumpjack, Position = new Position { X = -3, Y = -5 } },
                new Entity { Name = EntityNames.Vanilla.Pumpjack, Position = new Position { X = 4, Y = 5 } },
            },
            Icons = new[]
            {
                new Icon
                {
                    Index = 1,
                    Signal = new SignalID
                    {
                        Name = EntityNames.Vanilla.Pumpjack,
                        Type = SignalTypes.Vanilla.Item,
                    }
                }
            },
            Item = ItemNames.Vanilla.Blueprint,
            Version = 0,
        };
        var avoid = Enumerable.Range(-7, 16).Select(x => new AvoidLocation(x, 0)).ToTableArray();

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
    public async Task ExecuteSample()
    {
        var result = Planner.ExecuteSample();

#if USE_VERIFY
        await Verify(GetGridString(result));
#else
        await Task.Yield();
#endif
    }

    [Fact]
    public void SetsPumpjackCenterDirection()
    {
        var (context, _) = Planner.ExecuteSample();

        var centers = context
            .Grid
            .EntityLocations
            .EnumerateItems()
            .Select(l => (Location: l, Entity: (context.Grid[l] as PumpjackCenter)!))
            .Where(l => l.Entity is not null)
            .OrderBy(x => x.Location.Y)
            .ThenBy(x => x.Location.X)
            .ToList();
        Assert.Equal(4, centers.Count);
        Assert.Equal(Direction.Down, centers[0].Entity.Direction);
        Assert.Equal(Direction.Left, centers[1].Entity.Direction);
        Assert.Equal(Direction.Up, centers[2].Entity.Direction);
        Assert.Equal(Direction.Right, centers[3].Entity.Direction);
    }

    [Fact]
    public void SetsDeltasFromOriginalPositions()
    {
        var (context, _) = Planner.ExecuteSample();

        Assert.Equal(16, context.DeltaX);
        Assert.Equal(13, context.DeltaY);
    }

    [Fact]
    public void CountsAllRotatedPumpjacks()
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        var blueprintString = "0eJyNkMsOgjAQRf/lrisJBcR26W8YY3hMTBVKU4qRkP67BaIxsnE3jztn7syEshnIWKUd5ARVdbqHPE3o1VUXzVxzoyFIKEctGHTRzpkZWnMrqjs8g9I1PSFjf2Yg7ZRTtDKWZLzooS3JBsF2msF0fRjo9LwpQHZJlDGMIciiLLBrZala+3vPNkj+B/JNTH+BfDa8nCW/vsDwINuvgkOc5oLnKRciEcF+U5QUfoLjR+39C6d7aOc=";
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        (_, var summary) = Planner.Execute(options, blueprint);

        // Assert
        Assert.Equal(2, summary.RotatedPumpjacks);
    }

    [Fact]
    public void CountsSomeRotatedPumpjacks()
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        var blueprintString = "0eJyNkE0OgjAQhe/y1pWECkG69BrGmAITU6WlocVISO9ugWiMbNzNz5vvzcyEqh3I9sp4iAmq7oyDOE1w6mpkO9f8aAkCypMGg5F6zuyg7U3WdwQGZRp6QqThzEDGK69oZSzJeDGDrqiPgu00g+1cHOjM7BQhu32SM4wxyJM8shvVU732eWAbJP8D+SZmv8BsXng5S3x9geFBvVsdD2lWlLzIeFnuy7h+KyuKP8Hxow7hBaWraOU=";
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        (_, var summary) = Planner.Execute(options, blueprint);

        // Assert
        Assert.Equal(1, summary.RotatedPumpjacks);
    }

    [Fact]
    public void CountsNoRotatedPumpjacks()
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        var blueprintString = "0eJyNkMsOgjAQRf/lriuRV7Bd+hvGGB4TU6WlKcVISP/dAtEY2bibx51zZ2ZC1Q5krNQOYoKsO91DnCb08qrLdq650RAEpCMFBl2qOTODMreyvsMzSN3QEyL2ZwbSTjpJK2NJxoseVEU2CLbTDKbrw0CnZ6cA2aVRzjCGII/ywG6kpXrtJ55tkMkfyDcx+wXu54WXs8TXFxgeZPvV8RBnBU+KLOE85WH9tqwo/ATHj9r7F6STaOE=";
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        (_, var summary) = Planner.Execute(options, blueprint);

        // Assert
        Assert.Equal(0, summary.RotatedPumpjacks);
    }

    [Fact]
    public void YieldsAlternateSolutions()
    {
        // Arrange
        var options = OilFieldOptions.ForMediumElectricPole;
        options.ValidateSolution = true;
        var blueprintString = "0eJyU1M1ugzAMB/B38TkH8sGgeZWqmii1pmwlRBCmIcS7L9QcNhUp7pFgfhiSvxe43icMg/MR7AKu7f0I9rzA6D58c9/WfNMhWAhTFz6b9gsExDlsKy5iB6sA52/4A1auFwHoo4sOyXhczO9+6q44pAJxYIV+TA/0fntTQowRMKdSndybG7Cle8UqnjjF4WriTJ7TDK6UxNV5zjA4Rd2pIs+VDE7unMxzbwxOK+IYW1FxuquIq/JczeE0cYytOHE4Onda5TlZ8L+W5XFyoUu+xwoG/T7NCIbkJEPu/ZX/PXXkcaKh5aF32N8L2TBPhznNrMccs38GoYBvHMa9YP0FAAD//wMAiTWvrw==";
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (_, summary) = Planner.Execute(options, blueprint);

        // Assert
        Assert.Single(summary.SelectedPlans.EnumerateItems());
        Assert.Single(summary.AlternatePlans.EnumerateItems());
        Assert.NotEmpty(summary.UnusedPlans.EnumerateItems());
    }
}
