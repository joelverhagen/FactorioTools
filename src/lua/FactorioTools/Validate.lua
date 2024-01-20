-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioTools
local KnapcodeOilField
local ListLocation
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  ListLocation = System.List(KnapcodeOilField.Location)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("Validate", function (namespace)
    local PipesAreConnected, UndergroundPipesArePipes, PipesDoNotMatch, BeaconsDoNotOverlap, NoExistingBeacons, NoOverlappingEntities, CandidateCoversMoreEntities, AllEntitiesHavePower
    PipesAreConnected = function (context, optimizedPipes)
      if context.Options.ValidateSolution then
        for _, terminals in System.each(context.CenterToTerminals:getValues()) do
          if #terminals ~= 1 then
            System.throw(KnapcodeFactorioTools.FactorioToolsException("A pumpjack has more than one terminal."))
          end
        end

        local goals = context:GetLocationSet2(true)
        for _, terminals in System.each(context.CenterToTerminals:getValues()) do
          for i = 0, #terminals - 1 do
            goals:Add(terminals:get(i).Terminal)
          end
        end

        local clone = KnapcodeOilField.ExistingPipeGrid(context.Grid, optimizedPipes)
        local start = KnapcodeFactorioTools.CollectionExtensions.First(goals:EnumerateItems(), KnapcodeOilField.Location)
        goals:Remove(start)
        local result = KnapcodeOilField.Dijkstras.GetShortestPaths(context, clone, start, goals, false, true)
        local reachedGoals = result.ReachedGoals
        reachedGoals:Add(start)
        local unreachedGoals = context:GetLocationSet(goals)
        unreachedGoals:ExceptWith(reachedGoals)
        if unreachedGoals:getCount() > 0 then
          -- Visualizer.Show(context.Grid, optimizedPipes.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
          System.throw(KnapcodeFactorioTools.FactorioToolsException("The pipes are not fully connected."))
        end
      end
    end
    UndergroundPipesArePipes = function (context, pipes, locationToDirection)
      if context.Options.ValidateSolution then
        local missing = KnapcodeFactorioTools.CollectionExtensions.ToSet(locationToDirection:getKeys(), context, true)
        missing:ExceptWith(pipes)

        if missing:getCount() > 0 then
          System.throw(KnapcodeFactorioTools.FactorioToolsException("The underground pipes should be in the pipe set."))
        end
      end
    end
    PipesDoNotMatch = function (context, pipes1, pipes2)
      if context.Options.ValidateSolution then
        if pipes1:SetEquals(pipes2) then
          System.throw(KnapcodeFactorioTools.FactorioToolsException("The two pipe configurations should not match."))
        end
      end
    end
    BeaconsDoNotOverlap = function (context, solutions)
      if context.Options.ValidateSolution and not context.Options.OverlapBeacons then
        for _, solution in System.each(solutions) do
          local beaconCenterToCoveredCenters = KnapcodeOilField.Helpers.GetProviderCenterToCoveredCenters(context, context.Options.BeaconWidth, context.Options.BeaconHeight, context.Options.BeaconSupplyWidth, context.Options.BeaconSupplyHeight, solution.Beacons, true, false)

          local coveredCenterToPoleCenters = KnapcodeOilField.Helpers.GetCoveredCenterToProviderCenters(context, beaconCenterToCoveredCenters)

          for _, default in System.each(coveredCenterToPoleCenters:EnumeratePairs()) do
            local pumpjackCenter, beaconCenters = default:Deconstruct()
            if beaconCenters:getCount() > 1 then
              System.throw(KnapcodeFactorioTools.FactorioToolsException("Multiple beacons are providing an effect to a pumpjack."))
            end
          end
        end
      end
    end
    NoExistingBeacons = function (context, existingBeacons)
      if context.Options.ValidateSolution and existingBeacons:getCount() > 0 then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("There should not be any existing beacons."))
      end
    end
    NoOverlappingEntities = function (context, optimizedPipes, undergroundPipes, beaconSolutions)
      if context.Options.ValidateSolution then
        if beaconSolutions == nil then
          local clone = KnapcodeOilField.PipeGrid(context.Grid)
          KnapcodeOilField.AddPipeEntities.Execute1(context, clone, optimizedPipes, undergroundPipes, false)
        else
          for _, solution in System.each(beaconSolutions) do
            local clone = KnapcodeOilField.PipeGrid(context.Grid)
            KnapcodeOilField.AddPipeEntities.Execute1(context, clone, optimizedPipes, undergroundPipes, false)
            KnapcodeOilField.Helpers.AddBeaconsToGrid(clone, context.Options, solution.Beacons)
          end
        end
      end
    end
    CandidateCoversMoreEntities = function (context, poweredEntities, coveredEntities, candidate, candidateInfo)
      if context.Options.ValidateSolution then
        local covered = candidateInfo.Covered
        local isSubsetOf = true
        do
          local i = 0
          while i < #poweredEntities and isSubsetOf do
            if covered:get(i) then
              isSubsetOf = coveredEntities:get(i)
            end
            i = i + 1
          end
        end

        if isSubsetOf then
          -- Visualizer.Show(context.Grid, new[] { candidate }.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());
          System.throw(KnapcodeFactorioTools.FactorioToolsException("Candidate " .. System.toString(candidate) .. " should have been eliminated."))
        end
      end
    end
    AllEntitiesHavePower = function (context)
      if context.Options.ValidateSolution then
        local poweredEntities
        poweredEntities, _ = KnapcodeOilField.Helpers.GetPoweredEntities(context):Deconstruct()

        local electricPoleCenters = ListLocation()
        for _, location in System.each(context.Grid:getEntityLocations():EnumerateItems()) do
          local entity = context.Grid:get(location)
          if System.is(entity, KnapcodeOilField.ElectricPoleCenter) then
            electricPoleCenters:Add(location)
          end
        end

        KnapcodeOilField.Helpers.GetElectricPoleCoverage(context, poweredEntities, electricPoleCenters)
      end
    end
    return {
      PipesAreConnected = PipesAreConnected,
      UndergroundPipesArePipes = UndergroundPipesArePipes,
      PipesDoNotMatch = PipesDoNotMatch,
      BeaconsDoNotOverlap = BeaconsDoNotOverlap,
      NoExistingBeacons = NoExistingBeacons,
      NoOverlappingEntities = NoOverlappingEntities,
      CandidateCoversMoreEntities = CandidateCoversMoreEntities,
      AllEntitiesHavePower = AllEntitiesHavePower
    }
  end)
end)
