-- Generated by CSharp.lua Compiler
local System = System
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  -- <summary>
  -- An entity (e.g. a pumpjack) that receives the effect of a provider entity (e.g. electric pole, beacon).
  -- </summary>
  namespace.class("ProviderRecipient", function (namespace)
    local __ctor__
    __ctor__ = function (this, center, width, height)
      this.Center = center
      this.Width = width
      this.Height = height
    end
    return {
      Width = 0,
      Height = 0,
      __ctor__ = __ctor__
    }
  end)
end)