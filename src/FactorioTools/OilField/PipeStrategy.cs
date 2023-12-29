namespace Knapcode.FactorioTools.OilField;

/// <summary>
/// The strategy to use while planning pipes between pumpjacks.
/// </summary>
public enum PipeStrategy
{
    /// <summary>
    /// The original Factorio Blueprint Editor (FBE) pipe planning strategy.
    /// </summary>
    FbeOriginal,

    /// <summary>
    /// The Factorio Blueprint Editor (FBE) pipe planning strategy, with minor modifications.
    /// </summary>
    Fbe,

    /// <summary>
    /// The connected centers pipe planning strategy, using a Delaunay Triangulation to identify pumpjacks that should
    /// be connected.
    /// </summary>
    ConnectedCentersDelaunay,

    /// <summary>
    /// The connected centers pipe planning strategy, using a Delaunay Triangulation following by a Prim's minimum
    /// spanning tree to identify pumpjacks that should be connected.
    /// </summary>
    ConnectedCentersDelaunayMst,

    /// <summary>
    /// The connected centers pipe planning strategy, using Dr. Chris C. N. Chu's FLUTE algorithm to identify pumpjacks
    /// that should be connected.
    /// </summary>
    ConnectedCentersFlute,
}
