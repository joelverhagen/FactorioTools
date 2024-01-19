namespace Knapcode.FactorioTools.OilField;

public static class TableList
{
    public static ITableList<T> New<T>()
    {
#if USE_ARRAY
        return new ListTableList<T>();
#else
        return new DictionaryTableList<T>();
#endif
    }

    public static ITableList<T> New<T>(int capacity)
    {
#if USE_ARRAY
        return new ListTableList<T>(capacity);
#else
        return new DictionaryTableList<T>(capacity);
#endif
    }

    public static ITableList<T> New<T>(T item)
    {
        var list = New<T>();
        list.Add(item);
        return list;
    }

    public static IReadOnlyTableList<T> Empty<T>()
    {
        return EmptyInstances<T>.Instance;
    }

    private static class EmptyInstances<T>
    {
        public static IReadOnlyTableList<T> Instance { get; } = New<T>(capacity: 0);
    }
}
