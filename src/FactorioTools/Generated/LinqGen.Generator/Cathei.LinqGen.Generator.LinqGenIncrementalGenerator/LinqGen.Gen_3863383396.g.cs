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
    internal struct Gen_QVMSD4 : IInternalStub<global::Knapcode.FactorioTools.OilField.TerminalLocation>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Gen_QVMSD4(global::System.Collections.Generic.List<global::Knapcode.FactorioTools.OilField.TerminalLocation> source_QVMSD4) : this()
        {
            this.source_QVMSD4 = source_QVMSD4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => this.source_QVMSD4.Count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Select_dBHvI2 Select(Func<global::Knapcode.FactorioTools.OilField.TerminalLocation, global:: < anonymous  type :  global :: Knapcode . FactorioTools . OilField . TerminalLocation  Terminal ,  global :: System . Collections . Generic . List < global :: Knapcode . FactorioTools . OilField . Location > Path ,  double  ChildCentroidDistanceSquared >> selector_dBHvI2) => new Select_dBHvI2(this, selector_dBHvI2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Select_jgyIl2 Select(Func<global::Knapcode.FactorioTools.OilField.TerminalLocation, global:: < anonymous  type :  global :: Knapcode . FactorioTools . OilField . TerminalLocation  Terminal ,  global :: < anonymous  type :  global :: Knapcode . FactorioTools . OilField . TerminalLocation  Terminal ,  global :: System . Collections . Generic . List < global :: Knapcode . FactorioTools . OilField . Location > Path > BestTerminal >> selector_jgyIl2) => new Select_jgyIl2(this, selector_jgyIl2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public global::Knapcode.FactorioTools.OilField.TerminalLocation Last()
        {
            var copy = this;
            int index_QVMSD4 = default;
            index_QVMSD4 = Count() - 1 - 1;
            bool isSet_MvIBk3 = false;
            global::Knapcode.FactorioTools.OilField.TerminalLocation result_MvIBk3 = default;
            while (true)
            {
                if ((uint)++index_QVMSD4 >= (uint)copy.source_QVMSD4.Count)
                    break;
                var current_QVMSD4 = copy.source_QVMSD4[index_QVMSD4];
                isSet_MvIBk3 = true;
                result_MvIBk3 = current_QVMSD4;
            }

            if (!isSet_MvIBk3)
                ExceptionUtils.ThrowInvalidOperation();
            return result_MvIBk3;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal global::System.Collections.Generic.List<global::Knapcode.FactorioTools.OilField.TerminalLocation> source_QVMSD4;
    }
}

namespace Cathei.LinqGen
{
    // Extension class needs to be internal to prevent ambiguous resolution
    internal static partial class LinqGenExtensions_Gen_QVMSD4
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Gen_QVMSD4 Gen(this global::System.Collections.Generic.List<global::Knapcode.FactorioTools.OilField.TerminalLocation> source_QVMSD4) => new Gen_QVMSD4(source_QVMSD4);
    }
}