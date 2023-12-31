using Knapcode.FactorioTools.OilField.Steps;

namespace Knapcode.FactorioTools.OilField;

public class PlannerFacts : BasePlannerFacts
{
    /// <summary>
    /// This blueprint found a bug in the SortedBatches class.
    /// </summary>
    [Fact]
    public void PlansPowerPoles()
    {
        // Arrange
        var options = OilFieldOptions.ForSubstation;
        options.ValidateSolution = true;
        var blueprintString = "0eJyM00sOgyAQANC7zJqFgp+WqzRN42fS0CoSwabGePeidNHEJsySYXjM8Fmg7iY0o9IO5AKqGbQFeVnAqruuui2mqx5Bgpl686iaJzBws9kiymEPKwOlW3yDTNcrA9ROOYXB2AfzTU99jaNPYH8sM1i/YNDbTh4RGYPZpwrvtmrEJswlKztwnMDxPHBZnBMELi0CV8S5jFJdErgyzuWU6sTO8TTOFQQuS8hcSWk2nB0n3OyJ0my4WZ7HuTO9WXGozr/p/Z3Ln4/C4IWj/SasHwAAAP//AwCxgxNI";
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        Planner.Execute(options, blueprint);
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
        var blueprintString = "0eJyU1EtugzAQBuC7zNoL/CBQX6WqIkJGldtgLDBVEeLuMRkWbYnk6RIzfIwfvxe43CYMg/MR7AKu7f0I9nWB0b375raN+aZDsBCmLnw07ScIiHPYRlzEDlYBzl/xG6xc3wSgjy46JOPxMJ/91F1wSAXiiRX6MX3Q++1PCTFGwJxKdXKvbsCW3hWrOHCKw9XEmTynGVwpiavznGFwirpTRZ4rGZzcOZnnTgxOK+IYW1FxuquIq/JczeE0cYyteOFwdO60ynOy4M+W5XFyoUu+xwoGLZ9mBGNb6fx89/7K315q9+hxoqEpaX+9p/39IxvmcJjTnfW4x+yPi1DAFw7jXrDeAQAA//8DAIk1r68=";
        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (_, summary) = Planner.Execute(options, blueprint);

        // Assert
        Assert.Single(summary.SelectedPlans);
        Assert.Single(summary.AlternatePlans);
        Assert.NotEmpty(summary.UnusedPlans);
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
        options.PipeStrategies = new List<PipeStrategy> { PipeStrategy.FbeOriginal };
        var blueprintString = "0eJyM1ctuhCAUBuB3OWsWAt5fpWkmjkMa2hGNl6bG+O4VzyzaGRL+pYgfB+GHja73xQyjdTPVG9m2dxPVbxtN9sM1d9/mms5QTcPSDZ9N+0WC5nXwLXY2He2CrLuZH6rl/i7IuNnO1rBxPqwXt3RXMx4dRMAa+un4oHd+pAPRStB6dNWHe7OjafldsosXTgGczJnL45wGOFUwV8a5FOFS5qo4lyGTrU5OJXEuR5aCq1MyzhXIZHlllfrPqQBXIlwV5ELVVci/K5nT8epkgngZe1m8PCnxrQJ5SDBUwh4QDIkkQ0v2CsBDouEH9V4JrAeSDfXwgKj5MyPulbhX4PtFA2HzWxWdr36KRxrykHwoPkifvdB6+K2FeulLfI875LxX6j8Xk6BvM06PDvsvAAAA//8DANANMhY=";

        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (_, summary) = Planner.Execute(options, blueprint);

        // Assert
        var plan = Assert.Single(summary.SelectedPlans);
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
        options.PipeStrategies = new List<PipeStrategy> { PipeStrategy.FbeOriginal };
        var blueprintString = "0eJyUlsluwzAMRP+FZx2sxeuvFEWRRSjUxorhpWgQ+N9rhToUiQCNj3HkZ4qaGepOx8tih9H5mbo7udPVT9S93Wlyn/5wCc/8obfU0bD0w9fh9E2C5tsQnrjZ9rQKcv5sf6mT67sg62c3O8uMx4/bh1/6ox23BSLBGq7T9sLVhy9tEN0Ium1L9cY9u9Ge+L9iFS84BeDKgnFlHqeR6irGNXmcAXBVrK7N40oAZ+QDp4o8rkJ6pxgn87gawdWMU3lcgxxFyThAKC3SuxrGyQIpr2WeAXiIL6RmHiBkiRhDxfoqgIc4w5RJnkrxEGvouN8aqA/yRoHz9pgD4SHu0FF/QLJIxB4m9q8FzgPyR5PkJXN5hz80EC4KmhusPw2ki1J4HjzzUv1T0ORg/WlkriH+MArnlfighHjVjvN9yj+T4tU79lsBPMgfUS9A/oWozO/XMA/IgyAFWC9AHmi5Q8+Af7XC89kA/g1LUb0YQH+h1WgeGGS/0Pzg833lbXfexz24+3eRFvRjxykuWP8AAAD//wMA5MG5Zw==";

        var blueprint = ParseBlueprint.Execute(blueprintString);

        // Act
        var (_, summary) = Planner.Execute(options, blueprint);

        // Assert
        var plan = Assert.Single(summary.SelectedPlans);
        Assert.Equal(PipeStrategy.Fbe, plan.PipeStrategy);
    }
}
