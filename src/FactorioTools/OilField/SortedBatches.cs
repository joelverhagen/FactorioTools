using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class SortedBatches<TInfo>
{
    private readonly bool _ascending;

    public SortedBatches(IEnumerable<KeyValuePair<int, Dictionary<Location, TInfo>>> pairs, bool ascending)
    {
        _ascending = ascending;
        Queue = new PriorityQueue<Dictionary<Location, TInfo>, int>();
        Lookup = new Dictionary<int, Dictionary<Location, TInfo>>();

        foreach ((var key, var candidateToInfo) in pairs)
        {
            Queue.Enqueue(candidateToInfo, _ascending ? key : -key);
            Lookup.Add(key, candidateToInfo);
        }
    }

    public PriorityQueue<Dictionary<Location, TInfo>, int> Queue { get; }
    public Dictionary<int, Dictionary<Location, TInfo>> Lookup { get; }

    public void RemoveCandidate(Location location, int oldKey)
    {
        Lookup[oldKey].Remove(location);
    }

    public void MoveCandidate(Location location, TInfo info, int oldKey, int newKey)
    {
        Lookup[oldKey].Remove(location);
        if (Lookup.TryGetValue(newKey, out var batches))
        {
            batches.Add(location, info);
        }
        else
        {
            batches = new Dictionary<Location, TInfo> { { location, info } };
            Lookup.Add(newKey, batches);
        }
    }
}