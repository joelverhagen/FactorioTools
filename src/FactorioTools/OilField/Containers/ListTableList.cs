using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public class ListTableList<T> : ITableList<T>
{
    private readonly List<T> _list;

    public ListTableList()
    {
        _list = new List<T>();
    }

    public ListTableList(int capacity)
    {
        _list = new List<T>(capacity);
    }

    public T this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public int Count => _list.Count;

    public void Add(T item)
    {
        _list.Add(item);
    }

    public void AddCollection(IReadOnlyCollection<T> collection)
    {
        _list.AddRange(collection);
    }

    public void AddRange(IReadOnlyTableList<T> collection)
    {
        _list.AddRange(((ListTableList<T>)collection)._list);
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    public IReadOnlyCollection<T> EnumerateItems()
    {
        return _list;
    }

    public bool Remove(T item)
    {
        return _list.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    public void RemoveRange(int index, int count)
    {
        _list.RemoveRange(index, count);
    }

    public void Reverse()
    {
        _list.Reverse();
    }

    public void Sort(Comparison<T> comparison)
    {
        _list.Sort(comparison);
    }

    public void SortRange(int index, int count, IComparer<T> comparer)
    {
        _list.Sort(index, count, comparer);
    }
}
