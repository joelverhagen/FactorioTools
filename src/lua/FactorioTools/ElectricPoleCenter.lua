-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeOilField
local HashSetElectricPoleCenter
System.import(function (out)
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  HashSetElectricPoleCenter = System.HashSet(KnapcodeOilField.ElectricPoleCenter)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("ElectricPoleCenter", function (namespace)
    local getLabel, getNeighbors, AddNeighbor, ClearNeighbors, Unlink, __ctor__
    __ctor__ = function (this, id)
      this._neighbors = HashSetElectricPoleCenter()
      System.base(this).__ctor__(this, id)
    end
    getLabel = function (this)
      return "E"
    end
    getNeighbors = function (this)
      return this._neighbors
    end
    AddNeighbor = function (this, neighbor)
      this._neighbors:Add(neighbor)
      neighbor._neighbors:Add(this)
    end
    ClearNeighbors = function (this)
      for _, neighbor in System.each(this._neighbors) do
        neighbor._neighbors:Remove(this)
      end

      this._neighbors:Clear()
    end
    Unlink = function (this)
      ClearNeighbors(this)
    end
    return {
      base = function (out)
        return {
          out.Knapcode.FactorioTools.OilField.GridEntity
        }
      end,
      getLabel = getLabel,
      getNeighbors = getNeighbors,
      AddNeighbor = AddNeighbor,
      ClearNeighbors = ClearNeighbors,
      Unlink = Unlink,
      __ctor__ = __ctor__
    }
  end)
end)
