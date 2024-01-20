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

#if USE_SHARED_INSTANCES
    public Queue<Location> LocationQueue = new();
    public Location[] LocationArray = Array.Empty<Location>();
    public int[] IntArrayX = Array.Empty<int>();
    public int[] IntArrayY = Array.Empty<int>();
    public ILocationDictionary<Location> LocationToLocation;
    public ILocationDictionary<double> LocationToDouble;
    public PriorityQueue<Location, double> LocationPriorityQueue = new();
    public ITableArray<Location> LocationListA = TableArray.New<Location>();
    public ITableArray<Location> LocationListB = TableArray.New<Location>();
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