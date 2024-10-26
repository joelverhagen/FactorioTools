-- Generated by CSharp.lua Compiler
local System = System
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  -- <summary>
  -- A particular attempt oil field plan.
  -- </summary>
  -- <param name="PipeStrategy">The pipe strategy used to generate the plan.</param>
  -- <param name="OptimizePipes">Whether or not the pipe optimized was used.</param>
  -- <param name="BeaconStrategy">Which beacon strategy, if any, was used.</param>
  -- <param name="BeaconEffectCount">The number of effects the beacons provided to pumpjacks. Higher is better.</param>
  -- <param name="BeaconCount">The number of beacons in the plan. For the same number of beacon effects, lower is better.</param>
  -- <param name="PipeCount">The number of pipes in the plan. For the same number of beacon effects and beacons, lower is better. If underground pipes are used, this only counts the upwards and downwards connections for the underground stretches of pipes.</param>
  -- <param name="PipeCountWithoutUnderground">The number of pipes before beacons or underground pipes are placed.</param>
  namespace.class("OilFieldPlan", function (namespace)
    local IsEquivalent, ToString, ToString1, __members__, __ctor__
    __ctor__ = function (this, PipeStrategy, OptimizePipes, BeaconStrategy, BeaconEffectCount, BeaconCount, PipeCount, PipeCountWithoutUnderground)
      this.PipeStrategy = PipeStrategy
      this.OptimizePipes = OptimizePipes
      this.BeaconStrategy = BeaconStrategy
      this.BeaconEffectCount = BeaconEffectCount
      this.BeaconCount = BeaconCount
      this.PipeCount = PipeCount
      this.PipeCountWithoutUnderground = PipeCountWithoutUnderground
    end
    IsEquivalent = function (this, other)
      return this.BeaconEffectCount == other.BeaconEffectCount and this.BeaconCount == other.BeaconCount and this.PipeCount == other.PipeCount
    end
    ToString = function (this)
      return ToString1(this, false)
    end
    ToString1 = function (this, includeCounts)
      local default
      local extern = this.PipeStrategy
      if extern == 0 --[[PipeStrategy.FbeOriginal]] then
        default = "FBE"
      elseif extern == 1 --[[PipeStrategy.Fbe]] then
        default = "FBE*"
      elseif extern == 2 --[[PipeStrategy.ConnectedCentersDelaunay]] then
        default = "CC-DT"
      elseif extern == 3 --[[PipeStrategy.ConnectedCentersDelaunayMst]] then
        default = "CC-DT-MST"
      elseif extern == 4 --[[PipeStrategy.ConnectedCentersFlute]] then
        default = "CC-FLUTE"
      else
        default = System.throw(System.NotImplementedException())
      end
      local output = default

      if this.OptimizePipes then
        output = System.toString(output) .. " -> optimize"
      end

      if (this.BeaconStrategy ~= nil) then
        local ref
        local out = System.Nullable.Value(this.BeaconStrategy)
        if out == 0 --[[BeaconStrategy.FbeOriginal]] then
          ref = " -> FBE"
        elseif out == 1 --[[BeaconStrategy.Fbe]] then
          ref = " -> FBE*"
        elseif out == 2 --[[BeaconStrategy.Snug]] then
          ref = " -> snug"
        else
          ref = System.throw(System.NotImplementedException())
        end
        output = System.toString(output) .. System.toString(ref)
      end

      if includeCounts then
        if (this.BeaconStrategy ~= nil) then
          output = System.toString(output) .. System.toString(" (effects: " .. this.BeaconEffectCount .. ", beacons: " .. this.BeaconCount .. ", pipes: " .. this.PipeCount .. ")")
        else
          output = System.toString(output) .. System.toString(" (pipes: " .. this.PipeCount .. ")")
        end
      end

      return output
    end
    __members__ = function ()
      return "OilFieldPlan", "PipeStrategy", "OptimizePipes", "BeaconStrategy", "BeaconEffectCount", "BeaconCount", "PipeCount", "PipeCountWithoutUnderground"
    end
    return {
      PipeStrategy = 0,
      OptimizePipes = false,
      BeaconEffectCount = 0,
      BeaconCount = 0,
      PipeCount = 0,
      PipeCountWithoutUnderground = 0,
      IsEquivalent = IsEquivalent,
      ToString = ToString,
      ToString1 = ToString1,
      base = function (out)
        return {
          System.RecordType,
          System.IEquatable_1(out.Knapcode.FactorioTools.OilField.OilFieldPlan)
        }
      end,
      __members__ = __members__,
      __ctor__ = __ctor__
    }
  end)
end)