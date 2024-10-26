-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioTools
local KnapcodeFactorioToolsData
local KnapcodeOilField
local ArrayIcon
local ListEntity
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeFactorioToolsData = Knapcode.FactorioTools.Data
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  ArrayIcon = System.Array(KnapcodeFactorioToolsData.Icon)
  ListEntity = System.List(KnapcodeFactorioToolsData.Entity)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("CleanBlueprint", function (namespace)
    local Execute
    Execute = function (blueprint)
      local context = KnapcodeOilField.InitializeContext.Execute(KnapcodeOilField.OilFieldOptions(), blueprint, System.Array.Empty(KnapcodeOilField.AvoidLocation))

      local entities = ListEntity()

      for _, center in System.each(context.Centers) do
        -- Pumpjacks are given a direction that doesn't overlap with another pumpjack, preferring the direction
        -- starting at the top then proceeding clockwise.
        local terminal = KnapcodeFactorioTools.CollectionExtensions.MinBy(context.CenterToTerminals:get(center), function (x)
          return x.Direction
        end, KnapcodeOilField.TerminalLocation, System.Int32)

        local default = KnapcodeFactorioToolsData.Entity()
        default.EntityNumber = #entities + 1
        default.Direction = terminal.Direction
        default.Name = "pumpjack" --[[Vanilla.Pumpjack]]
        local extern = KnapcodeFactorioToolsData.Position()
        extern.X = center.X
        extern.Y = center.Y
        default.Position = extern
        entities:Add(default)
      end

      local default = KnapcodeFactorioToolsData.Blueprint()
      default.Entities = entities:ToArray()
      local extern = KnapcodeFactorioToolsData.Icon()
      extern.Index = 1
      local ref = KnapcodeFactorioToolsData.SignalID()
      ref.Name = "pumpjack" --[[Vanilla.Pumpjack]]
      ref.Type = "item" --[[Vanilla.Item]]
      extern.Signal = ref
      default.Icons = ArrayIcon(1, { extern })
      default.Item = "blueprint" --[[Vanilla.Blueprint]]
      default.Version = 0
      return default
    end
    return {
      Execute = Execute
    }
  end)
end)