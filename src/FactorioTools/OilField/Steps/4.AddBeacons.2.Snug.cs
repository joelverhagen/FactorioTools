using Knapcode.FactorioTools.OilField.Grid;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static partial class AddBeacons
{
    private static Dictionary<Location, BeaconCenter> AddBeacons_Snug(Context context)
    {
        var poweredEntities = context
            .CenterToTerminals
            .Keys
            .Select(c => new ProviderRecipient(c, PumpjackWidth, PumpjackHeight))
            .ToList();

        // We don't try to remove unused beacons here because there should not be any existing beacons at this point.
        (var candidateToCovered, var coveredEntities, var beacons) = GetBeaconCandidateToCovered(context, poweredEntities, removeUnused: false);

        if (context.Options.ValidateSolution && beacons.Count > 0)
        {
            throw new InvalidOperationException("There should not be any existing beacons.");
        }

        var candidateToMiddleDistance = candidateToCovered.ToDictionary(
            x => x.Key,
            x => x.Key.GetEuclideanDistance(context.Grid.Middle));

        var candidateToEntityDistance = GetCandidateToEntityDistance(poweredEntities, candidateToCovered);

        // Visualizer.Show(context.Grid, candidateToCovered.Keys.Select(l => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(l.X, l.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        while (candidateToCovered.Count > 0)
        {
            var candidate = candidateToCovered
                .Keys
                .MinBy(c => (
                    beacons.Count > 0 ? beacons.Keys.Min(x => x.GetManhattanDistance(c)) : 0,
                    -candidateToCovered[c].CountTrue(),
                    -candidateToEntityDistance[c],
                    candidateToMiddleDistance[c]
                ))!;

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

        return beacons;
    }
}