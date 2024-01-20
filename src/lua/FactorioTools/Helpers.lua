-- Generated by CSharp.lua Compiler
local System = System
local DelaunatorSharp = DelaunatorSharp
local ArrayIPoint = System.Array(DelaunatorSharp.IPoint)
local KnapcodeFactorioTools
local KnapcodeOilField
local ListTuple
local ListLocation
local ListEndpoints
local ListTerminalLocation
local ListProviderRecipient
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  ListTuple = System.List(System.Tuple)
  ListLocation = System.List(KnapcodeOilField.Location)
  ListEndpoints = System.List(KnapcodeOilField.Endpoints)
  ListTerminalLocation = System.List(KnapcodeOilField.TerminalLocation)
  ListProviderRecipient = System.List(KnapcodeOilField.ProviderRecipient)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("Helpers", function (namespace)
    local TerminalOffsets, AddPumpjack, GetCenterToTerminals, PopulateCenterToTerminals, GetLocationToTerminals, PopulateLocationToTerminals, GetBeaconCandidateToCovered, GetElectricPoleCandidateToCovered, 
    GetCandidateToCovered, GetProviderCenterToCoveredCenters, GetCoveredCenterToProviderCenters, AddCoveredCenters, DoesProviderFit, GetEntityDistance, AddProviderAndPreventMultipleProviders, AddProviderAndAllowMultipleProviders, 
    GetElectricPoleCoverage, GetPoweredEntities, GetCoveredToCandidates, GetProviderOverlapBounds, RemoveOverlappingCandidates, RemoveOverlappingCandidates1, RemoveOverlappingCandidates2, RemoveEntity, 
    AddProviderToGrid, AddBeaconsToGrid, IsProviderInBounds, EliminateOtherTerminals, GetPath, AddPath, AreLocationsCollinear, CountTurns, 
    MakeStraightLineOnEmpty, MakeStraightLine, PointsToLines, PointsToLines1, ToInt, static
    static = function (this)
      local default = ListTuple()
      default:Add(System.Tuple(0 --[[Direction.Up]], KnapcodeOilField.Location(1, - 2)))
      default:Add(System.Tuple(2 --[[Direction.Right]], KnapcodeOilField.Location(2, - 1)))
      default:Add(System.Tuple(4 --[[Direction.Down]], KnapcodeOilField.Location(- 1, 2)))
      default:Add(System.Tuple(6 --[[Direction.Left]], KnapcodeOilField.Location(- 2, 1)))
      TerminalOffsets = default
      this.TerminalOffsets = TerminalOffsets
    end
    AddPumpjack = function (grid, center)
      local centerEntity = KnapcodeOilField.PumpjackCenter(grid:GetId())
      for x = - 1, 1 do
        for y = - 1, 1 do
          local entity = (x ~= 0 or y ~= 0) and KnapcodeOilField.PumpjackSide(grid:GetId(), centerEntity) or centerEntity
          grid:AddEntity(KnapcodeOilField.Location(center.X + x, center.Y + y), entity)
        end
      end

      return centerEntity
    end
    GetCenterToTerminals = function (context, grid, centers)
      local centerToTerminals = context:GetLocationDictionary(ListTerminalLocation)
      PopulateCenterToTerminals(centerToTerminals, grid, centers)
      return centerToTerminals
    end
    PopulateCenterToTerminals = function (centerToTerminals, grid, centers)
      for _, center in System.each(centers) do
        local candidateTerminals = ListTerminalLocation()
        for _, default in System.each(TerminalOffsets) do
          local direction, translation = default:Deconstruct()
          local location = center:Translate1(translation)
          local terminal = KnapcodeOilField.TerminalLocation(center, location, direction)
          local existing = grid:get(location)
          if existing == nil or System.is(existing, KnapcodeOilField.Pipe) then
            candidateTerminals:Add(terminal)
          end
        end

        if #candidateTerminals == 0 then
          System.throw(System.new(KnapcodeFactorioTools.FactorioToolsException, 2, "At least one pumpjack has no room for a pipe connection. Try removing some pumpjacks.", true))
        end

        centerToTerminals:Add(center, candidateTerminals)
      end
    end
    GetLocationToTerminals = function (context, centerToTerminals)
      local locationToTerminals = context:GetLocationDictionary(ListTerminalLocation)
      PopulateLocationToTerminals(locationToTerminals, centerToTerminals)
      return locationToTerminals
    end
    PopulateLocationToTerminals = function (locationToTerminals, centerToTerminals)
      for _, terminals in System.each(centerToTerminals:getValues()) do
        for _, terminal in System.each(terminals) do
          local default, list = locationToTerminals:TryGetValue(terminal.Terminal)
          if not default then
            list = ListTerminalLocation(2)
            locationToTerminals:Add(terminal.Terminal, list)
          end

          list:Add(terminal)
        end
      end
    end
    GetBeaconCandidateToCovered = function (context, recipients, candidateFactory, removeUnused, TInfo)
      return GetCandidateToCovered(context, recipients, candidateFactory, context.Options.BeaconWidth, context.Options.BeaconHeight, context.Options.BeaconSupplyWidth, context.Options.BeaconSupplyHeight, removeUnused, true, false, KnapcodeOilField.BeaconCenter, TInfo)
    end
    GetElectricPoleCandidateToCovered = function (context, recipients, candidateFactory, removeUnused, TInfo)
      return GetCandidateToCovered(context, recipients, candidateFactory, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight, context.Options.ElectricPoleSupplyWidth, context.Options.ElectricPoleSupplyHeight, removeUnused, true, true, KnapcodeOilField.ElectricPoleCenter, TInfo)
    end
    GetCandidateToCovered = function (context, recipients, candidateFactory, providerWidth, providerHeight, supplyWidth, supplyHeight, removeUnused, includePumpjacks, includeBeacons, TProvider, TInfo)
      local candidateToInfo = context:GetLocationDictionary(TInfo)
      local coveredEntities = System.new(KnapcodeOilField.CustomCountedBitArray, 2, #recipients)

      local providers = context:GetLocationDictionary(TProvider)
      for _, location in System.each(context.Grid:getEntityLocations():EnumerateItems()) do
        local continue
        repeat
          local provider = System.as(context.Grid:get(location), TProvider)
          if provider == nil then
            continue = true
            break
          end

          providers:Add(location, provider)
          continue = true
        until 1
        if not continue then
          break
        end
      end
      local unusedProviders = KnapcodeFactorioTools.CollectionExtensions.ToReadOnlySet1(providers:getKeys(), context, removeUnused)

      for i = 0, #recipients - 1 do
        local continue
        repeat
          local entity = recipients:get(i)

          local minX = math.Max(System.div((providerWidth - 1), 2), entity.Center.X - (System.div((entity.Width - 1), 2)) - (System.div(supplyWidth, 2)))
          local minY = math.Max(System.div((providerHeight - 1), 2), entity.Center.Y - (System.div((entity.Height - 1), 2)) - (System.div(supplyHeight, 2)))
          local maxX = math.Min(context.Grid.Width - (System.div(providerWidth, 2)) - 1, entity.Center.X + (System.div(entity.Width, 2)) + (System.div((supplyWidth - 1), 2)))
          local maxY = math.Min(context.Grid.Height - (System.div(providerHeight, 2)) - 1, entity.Center.Y + (System.div(entity.Height, 2)) + (System.div((supplyHeight - 1), 2)))

          -- Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

          for x = minX, maxX do
            local continue
            repeat
              for y = minY, maxY do
                local continue
                repeat
                  local candidate = KnapcodeOilField.Location(x, y)
                  if context.Grid:get(candidate) ~= nil then
                    local default, existing = providers:TryGetValue(candidate)
                    if default then
                      unusedProviders:Remove(candidate)
                      coveredEntities:set(i, true)
                    end
                  else
                    local fits = DoesProviderFit(context.Grid, providerWidth, providerHeight, candidate)
                    if not fits then
                      continue = true
                      break
                    end

                    local default, info = candidateToInfo:TryGetValue(candidate)
                    if not default then
                      local covered = System.new(KnapcodeOilField.CustomCountedBitArray, 2, #recipients)
                      covered:set(i, true)
                      info = candidateFactory:Create(covered)
                      candidateToInfo:Add(candidate, info)
                    else
                      info.Covered:set(i, true)
                    end
                  end
                  continue = true
                until 1
                if not continue then
                  break
                end
              end
              continue = true
            until 1
            if not continue then
              break
            end
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      if removeUnused and unusedProviders:getCount() > 0 then
        local coveredCenters = context:GetLocationSet1()
        for _, center in System.each(unusedProviders:EnumerateItems()) do
          local entityMinX = center.X - (System.div((providerWidth - 1), 2))
          local entityMinY = center.Y - (System.div((providerHeight - 1), 2))
          local entityMaxX = center.X + (System.div(providerWidth, 2))
          local entityMaxY = center.Y + (System.div(providerHeight, 2))

          -- Expand the loop bounds beyond the entity bounds so we can removed candidates that are not longer valid with
          -- the newly added provider, i.e. they would overlap with what was just added.
          local minX = math.Max(System.div((providerWidth - 1), 2), entityMinX - (System.div(providerWidth, 2)))
          local minY = math.Max(System.div((providerHeight - 1), 2), entityMinY - (System.div(providerHeight, 2)))
          local maxX = math.Min(context.Grid.Width - (System.div(providerWidth, 2)) - 1, entityMaxX + (System.div((providerWidth - 1), 2)))
          local maxY = math.Min(context.Grid.Height - (System.div(providerHeight, 2)) - 1, entityMaxY + (System.div((providerHeight - 1), 2)))

          for x = minX, maxX do
            for y = minY, maxY do
              local candidate = KnapcodeOilField.Location(x, y)

              if x >= entityMinX and x <= entityMaxX and y >= entityMinY and y <= entityMaxY then
                context.Grid:RemoveEntity(candidate)
              else
                AddCoveredCenters(coveredCenters, context.Grid, candidate, providerWidth, providerHeight, supplyWidth, supplyHeight, includePumpjacks, includeBeacons)

                local default, info = candidateToInfo:TryGetValue(candidate)
                if not default then
                  local covered = System.new(KnapcodeOilField.CustomCountedBitArray, 2, #recipients)
                  info = candidateFactory:Create(covered)
                  candidateToInfo:Add(candidate, info)
                end

                for i = 0, #recipients - 1 do
                  if coveredCenters:Contains(recipients:get(i).Center) then
                    info.Covered:set(i, true)
                  end
                end

                coveredCenters:Clear()
              end
            end
          end

          providers:Remove(center)
        end
      end

      if providers:getCount() > 0 or unusedProviders:getCount() > 0 then
        -- Remove candidates that only cover recipients that are already covered.
        local toRemove = ListLocation()
        for _, default in System.each(candidateToInfo:EnumeratePairs()) do
          local candidate, info = default:Deconstruct()
          local subset = KnapcodeOilField.CustomCountedBitArray(info.Covered)
          subset:Not()
          subset:Or(coveredEntities)
          if subset:All(true) then
            toRemove:Add(candidate)
          end
        end

        for i = 0, #toRemove - 1 do
          candidateToInfo:Remove(toRemove:get(i))
        end
      end

      return System.ValueTuple(candidateToInfo, coveredEntities, providers)
    end
    GetProviderCenterToCoveredCenters = function (context, providerWidth, providerHeight, supplyWidth, supplyHeight, providerCenters, includePumpjacks, includeBeacons)
      local poleCenterToCoveredCenters = context:GetLocationDictionary(KnapcodeOilField.ILocationSet)

      for _, center in System.each(providerCenters) do
        local coveredCenters = context:GetLocationSet2(true)
        AddCoveredCenters(coveredCenters, context.Grid, center, providerWidth, providerHeight, supplyWidth, supplyHeight, includePumpjacks, includeBeacons)

        poleCenterToCoveredCenters:Add(center, coveredCenters)
      end

      return poleCenterToCoveredCenters
    end
    GetCoveredCenterToProviderCenters = function (context, providerCenterToCoveredCenters)
      local output = context:GetLocationDictionary(KnapcodeOilField.ILocationSet)

      for _, default in System.each(providerCenterToCoveredCenters:EnumeratePairs()) do
        local center, covered = default:Deconstruct()
        for _, otherCenter in System.each(covered:EnumerateItems()) do
          local extern, centers = output:TryGetValue(otherCenter)
          if not extern then
            centers = context:GetLocationSet2(true)
            output:Add(otherCenter, centers)
          end

          centers:Add(center)
        end
      end

      return output
    end
    AddCoveredCenters = function (coveredCenters, grid, center, providerWidth, providerHeight, supplyWidth, supplyHeight, includePumpjacks, includeBeacons)

      local minX = math.Max(2 --[[minPoweredEntityWidth - 1]], center.X - (System.div(supplyWidth, 2)) + (System.mod((providerWidth - 1), 2)))
      local minY = math.Max(2 --[[minPoweredEntityHeight - 1]], center.Y - (System.div(supplyHeight, 2)) + (System.mod((providerHeight - 1), 2)))
      local maxX = math.Min(grid.Width - 3 --[[minPoweredEntityWidth]] + 1, center.X + (System.div(supplyWidth, 2)))
      local maxY = math.Min(grid.Height - 3 --[[minPoweredEntityHeight]] + 1, center.Y + (System.div(supplyHeight, 2)))

      for x = minX, maxX do
        for y = minY, maxY do
          local location = KnapcodeOilField.Location(x, y)

          local entity = grid:get(location)
          if includePumpjacks and System.is(entity, KnapcodeOilField.PumpjackCenter) then
            coveredCenters:Add(location)
          else
            local pumpjackSide
            local default
            if includePumpjacks then
              pumpjackSide = entity
              if System.is(pumpjackSide, KnapcodeOilField.PumpjackSide) then
                default = true
              end
            end
            if default then
              coveredCenters:Add(grid:getEntityIdToLocation():get(pumpjackSide.Center.Id))
            elseif includeBeacons and System.is(entity, KnapcodeOilField.BeaconCenter) then
              coveredCenters:Add(location)
            else
              local beaconSide
              local default
              if includeBeacons then
                beaconSide = entity
                if System.is(beaconSide, KnapcodeOilField.BeaconSide) then
                  default = true
                end
              end
              if default then
                coveredCenters:Add(grid:getEntityIdToLocation():get(beaconSide.Center.Id))
              end
            end
          end
        end
      end
    end
    -- <summary>
    -- Checks if the provider fits at the provided center location. This does NOT account for grid bounds.
    -- </summary>
    DoesProviderFit = function (grid, providerWidth, providerHeight, center)
      local minX = center.X - (System.div((providerWidth - 1), 2))
      local maxX = center.X + (System.div(providerWidth, 2))
      local minY = center.Y - (System.div((providerHeight - 1), 2))
      local maxY = center.Y + (System.div(providerHeight, 2))

      for x = minX, maxX do
        for y = minY, maxY do
          local location = KnapcodeOilField.Location(x, y)
          if not grid:IsEmpty(location) then
            return false
          end
        end
      end

      return true
    end
    GetEntityDistance = function (poweredEntities, candidate, covered)
      local sum = 0
      for i = 0, #poweredEntities - 1 do
        if covered:get(i) then
          sum = sum + candidate:GetEuclideanDistance(poweredEntities:get(i).Center)
        end
      end

      return sum
    end
    AddProviderAndPreventMultipleProviders = function (context, center, centerInfo, providerWidth, providerHeight, recipients, coveredEntities, coveredToCandidates, candidateToInfo, TInfo)
      -- Console.WriteLine("adding " + center);

      local newlyCovered = KnapcodeOilField.CustomCountedBitArray(coveredEntities)
      newlyCovered:Not()
      newlyCovered:And(centerInfo.Covered)

      if newlyCovered.TrueCount == 0 then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("At least one recipient should should have been newly covered."))
      end

      coveredEntities:Or(centerInfo.Covered)

      RemoveOverlappingCandidates1(context.Grid, center, providerWidth, providerHeight, candidateToInfo, coveredToCandidates, TInfo)


      local toRemove = ListLocation()
      local updated = context:GetLocationSet1()
      for i = 0, #recipients - 1 do
        local continue
        repeat
          if not newlyCovered:get(i) then
            continue = true
            break
          end

          for _, default in System.each(coveredToCandidates:get(i):EnumeratePairs()) do
            local otherCandidate, otherInfo = default:Deconstruct()
            local continue
            repeat
              if not updated:Add(otherCandidate) then
                continue = true
                break
              end

              toRemove:Add(otherCandidate)
              continue = true
            until 1
            if not continue then
              break
            end
          end

          if #toRemove > 0 then
            for j = 0, #toRemove - 1 do
              candidateToInfo:Remove(toRemove:get(j))
            end

            toRemove:Clear()
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end
    end
    AddProviderAndAllowMultipleProviders = function (context, center, centerInfo, providerWidth, providerHeight, recipients, coveredEntities, coveredToCandidates, candidateToInfo, scopedCandidateToInfo, coveredCountBatches, TInfo)
      -- Console.WriteLine("adding " + center);

      local newlyCovered = KnapcodeOilField.CustomCountedBitArray(coveredEntities)
      newlyCovered:Not()
      newlyCovered:And(centerInfo.Covered)

      if newlyCovered.TrueCount == 0 then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("At least one recipient should should have been newly covered."))
      end

      coveredEntities:Or(centerInfo.Covered)

      RemoveOverlappingCandidates2(context.Grid, center, providerWidth, providerHeight, candidateToInfo, scopedCandidateToInfo, coveredToCandidates, TInfo)

      if coveredEntities:All(true) then
        return
      end


      local toRemove = ListLocation()
      local updated = context:GetLocationSet1()
      -- Remove the covered entities from the candidate data, so that the next candidates are discounted
      -- by the entities that no longer need to be covered.
      for i = 0, #recipients - 1 do
        local continue
        repeat
          if not newlyCovered:get(i) then
            continue = true
            break
          end

          local currentCandidates = coveredToCandidates:get(i)
          for _, default in System.each(currentCandidates:EnumeratePairs()) do
            local otherCandidate, otherInfo = default:Deconstruct()
            local continue
            repeat
              if not updated:Add(otherCandidate) then
                continue = true
                break
              end

              local modified = false
              local oldCoveredCount = otherInfo.Covered.TrueCount
              do
                local j = 0
                while j < #recipients and otherInfo.Covered.TrueCount > 0 do
                  if coveredEntities:get(j) and otherInfo.Covered:get(j) then
                    otherInfo.Covered:set(j, false)
                    modified = true

                    -- avoid modifying the collection we are enumerating
                    if i ~= j then
                      coveredToCandidates:get(j):Remove(otherCandidate)
                    end
                  end
                  j = j + 1
                end
              end

              if otherInfo.Covered.TrueCount == 0 then
                toRemove:Add(otherCandidate)
                coveredCountBatches:RemoveCandidate(otherCandidate, oldCoveredCount)
              elseif modified then
                coveredCountBatches:MoveCandidate(context, otherCandidate, otherInfo, oldCoveredCount, otherInfo.Covered.TrueCount)

                local entityDistance = 0
                for j = 0, #recipients - 1 do
                  entityDistance = entityDistance + otherCandidate:GetEuclideanDistance(recipients:get(j).Center)
                end

                otherInfo.EntityDistance = entityDistance
              end
              continue = true
            until 1
            if not continue then
              break
            end
          end

          -- now that we're done enumerating this dictionary, we can clear it
          currentCandidates:Clear()

          if #toRemove > 0 then
            for j = 0, #toRemove - 1 do
              if candidateToInfo:Remove(toRemove:get(j)) then
                scopedCandidateToInfo:Remove(toRemove:get(j))
              end
            end

            toRemove:Clear()
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end
    end
    GetElectricPoleCoverage = function (context, poweredEntities, electricPoleCenters)
      local poleCenterToCoveredCenters = GetProviderCenterToCoveredCenters(context, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight, context.Options.ElectricPoleSupplyWidth, context.Options.ElectricPoleSupplyHeight, electricPoleCenters, true, true)

      local coveredCenterToPoleCenters = GetCoveredCenterToProviderCenters(context, poleCenterToCoveredCenters)

      if coveredCenterToPoleCenters:getCount() ~= #poweredEntities then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("Not all powered entities are covered by an electric pole."))
      end

      return System.ValueTuple(poleCenterToCoveredCenters, coveredCenterToPoleCenters)
    end
    GetPoweredEntities = function (context)
      local poweredEntities = ListProviderRecipient()
      local hasBeacons = false

      for _, location in System.each(context.Grid:getEntityLocations():EnumerateItems()) do
        local entity = context.Grid:get(location)
        if System.is(entity, KnapcodeOilField.PumpjackCenter) then
          poweredEntities:Add(KnapcodeOilField.ProviderRecipient(location, 3 --[[Helpers.PumpjackWidth]], 3 --[[Helpers.PumpjackHeight]]))
        elseif System.is(entity, KnapcodeOilField.BeaconCenter) then
          poweredEntities:Add(KnapcodeOilField.ProviderRecipient(location, context.Options.BeaconWidth, context.Options.BeaconHeight))
          hasBeacons = true
        end
      end

      -- sort the result so the above dictionary enumerator order does not impact output
      poweredEntities:Sort(function (a, b)
        local c = System.Int32.CompareTo(a.Center.Y, b.Center.Y)
        if c ~= 0 then
          return c
        end

        return System.Int32.CompareTo(a.Center.X, b.Center.X)
      end)

      return System.ValueTuple(poweredEntities, hasBeacons)
    end
    GetCoveredToCandidates = function (context, allCandidateToInfo, coveredEntities, TInfo)
      local coveredToCandidates = System.Dictionary(System.Int32, KnapcodeOilField.ILocationDictionary_1(TInfo))(coveredEntities.Count)
      for i = 0, coveredEntities.Count - 1 do
        local candidates = context:GetLocationDictionary(TInfo)
        for _, default in System.each(allCandidateToInfo:EnumeratePairs()) do
          local candidate, info = default:Deconstruct()
          if info.Covered:get(i) then
            candidates:Add(candidate, info)
          end
        end

        coveredToCandidates:AddKeyValue(i, candidates)
      end

      return coveredToCandidates
    end
    GetProviderOverlapBounds = function (grid, center, providerWidth, providerHeight)
      local entityMinX = center.X - (System.div((providerWidth - 1), 2))
      local entityMinY = center.Y - (System.div((providerHeight - 1), 2))
      local entityMaxX = center.X + (System.div(providerWidth, 2))
      local entityMaxY = center.Y + (System.div(providerHeight, 2))

      local overlapMinX = math.Max(System.div((providerWidth - 1), 2), entityMinX - (System.div(providerWidth, 2)))
      local overlapMinY = math.Max(System.div((providerHeight - 1), 2), entityMinY - (System.div(providerHeight, 2)))
      local overlapMaxX = math.Min(grid.Width - (System.div(providerWidth, 2)) - 1, entityMaxX + (System.div((providerWidth - 1), 2)))
      local overlapMaxY = math.Min(grid.Height - (System.div(providerHeight, 2)) - 1, entityMaxY + (System.div((providerHeight - 1), 2)))

      return System.ValueTuple(overlapMinX, overlapMinY, overlapMaxX, overlapMaxY)
    end
    RemoveOverlappingCandidates = function (grid, center, providerWidth, providerHeight, candidateToInfo, TInfo)
      local overlapMinX, overlapMinY, overlapMaxX, overlapMaxY = GetProviderOverlapBounds(grid, center, providerWidth, providerHeight):Deconstruct()

      for x = overlapMinX, overlapMaxX do
        for y = overlapMinY, overlapMaxY do
          local location = KnapcodeOilField.Location(x, y)
          candidateToInfo:Remove(location)
        end
      end
    end
    RemoveOverlappingCandidates1 = function (grid, center, providerWidth, providerHeight, candidateToInfo, coveredToCandidates, TInfo)
      local overlapMinX, overlapMinY, overlapMaxX, overlapMaxY = GetProviderOverlapBounds(grid, center, providerWidth, providerHeight):Deconstruct()

      for x = overlapMinX, overlapMaxX do
        for y = overlapMinY, overlapMaxY do
          local location = KnapcodeOilField.Location(x, y)
          local default, info = candidateToInfo:TryGetValue(location)
          if default then
            candidateToInfo:Remove(location)
            for i = 0, info.Covered.Count - 1 do
              if info.Covered:get(i) then
                coveredToCandidates:get(i):Remove(location)
              end
            end
          end
        end
      end
    end
    RemoveOverlappingCandidates2 = function (grid, center, providerWidth, providerHeight, candidateToInfo, scopedCandidateToInfo, coveredToCandidates, TInfo)
      local overlapMinX, overlapMinY, overlapMaxX, overlapMaxY = GetProviderOverlapBounds(grid, center, providerWidth, providerHeight):Deconstruct()

      for x = overlapMinX, overlapMaxX do
        for y = overlapMinY, overlapMaxY do
          local location = KnapcodeOilField.Location(x, y)
          if coveredToCandidates ~= nil then
            local default, info = candidateToInfo:TryGetValue(location)
            if default then
              candidateToInfo:Remove(location)
              scopedCandidateToInfo:Remove(location)
              for i = 0, info.Covered.Count - 1 do
                if info.Covered:get(i) then
                  coveredToCandidates:get(i):Remove(location)
                end
              end
            end
          elseif candidateToInfo:Remove(location) then
            scopedCandidateToInfo:Remove(location)
          end
        end
      end
    end
    RemoveEntity = function (grid, center, width, height)
      local minX = center.X - (System.div((width - 1), 2))
      local maxX = center.X + (System.div(width, 2))
      local minY = center.Y - (System.div((height - 1), 2))
      local maxY = center.Y + (System.div(height, 2))

      for x = minX, maxX do
        for y = minY, maxY do
          grid:RemoveEntity(KnapcodeOilField.Location(x, y))
        end
      end
    end
    AddProviderToGrid = function (grid, center, centerEntity, getNewSide, providerWidth, providerHeight, TCenter, TSide)
      local minX = center.X - (System.div((providerWidth - 1), 2))
      local maxX = center.X + (System.div(providerWidth, 2))
      local minY = center.Y - (System.div((providerHeight - 1), 2))
      local maxY = center.Y + (System.div(providerHeight, 2))

      for x = minX, maxX do
        for y = minY, maxY do
          local location = KnapcodeOilField.Location(x, y)
          local default
          if KnapcodeOilField.Location.op_Equality(location, center) then
            default = centerEntity
          else
            default = getNewSide(centerEntity, TCenter, TSide)
          end
          grid:AddEntity(location, default)
        end
      end
    end
    AddBeaconsToGrid = function (grid, options, centers)
      for _, center in System.each(centers) do
        AddProviderToGrid(grid, center, KnapcodeOilField.BeaconCenter(grid:GetId()), function (c)
          return KnapcodeOilField.BeaconSide(grid:GetId(), c)
        end, options.BeaconWidth, options.BeaconHeight, KnapcodeOilField.BeaconCenter, KnapcodeOilField.BeaconSide)
      end
    end
    IsProviderInBounds = function (grid, providerWidth, providerHeight, center)
      return center.X - (System.div((providerWidth - 1), 2)) > 0 and center.Y - (System.div((providerHeight - 1), 2)) > 0 and center.X + (System.div(providerWidth, 2)) < grid.Width and center.Y + (System.div(providerHeight, 2)) < grid.Height
    end
    EliminateOtherTerminals = function (context, selectedTerminal)
      local terminalOptions = context.CenterToTerminals:get(selectedTerminal.Center)

      if #terminalOptions == 1 then
        return
      end

      for i = 0, #terminalOptions - 1 do
        local continue
        repeat
          local otherTerminal = terminalOptions:get(i)
          if otherTerminal == selectedTerminal then
            continue = true
            break
          end

          local terminals = context.LocationToTerminals:get(otherTerminal.Terminal)

          if #terminals == 1 then
            context.LocationToTerminals:Remove(otherTerminal.Terminal)
          else
            terminals:Remove(otherTerminal)
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      terminalOptions:Clear()
      terminalOptions:Add(selectedTerminal)
    end
    GetPath = function (cameFrom, start, reachedGoal)
      local sizeEstimate = 2 * start:GetManhattanDistance(reachedGoal)
      local path = ListLocation(sizeEstimate)
      AddPath(cameFrom, reachedGoal, path)
      return path
    end
    AddPath = function (cameFrom, reachedGoal, outputList)
      local current = reachedGoal
      while true do
        local next = cameFrom:get(current)
        outputList:Add(current)
        if KnapcodeOilField.Location.op_Equality(next, current) then
          break
        end

        current = next
      end
    end
    AreLocationsCollinear = function (locations)
      local lastSlope = 0
      for i = 0, locations:getCount() - 1 do
        if i == locations:getCount() - 1 then
          return true
        end

        local node = locations:get(i)
        local next = locations:get(i + 1)
        local dX = math.Abs(node.X - next.X)
        local dY = math.Abs(node.Y - next.Y)
        if i == 0 then
          lastSlope = dY / dX
        elseif lastSlope ~= dY / dX then
          break
        end
      end

      return false
    end
    CountTurns = function (path)
      local previousDirection = - 1
      local turns = 0
      for i = 1, #path - 1 do
        local currentDirection = (path:get(i).X == path:get(i - 1).X) and 0 or 1
        if previousDirection ~= - 1 then
          if previousDirection ~= currentDirection then
            turns = turns + 1
          end
        end

        previousDirection = currentDirection
      end

      return turns
    end
    MakeStraightLineOnEmpty = function (grid, a, b)
      if a.X == b.X then
        local min, max = ((a.Y < b.Y) and System.ValueTuple(a.Y, b.Y) or System.ValueTuple(b.Y, a.Y)):Deconstruct()
        local line = ListLocation(max - min + 1)
        for y = min, max do
          if not grid:IsEmpty(KnapcodeOilField.Location(a.X, y)) then
            return nil
          end

          line:Add(KnapcodeOilField.Location(a.X, y))
        end

        return line
      end

      if a.Y == b.Y then
        local min, max = ((a.X < b.X) and System.ValueTuple(a.X, b.X) or System.ValueTuple(b.X, a.X)):Deconstruct()
        local line = ListLocation(max - min + 1)
        for x = min, max do
          if not grid:IsEmpty(KnapcodeOilField.Location(x, a.Y)) then
            return nil
          end

          line:Add(KnapcodeOilField.Location(x, a.Y))
        end

        return line
      end

      System.throw(System.ArgumentException("The two points must be one the same line either horizontally or vertically."))
    end
    MakeStraightLine = function (a, b)
      if a.X == b.X then
        local min, max = ((a.Y < b.Y) and System.ValueTuple(a.Y, b.Y) or System.ValueTuple(b.Y, a.Y)):Deconstruct()
        local line = ListLocation(max - min + 1)
        for y = min, max do
          line:Add(KnapcodeOilField.Location(a.X, y))
        end

        return line
      end

      if a.Y == b.Y then
        local min, max = ((a.X < b.X) and System.ValueTuple(a.X, b.X) or System.ValueTuple(b.X, a.X)):Deconstruct()
        local line = ListLocation(max - min + 1)
        for x = min, max do
          line:Add(KnapcodeOilField.Location(x, a.Y))
        end

        return line
      end

      System.throw(System.ArgumentException("The two points must be one the same line either horizontally or vertically."))
    end
    PointsToLines = function (nodes)
      return PointsToLines1(KnapcodeFactorioTools.CollectionExtensions.ToList(nodes, KnapcodeOilField.Location), true)
    end
    -- <summary>
    -- Source: https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts#L62
    -- </summary>
    PointsToLines1 = function (nodes, sort)
      local filteredNodes
      if sort then
        local sortedXY = KnapcodeFactorioTools.CollectionExtensions.ToList(nodes, KnapcodeOilField.Location)
        sortedXY:Sort(function (a, b)
          local c = System.Int32.CompareTo(a.X, b.X)
          if c ~= 0 then
            return c
          end

          return System.Int32.CompareTo(a.Y, b.Y)
        end)
        filteredNodes = sortedXY
      else
        filteredNodes = nodes
      end

      if filteredNodes:getCount() == 1 then
        local default = ListEndpoints()
        default:Add(KnapcodeOilField.Endpoints(filteredNodes:get(0), filteredNodes:get(0)))
        return default
      elseif filteredNodes:getCount() == 2 then
        local default = ListEndpoints()
        default:Add(KnapcodeOilField.Endpoints(filteredNodes:get(0), filteredNodes:get(1)))
        return default
      end

      -- Check that nodes are not collinear
      if AreLocationsCollinear(filteredNodes) then
        local collinearLines = ListEndpoints(filteredNodes:getCount() - 1)
        for i = 1, filteredNodes:getCount() - 1 do
          collinearLines:Add(KnapcodeOilField.Endpoints(filteredNodes:get(i - 1), filteredNodes:get(i)))
        end

        return collinearLines
      end

      local points = ArrayIPoint(filteredNodes:getCount())
      for i = 0, filteredNodes:getCount() - 1 do
        local node = filteredNodes:get(i)
        points:set(i, DelaunatorSharp.Point(node.X, node.Y))
      end
      local delaunator = DelaunatorSharp.Delaunator(points)

      local lines = ListEndpoints()
      for e = 0, #delaunator.Triangles - 1 do
        if e > delaunator.Halfedges:get(e) then
          local p = filteredNodes:get(delaunator.Triangles:get(e))
          local q = filteredNodes:get(delaunator.Triangles:get((System.mod(e, 3) == 2) and (e - 2) or (e + 1)))
          lines:Add(KnapcodeOilField.Endpoints(p, q))
        end
      end

      return lines
    end
    ToInt = function (x, error)
      if math.Abs(System.modf(x, 1)) > 1.40129846E-43 --[[float.Epsilon * 100]] then
        System.throw(KnapcodeFactorioTools.FactorioToolsException(error))
      end

      return System.ToInt32(math.Round(x, 0))
    end
    return {
      AddPumpjack = AddPumpjack,
      GetCenterToTerminals = GetCenterToTerminals,
      PopulateCenterToTerminals = PopulateCenterToTerminals,
      GetLocationToTerminals = GetLocationToTerminals,
      PopulateLocationToTerminals = PopulateLocationToTerminals,
      GetBeaconCandidateToCovered = GetBeaconCandidateToCovered,
      GetElectricPoleCandidateToCovered = GetElectricPoleCandidateToCovered,
      GetProviderCenterToCoveredCenters = GetProviderCenterToCoveredCenters,
      GetCoveredCenterToProviderCenters = GetCoveredCenterToProviderCenters,
      DoesProviderFit = DoesProviderFit,
      GetEntityDistance = GetEntityDistance,
      AddProviderAndPreventMultipleProviders = AddProviderAndPreventMultipleProviders,
      AddProviderAndAllowMultipleProviders = AddProviderAndAllowMultipleProviders,
      GetElectricPoleCoverage = GetElectricPoleCoverage,
      GetPoweredEntities = GetPoweredEntities,
      GetCoveredToCandidates = GetCoveredToCandidates,
      GetProviderOverlapBounds = GetProviderOverlapBounds,
      RemoveOverlappingCandidates = RemoveOverlappingCandidates,
      RemoveOverlappingCandidates1 = RemoveOverlappingCandidates1,
      RemoveOverlappingCandidates2 = RemoveOverlappingCandidates2,
      RemoveEntity = RemoveEntity,
      AddProviderToGrid = AddProviderToGrid,
      AddBeaconsToGrid = AddBeaconsToGrid,
      IsProviderInBounds = IsProviderInBounds,
      EliminateOtherTerminals = EliminateOtherTerminals,
      GetPath = GetPath,
      AddPath = AddPath,
      AreLocationsCollinear = AreLocationsCollinear,
      CountTurns = CountTurns,
      MakeStraightLineOnEmpty = MakeStraightLineOnEmpty,
      MakeStraightLine = MakeStraightLine,
      PointsToLines = PointsToLines,
      PointsToLines1 = PointsToLines1,
      ToInt = ToInt,
      static = static
    }
  end)
end)
