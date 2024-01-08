using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class SharedInstances
{
    public SharedInstances(SquareGrid grid)
    {
#if USE_SHARED_INSTANCES
#if USE_HASHSETS
        LocationSetA = new LocationHashSet(grid.Width, grid.Height);
        LocationSetB = new LocationHashSet(grid.Width, grid.Height);
        LocationToLocation = new LocationHashDictionary<Location>(grid.Width, grid.Height);
        LocationToDouble = new LocationHashDictionary<double>(grid.Width, grid.Height);
#else
        LocationSetA = new LocationIntSet(grid.Width, grid.Height);
        LocationSetB = new LocationIntSet(grid.Width, grid.Height);
        LocationToLocation = new LocationIntDictionary<Location>(grid.Width, grid.Height);
        LocationToDouble = new LocationIntDictionary<double>(grid.Width, grid.Height);
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
    public List<Location> LocationListA = new();
    public List<Location> LocationListB = new();
    public ILocationSet LocationSetA;
    public ILocationSet LocationSetB;

    public T[] GetArray<T>(ref T[] array, int length) where T : struct
    {
        if (array.Length < length)
        {
            array = new T[length];
        }

        return array;
    }
#endif
}