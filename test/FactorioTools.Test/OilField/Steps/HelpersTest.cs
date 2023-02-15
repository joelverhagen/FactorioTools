using Knapcode.FactorioTools.OilField.Data;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

public class HelpersTest
{
    public class GetProviderCenterToCoveredCentersTest
    {
        [Fact]
        public void SmallElectricPole()
        {
            var context = InitializeContext.GetEmpty(Options.ForSmallElectricPole, width: 20, height: 20);
            var center = new Location(10, 10);
            AddElectricPole(context, center);

            AddBeacon(context, new Location(11, 12));
            AddBeacon(context, new Location(12, 6));
            AddBeacon(context, new Location(12, 9));
            AddBeacon(context, new Location(14, 12));
            AddBeacon(context, new Location(16, 7));
            AddBeacon(context, new Location(6, 9));
            AddBeacon(context, new Location(7, 13));
            AddBeacon(context, new Location(9, 8));

            var providerCenterToCoveredCenters = Helpers.GetProviderCenterToCoveredCenters(
                context.Grid,
                context.Options.ElectricPoleWidth,
                context.Options.ElectricPoleHeight,
                context.Options.ElectricPoleSupplyWidth,
                context.Options.ElectricPoleSupplyHeight,
                new[] { center },
                includePumpjacks: true,
                includeBeacons: true);

            (var providerCenter, var coveredCenters) = providerCenterToCoveredCenters.Single();
            Assert.Equal(center, providerCenter);
            Assert.Equal(4, coveredCenters.Count);
            Assert.Contains(new Location(11, 12), coveredCenters);
            Assert.Contains(new Location(12, 9), coveredCenters);
            Assert.Contains(new Location(7, 13), coveredCenters);
            Assert.Contains(new Location(9, 8), coveredCenters);
        }

        [Fact]
        public void MediumElectricPole()
        {
            var context = InitializeContext.GetEmpty(Options.ForMediumElectricPole, width: 20, height: 20);
            var center = new Location(10, 10);
            AddElectricPole(context, center);

            AddBeacon(context, new Location(12, 9));
            AddBeacon(context, new Location(13, 6));
            AddBeacon(context, new Location(14, 14));
            AddBeacon(context, new Location(15, 10));
            AddBeacon(context, new Location(16, 7));
            AddBeacon(context, new Location(5, 9));
            AddBeacon(context, new Location(6, 14));
            AddBeacon(context, new Location(6, 6));
            AddBeacon(context, new Location(9, 12));
            AddBeacon(context, new Location(9, 7));

            var providerCenterToCoveredCenters = Helpers.GetProviderCenterToCoveredCenters(
                context.Grid,
                context.Options.ElectricPoleWidth,
                context.Options.ElectricPoleHeight,
                context.Options.ElectricPoleSupplyWidth,
                context.Options.ElectricPoleSupplyHeight,
                new[] { center },
                includePumpjacks: true,
                includeBeacons: true);

            (var providerCenter, var coveredCenters) = providerCenterToCoveredCenters.Single();
            Assert.Equal(center, providerCenter);
            Assert.Equal(7, coveredCenters.Count);
            Assert.Contains(new Location(12, 9), coveredCenters);
            Assert.Contains(new Location(13, 6), coveredCenters);
            Assert.Contains(new Location(14, 14), coveredCenters);
            Assert.Contains(new Location(6, 14), coveredCenters);
            Assert.Contains(new Location(6, 6), coveredCenters);
            Assert.Contains(new Location(9, 12), coveredCenters);
            Assert.Contains(new Location(9, 7), coveredCenters);
        }

        [Fact]
        public void Substation()
        {
            var context = InitializeContext.GetEmpty(Options.ForSubstation, width: 28, height: 28);
            var center = new Location(12, 12);
            AddElectricPole(context, center);

            AddBeacon(context, new Location(11, 15));
            AddBeacon(context, new Location(11, 18));
            AddBeacon(context, new Location(11, 21));
            AddBeacon(context, new Location(11, 24));
            AddBeacon(context, new Location(15, 3));
            AddBeacon(context, new Location(19, 16));
            AddBeacon(context, new Location(2, 13));
            AddBeacon(context, new Location(2, 2));
            AddBeacon(context, new Location(2, 5));
            AddBeacon(context, new Location(20, 13));
            AddBeacon(context, new Location(21, 10));
            AddBeacon(context, new Location(22, 22));
            AddBeacon(context, new Location(22, 7));
            AddBeacon(context, new Location(23, 4));
            AddBeacon(context, new Location(3, 22));
            AddBeacon(context, new Location(4, 8));
            AddBeacon(context, new Location(6, 17));
            AddBeacon(context, new Location(9, 9));

            var providerCenterToCoveredCenters = Helpers.GetProviderCenterToCoveredCenters(
                context.Grid,
                context.Options.ElectricPoleWidth,
                context.Options.ElectricPoleHeight,
                context.Options.ElectricPoleSupplyWidth,
                context.Options.ElectricPoleSupplyHeight,
                new[] { center },
                includePumpjacks: true,
                includeBeacons: true);

            /*
            var bp = GridToBlueprintString.Execute(context, addOffsetCorrection: true);
            var pair = providerCenterToCoveredCenters.Single();
            Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), pair.Value.Select(p => (DelaunatorSharp.IEdge)new DelaunatorSharp.Edge(0, new DelaunatorSharp.Point(p.X, p.Y), new DelaunatorSharp.Point(pair.Key.X, pair.Key.Y))));
            */

            (var providerCenter, var coveredCenters) = providerCenterToCoveredCenters.Single();
            Assert.Equal(center, providerCenter);
            Assert.Equal(13, coveredCenters.Count);
            Assert.Contains(new Location(11, 15), coveredCenters);
            Assert.Contains(new Location(11, 18), coveredCenters);
            Assert.Contains(new Location(11, 21), coveredCenters);
            Assert.Contains(new Location(15, 3), coveredCenters);
            Assert.Contains(new Location(19, 16), coveredCenters);
            Assert.Contains(new Location(20, 13), coveredCenters);
            Assert.Contains(new Location(21, 10), coveredCenters);
            Assert.Contains(new Location(22, 22), coveredCenters);
            Assert.Contains(new Location(22, 7), coveredCenters);
            Assert.Contains(new Location(3, 22), coveredCenters);
            Assert.Contains(new Location(4, 8), coveredCenters);
            Assert.Contains(new Location(6, 17), coveredCenters);
            Assert.Contains(new Location(9, 9), coveredCenters);
        }

        [Fact]
        public void BigElectricPole()
        {
            var context = InitializeContext.GetEmpty(Options.ForBigElectricPole, width: 20, height: 20);
            var center = new Location(10, 10);
            AddElectricPole(context, center);

            AddBeacon(context, new Location(7, 11));
            AddBeacon(context, new Location(10, 8));
            AddBeacon(context, new Location(10, 13));
            AddBeacon(context, new Location(13, 12));
            AddBeacon(context, new Location(14, 9));

            var providerCenterToCoveredCenters = Helpers.GetProviderCenterToCoveredCenters(
                context.Grid,
                context.Options.ElectricPoleWidth,
                context.Options.ElectricPoleHeight,
                context.Options.ElectricPoleSupplyWidth,
                context.Options.ElectricPoleSupplyHeight,
                new[] { center },
                includePumpjacks: true,
                includeBeacons: true);
            
            /*
            var bp = GridToBlueprintString.Execute(context, addOffsetCorrection: true);
            var pair = providerCenterToCoveredCenters.Single();
            Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), pair.Value.Select(p => (DelaunatorSharp.IEdge)new DelaunatorSharp.Edge(0, new DelaunatorSharp.Point(p.X, p.Y), new DelaunatorSharp.Point(pair.Key.X, pair.Key.Y))));
            */

            (var providerCenter, var coveredCenters) = providerCenterToCoveredCenters.Single();
            Assert.Equal(center, providerCenter);
            Assert.Equal(3, coveredCenters.Count);
            Assert.Contains(new Location(10, 13), coveredCenters);
            Assert.Contains(new Location(10, 8), coveredCenters);
            Assert.Contains(new Location(13, 12), coveredCenters);
        }

        [Fact]
        public void Beacon()
        {
            var context = InitializeContext.GetEmpty(Options.ForSmallElectricPole, width: 20, height: 20);
            var center = new Location(10, 10);
            AddBeacon(context, center);

            AddPumpjack(context, new Location(10, 13));
            AddPumpjack(context, new Location(10, 16));
            AddPumpjack(context, new Location(12, 4));
            AddPumpjack(context, new Location(13, 11));
            AddPumpjack(context, new Location(15, 15));
            AddPumpjack(context, new Location(15, 6));
            AddPumpjack(context, new Location(4, 14));
            AddPumpjack(context, new Location(5, 11));
            AddPumpjack(context, new Location(5, 5));
            AddPumpjack(context, new Location(6, 8));
            AddPumpjack(context, new Location(9, 7));

            var providerCenterToCoveredCenters = Helpers.GetProviderCenterToCoveredCenters(
                context.Grid,
                context.Options.BeaconWidth,
                context.Options.BeaconHeight,
                context.Options.BeaconSupplyWidth,
                context.Options.BeaconSupplyHeight,
                new[] { center },
                includePumpjacks: true,
                includeBeacons: false);
            
            /*
            var bp = GridToBlueprintString.Execute(context, addOffsetCorrection: true);
            var pair = providerCenterToCoveredCenters.Single();
            Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), pair.Value.Select(p => (DelaunatorSharp.IEdge)new DelaunatorSharp.Edge(0, new DelaunatorSharp.Point(p.X, p.Y), new DelaunatorSharp.Point(pair.Key.X, pair.Key.Y))));
            */

            (var providerCenter, var coveredCenters) = providerCenterToCoveredCenters.Single();
            Assert.Equal(center, providerCenter);
            Assert.Equal(8, coveredCenters.Count);
            Assert.Contains(new Location(15, 15), coveredCenters);
            Assert.Contains(new Location(15, 6), coveredCenters);
            Assert.Contains(new Location(10, 13), coveredCenters);
            Assert.Contains(new Location(13, 11), coveredCenters);
            Assert.Contains(new Location(5, 11), coveredCenters);
            Assert.Contains(new Location(5, 5), coveredCenters);
            Assert.Contains(new Location(6, 8), coveredCenters);
            Assert.Contains(new Location(9, 7), coveredCenters);
        }

        private static ElectricPoleCenter AddElectricPole(Context context, Location center)
        {
            var entity = new ElectricPoleCenter();

            Helpers.AddProvider(
                context.Grid,
                center,
                entity,
                c => new ElectricPoleSide(c),
                providerWidth: context.Options.ElectricPoleWidth,
                providerHeight: context.Options.ElectricPoleHeight);

            return entity;
        }

        private static BeaconCenter AddBeacon(Context context, Location center)
        {
            var entity = new BeaconCenter();

            Helpers.AddProvider(
                context.Grid,
                center,
                entity,
                c => new BeaconSide(c),
                providerWidth: context.Options.BeaconWidth,
                providerHeight: context.Options.BeaconHeight);

            return entity;
        }

        private static PumpjackCenter AddPumpjack(Context context, Location center)
        {
            var entity = Helpers.AddPumpjack(context.Grid, center);

            context.CenterToTerminals = Helpers.GetCenterToTerminals(context.Grid, context.CenterToTerminals.Keys.Concat(new[] { center }.Distinct()));
            context.LocationToTerminals = Helpers.GetLocationToTerminals(context.CenterToTerminals);

            foreach (var terminals in context.CenterToTerminals.Values.ToList())
            {
                Helpers.EliminateOtherTerminals(context, terminals.First());
            }

            return entity;
        }
    }
}