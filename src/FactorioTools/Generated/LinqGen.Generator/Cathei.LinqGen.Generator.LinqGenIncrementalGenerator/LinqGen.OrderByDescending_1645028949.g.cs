﻿// DO NOT EDIT
// Generated by LinqGen.Generator
#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cathei.LinqGen;
using Cathei.LinqGen.Hidden;

namespace Cathei.LinqGen.Hidden
{
    // Non-exported Enumerable should consider anonymous type, thus it will be internal
    internal struct OrderByDesc_zANKn1 : IInternalOrderedStub<global::Knapcode.FactorioTools.OilField.AddPipes.Trunk>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal OrderByDesc_zANKn1(in Gen_Prmo21 source, global::System.Func<global::Knapcode.FactorioTools.OilField.AddPipes.Trunk, int> selector_zANKn1) : this()
        {
            this.source_Prmo21 = source.source_Prmo21;
            this.selector_zANKn1 = selector_zANKn1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => this.source_Prmo21.Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ThenBy_zREfR1 ThenBy(global::System.Func<global::Knapcode.FactorioTools.OilField.AddPipes.Trunk, int> selector_zREfR1) => new ThenBy_zREfR1(this, selector_zREfR1);
        internal struct Sorter : IComparer<int>, IDisposable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Sorter(PooledListManaged<global::Knapcode.FactorioTools.OilField.AddPipes.Trunk> elements, global::System.Func<global::Knapcode.FactorioTools.OilField.AddPipes.Trunk, int> selector_zANKn1)
            {
                this.comparer_zANKn1 = default;
                keys_zANKn1 = new PooledListNative<int>(elements.Count);
                for (int i = 0; i < elements.Count; ++i)
                    keys_zANKn1.Add(selector_zANKn1.Invoke(elements[i]));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(int x, int y)
            {
                int result;
                result = comparer_zANKn1.Compare(keys_zANKn1[x], keys_zANKn1[y]);
                if (result != 0)
                    return -result;
                return x - y;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                keys_zANKn1.Dispose();
            }

            public void PartialQuickSort(DynamicArrayNative<int> indexesToSort, int left, int right, int min, int max)
            {
                do
                {
                    int mid = PartitionHoare(indexesToSort, left, right);
                    if (left < mid && mid >= min)
                        PartialQuickSort(indexesToSort, left, mid, min, max);
                    left = mid + 1;
                }
                while (left < right && left <= max);
            }

            // Hoare partition scheme
            // This implementation is faster when using struct comparer (more comparison and less copy)
            private int PartitionHoare(DynamicArrayNative<int> indexesToSort, int left, int right)
            {
                // preventing overflow of the pivot
                int pivot = left + ((right - left) >> 1);
                int pivotIndex = indexesToSort[pivot];
                int i = left - 1;
                int j = right + 1;
                while (true)
                {
                    // Move the left index to the right at least once and while the element at
                    // the left index is less than the pivot
                    while (Compare(indexesToSort[++i], pivotIndex) < 0)
                        ;
                    // Move the right index to the left at least once and while the element at
                    // the right index is greater than the pivot
                    while (Compare(indexesToSort[--j], pivotIndex) > 0)
                        ;
                    // If the indices crossed, return
                    if (i >= j)
                        return j;
                    // Swap the elements at the left and right indices
                    (indexesToSort[i], indexesToSort[j]) = (indexesToSort[j], indexesToSort[i]);
                }
            }

            private ValueCompareToComparer<int> comparer_zANKn1;
            private PooledListNative<int> keys_zANKn1;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal global::System.Collections.Generic.List<global::Knapcode.FactorioTools.OilField.AddPipes.Trunk> source_Prmo21;
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal global::System.Func<global::Knapcode.FactorioTools.OilField.AddPipes.Trunk, int> selector_zANKn1;
    }
}

namespace Cathei.LinqGen
{
}