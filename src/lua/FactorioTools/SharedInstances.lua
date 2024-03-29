-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeOilField
local ArrayLocation
local QueueArrayLocation
System.import(function (out)
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  ArrayLocation = System.Array(KnapcodeOilField.Location)
  QueueArrayLocation = System.Queue(ArrayLocation)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("SharedInstances", function (namespace)
    local GetNeighborArray, ReturnNeighborArray, __ctor__
    __ctor__ = function (this, grid)
      this._neighborArrays = QueueArrayLocation()
    end
    GetNeighborArray = function (this)
      if #this._neighborArrays > 0 then
        return this._neighborArrays:Dequeue()
      end

      return ArrayLocation(4)
    end
    ReturnNeighborArray = function (this, array)
      this._neighborArrays:Enqueue(array)
    end
    return {
      GetNeighborArray = GetNeighborArray,
      ReturnNeighborArray = ReturnNeighborArray,
      __ctor__ = __ctor__
    }
  end)
end)
