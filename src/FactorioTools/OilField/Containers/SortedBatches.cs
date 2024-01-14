using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class SortedBatches<TInfo>
{
    private readonly bool _ascending;

    public SortedBatches(IReadOnlyCollection<KeyValuePair<int, ILocationDictionary<TInfo>>> pairs, bool ascending)
    {
        _ascending = ascending;
        Queue = new PriorityQueue<ILocationDictionary<TInfo>, int>();
        Lookup = new Dictionary<int, ILocationDictionary<TInfo>>();

        foreach ((var key, var candidateToInfo) in pairs)
        {
            Queue.Enqueue(candidateToInfo, _ascending ? key : -key);
            Lookup.Add(key, candidateToInfo);
        }
    }

    public PriorityQueue<ILocationDictionary<TInfo>, int> Queue { get; }
    public Dictionary<int, ILocationDictionary<TInfo>> Lookup { get; }

    public void RemoveCandidate(Location location, int oldKey)
    {
        Lookup[oldKey].Remove(location);
    }

    public void MoveCandidate(Context context, Location location, TInfo info, int oldKey, int newKey)
    {
        Lookup[oldKey].Remove(location);
        if (Lookup.TryGetValue(newKey, out var batches))
        {
            batches.Add(location, info);
        }
        else
        {
            batches = context.GetLocationDictionary<TInfo>();
            batches.Add(location, info);
            Lookup.Add(newKey, batches);
        }
    }
}