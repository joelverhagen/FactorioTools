using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class SharedInstances
{
    public SharedInstances(SquareGrid grid)
    {
#if USE_SHARED_INSTANCES
#if USE_HASHSETS
        LocationSetA = new LocationHashSet();
        LocationSetB = new LocationHashSet();
        LocationToLocation = new LocationHashDictionary<Location>();
        LocationToDouble = new LocationHashDictionary<double>();
#else
        LocationSetA = new LocationIntSet(grid.Width);
        LocationSetB = new LocationIntSet(grid.Width);
        LocationToLocation = new LocationIntDictionary<Location>(grid.Width);
        LocationToDouble = new LocationIntDictionary<double>(grid.Width);
#endif
#endif
    }

#if RENT_NEIGHBORS
    private Queue<Location[]> _neighborArrays = new Queue<Location[]>();

    public Location[] GetNeighborArray()
    {
        if (_neighborArrays.Count > 0)
        {
            return _neighborArrays.Dequeue();
        }

        return new Location[4];
    }

    public void ReturnNeighborArray(Location[] array)
    {
        _neighborArrays.Enqueue(array);
    }
#endif

#if USE_SHARED_INSTANCES
    public Queue<Location> LocationQueue = new();
    public Location[] LocationArray = Array.Empty<Location>();
    public int[] IntArrayX = Array.Empty<int>();
    public int[] IntArrayY = Array.Empty<int>();
    public ILocationDictionary<Location> LocationToLocation;
    public ILocationDictionary<double> LocationToDouble;
    public PriorityQueue<Location, double> LocationPriorityQueue = new();
    public List<Location> LocationListA = new();
    public List<Location> LocationListB = new();
    public ILocationSet LocationSetA;
    public ILocationSet LocationSetB;

    public T[] GetArray<T>(ref T[] array, int length)
    {
        if (array.Length < length)
        {
            array = new T[length];
        }

        return array;
    }
#endif
}