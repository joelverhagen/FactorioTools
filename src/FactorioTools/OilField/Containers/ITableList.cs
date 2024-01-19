﻿using System;
using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public interface ITableList<T> : IReadOnlyTableList<T>
{
    new T this[int index] { get; set; }

    void Add(T item);
    void AddCollection(IReadOnlyCollection<T> collection);
    void AddRange(IReadOnlyTableList<T> collection);
    void Clear();
    bool Remove(T item);
    void RemoveAt(int index);
    void RemoveRange(int index, int count);
    void Reverse();
    void Sort(Comparison<T> comparison);
    void SortRange(int index, int count, IComparer<T> comparer);
}
