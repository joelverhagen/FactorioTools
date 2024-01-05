using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class SharedInstances
{
    private readonly SquareGrid _grid;

    public SharedInstances(SquareGrid grid)
    {
        _grid = grid;
#if USE_SHARED_INSTANCES
#if USE_HASHSETS
        LocationSetA = new LocationSet();
        LocationSetB = new LocationSet();
#else
        LocationSetA = new LocationSet(grid.Width, grid.Height);
        LocationSetB = new LocationSet(grid.Width, grid.Height);
#endif
#endif
    }

#if USE_SHARED_INSTANCES
    public Queue<Location> LocationQueue = new();
    public Location[] LocationArray = Array.Empty<Location>();
    public int[] IntArrayX = Array.Empty<int>();
    public int[] IntArrayY = Array.Empty<int>();
    public Dictionary<Location, Location> LocationToLocation = new();
    public Dictionary<Location, double> LocationToDouble = new();
    public PriorityQueue<Location, double> LocationPriorityQueue = new();
    public List<Location> LocationListA = new();
    public List<Location> LocationListB = new();
    public LocationSet LocationSetA;
    public LocationSet LocationSetB;

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