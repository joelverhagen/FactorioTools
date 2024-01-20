-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioTools
local KnapcodeOilField
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("BeaconPlannerResult", function (namespace)
    local __members__, __ctor__
    __ctor__ = function (this, Beacons, Effects)
      this.Beacons = Beacons
      this.Effects = Effects
    end
    __members__ = function ()
      return "BeaconPlannerResult", "Beacons", "Effects"
    end
    return {
      Effects = 0,
      base = function (out)
        return {
          System.RecordType,
          System.IEquatable_1(out.Knapcode.FactorioTools.OilField.BeaconPlannerResult)
        }
      end,
      __members__ = __members__,
      __ctor__ = __ctor__
    }
  end)

  namespace.class("PlanBeacons", function (namespace)
    local Execute
    Execute = function (context, pipes)
      for _, pipe in System.each(pipes:EnumerateItems()) do
        context.Grid:AddEntity(pipe, KnapcodeOilField.TemporaryEntity(context.Grid:GetId()))
      end

      local solutions = KnapcodeOilField.TableArray.New1(context.Options.BeaconStrategies:getCount(), KnapcodeOilField.BeaconSolution)

      local completedStrategies = System.new(KnapcodeOilField.CustomCountedBitArray, 2, 3 --[[(int)BeaconStrategy.Snug + 1]])
      -- max value
      for i = 0, context.Options.BeaconStrategies:getCount() - 1 do
        local continue
        repeat
          local strategy = context.Options.BeaconStrategies:get(i)

          if completedStrategies:get(strategy) then
            continue = true
            break
          end

          local default
          local extern = strategy
          if extern == 0 --[[BeaconStrategy.FbeOriginal]] then
            default = KnapcodeOilField.PlanBeaconsFbe.Execute(context, strategy)
          elseif extern == 1 --[[BeaconStrategy.Fbe]] then
            default = KnapcodeOilField.PlanBeaconsFbe.Execute(context, strategy)
          elseif extern == 2 --[[BeaconStrategy.Snug]] then
            default = KnapcodeOilField.PlanBeaconsSnug.Execute(context)
          else
            default = System.throw(System.NotImplementedException())
          end
          local beacons, effects = default:Deconstruct()

          completedStrategies:set(strategy, true)

          solutions:Add(KnapcodeOilField.BeaconSolution(strategy, beacons, effects))
          continue = true
        until 1
        if not continue then
          break
        end
      end

      for _, pipe in System.each(pipes:EnumerateItems()) do
        context.Grid:RemoveEntity(pipe)
      end

      if solutions:getCount() == 0 then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("At least one beacon strategy must be used."))
      end

      KnapcodeOilField.Validate.BeaconsDoNotOverlap(context, solutions)

      return solutions
    end
    return {
      Execute = Execute
    }
  end)
end)
