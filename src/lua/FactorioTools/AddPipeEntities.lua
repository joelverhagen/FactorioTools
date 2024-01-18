-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioTools
local KnapcodeOilField
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("AddPipeEntities", function (namespace)
    local Execute, Execute1
    Execute = function (context, pipes, undergroundPipes)
      Execute1(context, context.Grid, pipes, undergroundPipes, false)
    end
    Execute1 = function (context, grid, pipes, undergroundPipes, allowMultipleTerminals)
      local addedPipes = context:GetLocationSet1()
      if undergroundPipes ~= nil then
        for _, default in System.each(undergroundPipes:EnumeratePairs()) do
          local location, direction = default:Deconstruct()
          addedPipes:Add(location)
          grid:AddEntity(location, KnapcodeOilField.UndergroundPipe(grid:GetId(), direction))
        end
      end

      for _, terminals in System.each(context.CenterToTerminals:getValues()) do
        if #terminals ~= 1 and not allowMultipleTerminals then
          System.throw(KnapcodeFactorioTools.FactorioToolsException("Every pumpjack should have a single terminal selected."))
        end

        for i = 0, #terminals - 1 do
          local terminal = terminals:get(i)
          if addedPipes:Add(terminal.Terminal) then
            grid:AddEntity(terminal.Terminal, KnapcodeOilField.Terminal(grid:GetId()))
          end
        end
      end

      for _, pipe in System.each(pipes:EnumerateItems()) do
        if addedPipes:Add(pipe) then
          grid:AddEntity(pipe, KnapcodeOilField.Pipe(grid:GetId()))
        end
      end
    end
    return {
      Execute = Execute,
      Execute1 = Execute1
    }
  end)
end)
