-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioToolsData
System.import(function (out)
  KnapcodeFactorioToolsData = Knapcode.FactorioTools.Data
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("TerminalLocation", function (namespace)
    local ToString, __ctor__
    __ctor__ = function (this, center, terminal, direction)
      this.Center = center
      this.Terminal = terminal
      this.Direction = direction
    end
    ToString = function (this)
      return "Pump " .. System.toString(this.Center, "M") .. " " .. System.EnumToString(this.Direction, KnapcodeFactorioToolsData.Direction) .. " terminal (" .. System.toString(this.Terminal, "M") .. ")"
    end
    return {
      Direction = 0,
      ToString = ToString,
      __ctor__ = __ctor__
    }
  end)
end)
