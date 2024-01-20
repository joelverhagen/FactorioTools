-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioTools
local KnapcodeOilField
local ListLocation
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  ListLocation = System.List(KnapcodeOilField.Location)
end)
System.namespace("Knapcode.FactorioTools", function (namespace)
  namespace.class("CollectionExtensions", function (namespace)
    local ToDictionary, Distinct, ToSet, ToReadOnlySet, ToReadOnlySet1, MaxBy, Max, MinBy, 
    Min, ToList, Single, Single1, First, First1, FirstOrDefault, Average, 
    SequenceEqual, Sum
    ToDictionary = function (items, context, keySelector, valueSelector, TItem, TValue)
      local dictionary = context:GetLocationDictionary1(items:getCount(), TValue)
      for _, item in System.each(items) do
        dictionary:Add(keySelector(item, TItem), valueSelector(item, TItem, TValue))
      end

      return dictionary
    end
    Distinct = function (locations, context)
      local set = context:GetLocationSet5(locations:getCount())
      local output = ListLocation(locations:getCount())
      for _, location in System.each(locations) do
        if set:Add(location) then
          output:Add(location)
        end
      end
      return output
    end
    ToSet = function (locations, context, allowEnumerate)
      return context:GetLocationSet10(locations, allowEnumerate)
    end
    ToReadOnlySet = function (locations, context)
      return context:GetReadOnlyLocationSet(locations)
    end
    ToReadOnlySet1 = function (locations, context, allowEnumerate)
      return context:GetReadOnlyLocationSet1(locations, allowEnumerate)
    end
    MaxBy = function (source, keySelector, TSource, TKey)
      local maxKey = System.default(TKey)
      local max = System.default(TSource)
      local hasItem = false
      local comparer = System.Comparer_1(TKey).getDefault()
      for _, item in System.each(source) do
        local key = keySelector(item, TSource, TKey)
        if hasItem then
          if comparer:Compare(key, maxKey) > 0 then
            maxKey = key
            max = item
          end
        else
          maxKey = key
          max = item
          hasItem = true
        end
      end

      return max
    end
    Max = function (source, selector, TSource, TResult)
      local max = System.default(TResult)
      local hasItem = false
      local comparer = System.Comparer_1(TResult).getDefault()
      for _, item in System.each(source) do
        local cmp = selector(item, TSource, TResult)
        if hasItem then
          if comparer:Compare(cmp, max) > 0 then
            max = cmp
          end
        else
          max = cmp
          hasItem = true
        end
      end

      return max
    end
    MinBy = function (source, keySelector, TSource, TKey)
      local minKey = System.default(TKey)
      local min = System.default(TSource)
      local hasItem = false
      local comparer = System.Comparer_1(TKey).getDefault()
      for _, item in System.each(source) do
        local key = keySelector(item, TSource, TKey)
        if hasItem then
          if comparer:Compare(key, minKey) < 0 then
            minKey = key
            min = item
          end
        else
          minKey = key
          min = item
          hasItem = true
        end
      end

      return min
    end
    Min = function (source, selector, TSource, TResult)
      local min = System.default(TResult)
      local hasItem = false
      local comparer = System.Comparer_1(TResult).getDefault()
      for _, item in System.each(source) do
        local cmp = selector(item, TSource, TResult)
        if hasItem then
          if comparer:Compare(cmp, min) < 0 then
            min = cmp
          end
        else
          min = cmp
          hasItem = true
        end
      end

      return min
    end
    ToList = function (source, TSource)
      local output = System.List(TSource)(source:getCount())
      output:AddRange(source)
      return output
    end
    Single = function (source, TSource)
      local single = System.default(TSource)
      local hasItem = false
      for _, item in System.each(source) do
        if hasItem then
          System.throw(KnapcodeFactorioTools.FactorioToolsException("Only one item should exist in the source."))
        else
          single = item
          hasItem = true
        end
      end

      if hasItem then
        return single
      end

      System.throw(KnapcodeFactorioTools.FactorioToolsException("An item should exist in the source."))
    end
    Single1 = function (source, predicate, TSource)
      local single = System.default(TSource)
      local hasItem = false
      for _, item in System.each(source) do
        if predicate(item, TSource) then
          if hasItem then
            System.throw(KnapcodeFactorioTools.FactorioToolsException("Only one item should have matched the predicate."))
          else
            single = item
            hasItem = true
          end
        end
      end

      if hasItem then
        return single
      end

      System.throw(KnapcodeFactorioTools.FactorioToolsException("An item should have matched the predicate."))
    end
    First = function (source, TSource)
      for _, item in System.each(source) do
        return item
      end

      System.throw(KnapcodeFactorioTools.FactorioToolsException("An item should have matched the predicate."))
    end
    First1 = function (source, predicate, TSource)
      for _, item in System.each(source) do
        if predicate(item, TSource) then
          return item
        end
      end

      System.throw(KnapcodeFactorioTools.FactorioToolsException("An item should have matched the predicate."))
    end
    FirstOrDefault = function (source, predicate, TSource)
      for i = 0, source:getCount() - 1 do
        local item = source:get(i)
        if predicate(item, TSource) then
          return item
        end
      end

      return System.default(TSource)
    end
    Average = function (source, selector, TSource)
      if source:getCount() == 0 then
        return 0
      end

      local sum = 0
      local count = 0
      for _, item in System.each(source) do
        sum = sum + selector(item, TSource)
        count = count + 1
      end

      return sum / count
    end
    SequenceEqual = function (first, second, TSource)
      if first:getCount() ~= second:getCount() then
        return false
      end

      local comparer = System.EqualityComparer(TSource).getDefault()

      for i = 0, first:getCount() - 1 do
        if not comparer:EqualsOf(first:get(i), second:get(i)) then
          return false
        end
      end

      return true
    end
    Sum = function (source, selector, TSource)
      local sum = 0
      for i = 0, source:getCount() - 1 do
        sum = sum + selector(source:get(i), TSource)
      end

      return sum
    end
    return {
      ToDictionary = ToDictionary,
      Distinct = Distinct,
      ToSet = ToSet,
      ToReadOnlySet = ToReadOnlySet,
      ToReadOnlySet1 = ToReadOnlySet1,
      MaxBy = MaxBy,
      Max = Max,
      MinBy = MinBy,
      Min = Min,
      ToList = ToList,
      Single = Single,
      Single1 = Single1,
      First = First,
      First1 = First1,
      FirstOrDefault = FirstOrDefault,
      Average = Average,
      SequenceEqual = SequenceEqual,
      Sum = Sum
    }
  end)
end)
