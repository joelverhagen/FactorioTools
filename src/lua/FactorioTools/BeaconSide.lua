-- Generated by CSharp.lua Compiler
local System = System
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("BeaconSide", function (namespace)
    local getLabel, __ctor__
    __ctor__ = function (this, id, center)
      System.base(this).__ctor__(this, id)
      this.Center = center
    end
    getLabel = function (this)
      return "b"
    end
    return {
      base = function (out)
        return {
          out.Knapcode.FactorioTools.OilField.GridEntity
        }
      end,
      getLabel = getLabel,
      __ctor__ = __ctor__
    }
  end)
end)