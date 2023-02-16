using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

public class HelpersTest
{
    public class GetProviderCenterToCoveredCentersTest : Facts
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

            var providerCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
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

            var providerCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
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

            var providerCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
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

            var providerCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
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

            var providerCenterToCoveredCenters = GetProviderCenterToCoveredCenters(
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
    }

    public class GetElectricPoleCandidateToCovered : Facts
    {
        [Fact]
        public void SmallElectricPole_RemoveUnused()
        {
            var context = InitializeContext.GetEmpty(Options.ForSmallElectricPole, width: 20, height: 7);
            AddElectricPole(context, new Location(1, 3));
            AddElectricPole(context, new Location(5, 3));
            AddElectricPole(context, new Location(8, 3));
            var pumpjacks = new[] { new Location(3, 2), new Location(13, 2) }
                .Select(c => new ProviderRecipient(c, PumpjackWidth, PumpjackHeight))
                .ToList();
            foreach (var pumpjack in pumpjacks)
            {
                AddPumpjack(context, pumpjack.Center);
            }

            (var candidateToCovered, var coveredEntities, var providers) = GetElectricPoleCandidateToCovered(
                context,
                pumpjacks,
                removeUnused: true);

            Assert.IsType<ElectricPoleCenter>(context.Grid[new Location(1, 3)]);
            Assert.IsType<ElectricPoleCenter>(context.Grid[new Location(5, 3)]);
            Assert.True(context.Grid.IsEmpty(new Location(8, 3)));

            Assert.Equal(2, providers.Count);
            Assert.Contains(new Location(1, 3), providers.Keys);
            Assert.Contains(new Location(5, 3), providers.Keys);

            Assert.Equal(2, coveredEntities.Count);
            Assert.True(coveredEntities[0]);
            Assert.False(coveredEntities[1]);

            // columns to the left and right of the pumpjack
            Assert.All(Enumerable.Range(0, 6).Select(y => new Location(10, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 10
            Assert.All(Enumerable.Range(0, 6).Select(y => new Location(11, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 11
            Assert.All(Enumerable.Range(0, 6).Select(y => new Location(15, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 15
            Assert.All(Enumerable.Range(0, 6).Select(y => new Location(16, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 16

            // columns broken up by the pumpjack itself
            Assert.All(new[] { new Location(12, 0) }.Concat(Enumerable.Range(4, 2).Select(y => new Location(12, y))), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 12
            Assert.All(new[] { new Location(12, 0) }.Concat(Enumerable.Range(4, 2).Select(y => new Location(13, y))), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 13
            Assert.All(new[] { new Location(12, 0) }.Concat(Enumerable.Range(4, 2).Select(y => new Location(14, y))), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 14

            Assert.Equal(33, candidateToCovered.Count);

            Assert.All(candidateToCovered.Values, c => Assert.Equal(2, c.Count));
            Assert.All(candidateToCovered.Values, c => Assert.False(c[0]));
            Assert.All(candidateToCovered.Values, c => Assert.True(c[1]));
        }

        [Fact]
        public void Substation_RemoveUnused()
        {
            var context = InitializeContext.GetEmpty(Options.ForSubstation, width: 16, height: 16);
            AddElectricPole(context, new Location(10, 12));
            AddElectricPole(context, new Location(12, 10));
            AddElectricPole(context, new Location(12, 12));
            var pumpjacks = new[] { new Location(2, 2),}
                .Select(c => new ProviderRecipient(c, PumpjackWidth, PumpjackHeight))
                .ToList();
            foreach (var pumpjack in pumpjacks)
            {
                AddPumpjack(context, pumpjack.Center);
            }

            (var candidateToCovered, var coveredEntities, var providers) = GetElectricPoleCandidateToCovered(
                context,
                pumpjacks,
                removeUnused: true);
            
            // Visualizer.Show(context.Grid, candidateToCovered.Keys.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

            Assert.All(context.Grid.EntityToLocation.Keys, e => Assert.True(e is PumpjackSide || e is PumpjackCenter));
            Assert.Empty(providers);

            // These are positions that should become candidates after unused substations are removed.
            Assert.Contains(new Location(9, 11), candidateToCovered.Keys);
            Assert.Contains(new Location(10, 11), candidateToCovered.Keys);
            Assert.Contains(new Location(11, 9), candidateToCovered.Keys);
            Assert.Contains(new Location(11, 10), candidateToCovered.Keys);
            Assert.Contains(new Location(11, 11), candidateToCovered.Keys);

            Assert.Equal(128, candidateToCovered.Count);
            Assert.All(candidateToCovered.Values, c => Assert.Single(c));
            Assert.All(candidateToCovered.Values, c => Assert.True(c[0]));

            // columns blocked at the top by the pumpjack itself
            Assert.All(Enumerable.Range(4, 8).Select(y => new Location(0, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 0
            Assert.All(Enumerable.Range(4, 8).Select(y => new Location(1, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 1
            Assert.All(Enumerable.Range(4, 8).Select(y => new Location(2, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 2
            Assert.All(Enumerable.Range(4, 8).Select(y => new Location(3, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 3

            // columns not blocked by the pumpjack
            Assert.All(Enumerable.Range(0, 12).Select(y => new Location(4, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 4
            Assert.All(Enumerable.Range(0, 12).Select(y => new Location(5, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 5
            Assert.All(Enumerable.Range(0, 12).Select(y => new Location(6, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 6
            Assert.All(Enumerable.Range(0, 12).Select(y => new Location(7, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 7
            Assert.All(Enumerable.Range(0, 12).Select(y => new Location(8, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 8
            Assert.All(Enumerable.Range(0, 12).Select(y => new Location(9, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 9
            Assert.All(Enumerable.Range(0, 12).Select(y => new Location(10, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 10
            Assert.All(Enumerable.Range(0, 12).Select(y => new Location(11, y)), l => Assert.Contains(l, candidateToCovered.Keys)); // x = 11
        }
    }

    public class Facts
    {
        internal static ElectricPoleCenter AddElectricPole(Context context, Location center)
        {
            var entity = new ElectricPoleCenter();

            AddProvider(
                context.Grid,
                center,
                entity,
                c => new ElectricPoleSide(c),
                providerWidth: context.Options.ElectricPoleWidth,
                providerHeight: context.Options.ElectricPoleHeight);

            return entity;
        }

        internal static BeaconCenter AddBeacon(Context context, Location center)
        {
            var entity = new BeaconCenter();

            AddProvider(
                context.Grid,
                center,
                entity,
                c => new BeaconSide(c),
                providerWidth: context.Options.BeaconWidth,
                providerHeight: context.Options.BeaconHeight);

            return entity;
        }

        internal static PumpjackCenter AddPumpjack(Context context, Location center)
        {
            var entity = Helpers.AddPumpjack(context.Grid, center);

            context.CenterToTerminals = GetCenterToTerminals(context.Grid, context.CenterToTerminals.Keys.Concat(new[] { center }.Distinct()));
            context.LocationToTerminals = GetLocationToTerminals(context.CenterToTerminals);

            foreach (var terminals in context.CenterToTerminals.Values.ToList())
            {
                EliminateOtherTerminals(context, terminals.First());
            }

            return entity;
        }
    }
}