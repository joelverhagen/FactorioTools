namespace Knapcode.FactorioTools.OilField;

public static class TableArray
{
    public static ITableArray<T> New<T>()
    {
#if USE_ARRAY
        return new ListTableArray<T>();
#else
        return new DictionaryTableArray<T>();
#endif
    }

    public static ITableArray<T> New<T>(int capacity)
    {
#if USE_ARRAY
        return new ListTableArray<T>(capacity);
#else
        return new DictionaryTableArray<T>(capacity);
#endif
    }

    public static ITableArray<T> New<T>(T item)
    {
        var list = New<T>();
        list.Add(item);
        return list;
    }

    public static IReadOnlyTableArray<T> Empty<T>()
    {
        return EmptyInstances<T>.Instance;
    }

    private static class EmptyInstances<T>
    {
        public static IReadOnlyTableArray<T> Instance { get; } = New<T>(capacity: 0);
    }
}
