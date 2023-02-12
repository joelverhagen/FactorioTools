using System.Collections;
using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class AddBeacons
{
    public static void Execute(Context context)
    {
        var poweredEntities = context.CenterToTerminals.Keys.Select(c => new ProviderRecipient(c, Width: 3, Height: 3)).ToList();

        var candidateToCovered = GetCandidateToCovered(
            context,
            poweredEntities,
            context.Options.BeaconWidth,
            context.Options.BeaconHeight,
            context.Options.BeaconSupplyWidth,
            context.Options.BeaconSupplyHeight);

        var candidateToEntityDistance = GetCandidateToEntityDistance(poweredEntities, candidateToCovered);

        var coveredEntities = new BitArray(poweredEntities.Count);

        // Visualizer.Show(context.Grid, candidateToCovered.Keys.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        var beacons = new Dictionary<Location, BeaconCenter>();

        while (candidateToCovered.Count > 0)
        {
            var candidate = candidateToCovered
                .Keys
                /*
                .OrderByDescending(x => candidateToCovered[x].CountTrue())
                .ThenBy(x => beacons.Count > 0 ? beacons.Keys.Min(c => c.GetEuclideanDistance(x)) : 0)
                .ThenByDescending(x => candidateToEntityDistance[x])
                */
                .OrderBy(x => beacons.Count > 0 ? beacons.Keys.Min(c => c.GetManhattanDistance(x)) : 0)
                .ThenByDescending(x => candidateToCovered[x].CountTrue())
                .ThenByDescending(x => candidateToEntityDistance[x])
                .First();

             var centerEntity = new BeaconCenter();

            AddProvider(
                context.Grid,
                candidate,
                centerEntity,
                c => new BeaconSide(c),
                context.Options.BeaconWidth,
                context.Options.BeaconHeight,
                candidateToCovered);

            beacons.Add(candidate, centerEntity);

            // Visualizer.Show(context.Grid, candidateToCovered.Keys.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());
        }

        // Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());
    }
}
