-- Generated by CSharp.lua Compiler
local System = System
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("SortedBatches_1", function (namespace)
    return function (TInfo)
      local RemoveCandidate, MoveCandidate, __ctor__
      local ILocationDictionary_1TInfo = Knapcode.FactorioTools.OilField.ILocationDictionary_1(TInfo)
      local DictInt32ILocationDictionary_1TInfo = System.Dictionary(System.Int32, ILocationDictionary_1TInfo)
      local PriorityQueueILocationDictionary_1TInfoInt32 = System.PriorityQueue(ILocationDictionary_1TInfo, System.Int32)
      __ctor__ = function (this, pairs, ascending)
        this._ascending = ascending
        this.Queue = PriorityQueueILocationDictionary_1TInfoInt32()
        this.Lookup = DictInt32ILocationDictionary_1TInfo()

        for _, default in System.each(pairs) do
          local key, candidateToInfo = default:Deconstruct()
          this.Queue:Enqueue(candidateToInfo, this._ascending and key or - key)
          this.Lookup:AddKeyValue(key, candidateToInfo)
        end
      end
      RemoveCandidate = function (this, location, oldKey)
        this.Lookup:get(oldKey):Remove(location)
      end
      MoveCandidate = function (this, context, location, info, oldKey, newKey)
        this.Lookup:get(oldKey):Remove(location)
        local default, batches = this.Lookup:TryGetValue(newKey, nil)
        if default then
          batches:Add(location, info)
        else
          batches = context:GetLocationDictionary(TInfo)
          batches:Add(location, info)
          this.Lookup:AddKeyValue(newKey, batches)
        end
      end
      return {
        _ascending = false,
        RemoveCandidate = RemoveCandidate,
        MoveCandidate = MoveCandidate,
        __ctor__ = __ctor__
      }
    end
  end)
end)