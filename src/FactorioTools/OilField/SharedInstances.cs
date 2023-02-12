namespace Knapcode.FactorioTools.OilField;

internal class SharedInstances
{
#if USE_SHARED_INSTANCES
    public required Queue<Location> LocationQueue;
    public required Location[] LocationArray;
    public required int[] IntArrayX;
    public required int[] IntArrayY;
    public required Dictionary<Location, Location> LocationToLocation;
    public required Dictionary<Location, double> LocationToDouble;
    public required PriorityQueue<Location, double> LocationPriorityQueue;
    public required List<Location> LocationListA;
    public required List<Location> LocationListB;
    public required HashSet<Location> LocationSetA;
    public required HashSet<Location> LocationSetB;

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