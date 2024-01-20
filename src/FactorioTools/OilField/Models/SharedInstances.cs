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
#if USE_ARRAY
    public Location[] LocationArray = Array.Empty<Location>();
    public int[] IntArrayX = Array.Empty<int>();
    public int[] IntArrayY = Array.Empty<int>();
#else
    public DictionaryTableArray<Location> LocationArray = TableArray.New<Location>(0);
    public DictionaryTableArray<int> IntArrayX = TableArray.New<int>(0);
    public DictionaryTableArray<int> IntArrayY = TableArray.New<int>(0);
#endif
    public ILocationDictionary<Location> LocationToLocation;
    public ILocationDictionary<double> LocationToDouble;
    public PriorityQueue<Location, double> LocationPriorityQueue = new();
    public ITableList<Location> LocationListA = TableList.New<Location>();
    public ITableList<Location> LocationListB = TableList.New<Location>();
    public ILocationSet LocationSetA;
    public ILocationSet LocationSetB;

#if USE_ARRAY
    public T[] GetArray<T>(ref T[] array, int length)
    {
        if (array.Length < length)
        {
            array = new T[length];
        }

        return array;
    }
#else
    public DictionaryTableArray<T> GetArray<T>(ref DictionaryTableArray<T> array, int length)
    {
        array.Resize(array.Length);
        return array;
    }
#endif
#endif
}