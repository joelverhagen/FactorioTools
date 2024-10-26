-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeOilField
local KnapcodePlanBeaconsFbe
local ListArea
local ListLocation
local ArrayLocation
local ListArrayLocation
local ListBeaconCandidate
System.import(function (out)
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  KnapcodePlanBeaconsFbe = Knapcode.FactorioTools.OilField.PlanBeaconsFbe
  ListArea = System.List(KnapcodePlanBeaconsFbe.Area)
  ListLocation = System.List(KnapcodeOilField.Location)
  ArrayLocation = System.Array(KnapcodeOilField.Location)
  ListArrayLocation = System.List(ArrayLocation)
  ListBeaconCandidate = System.List(KnapcodePlanBeaconsFbe.BeaconCandidate)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  -- <summary>
  -- This "FBE" implementation is based on Teoxoy's Factorio Blueprint Editor (FBE).
  -- Source:
  -- - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/beacon.ts
  -- - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts
  -- 
  -- It has been modified:
  -- 
  -- - Some performance improvements (some are .NET specific)
  -- - Many .NET specific performance improvemants.
  -- - The beacon candidate sorting only happens once.
  -- - Some quality improvements which yield better and more consistent results.
  -- - Sorting only once
  -- - Sort with a stable tie-breaking criteria (distance from the middle)
  -- - Add support for non-standard beacon sizes
  -- - Add support for non-overlapping beacons (for Space Exploration beacon overlap)
  -- </summary>
  namespace.class("PlanBeaconsFbe", function (namespace)
    local Execute, GetBeacons, SortPossibleBeacons, SortPossibleBeaconsOriginal, GetPossibleBeacons, GetBounds, GetOccupiedPositions, GetEntityAreas, 
    GetEffectEntityAreas, GetPossibleBeaconAreas, GetPointToBeaconCount, GetPointToEntityArea, class
    namespace.class("Bounds", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, EntityMinX, EntityMinY, EntityMaxX, EntityMaxY)
        this.EntityMinX = EntityMinX
        this.EntityMinY = EntityMinY
        this.EntityMaxX = EntityMaxX
        this.EntityMaxY = EntityMaxY
      end
      __members__ = function ()
        return "Bounds", "EntityMinX", "EntityMinY", "EntityMaxX", "EntityMaxY"
      end
      return {
        EntityMinX = 0,
        EntityMinY = 0,
        EntityMaxX = 0,
        EntityMaxY = 0,
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.PlanBeaconsFbe.Bounds)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("Area", function (namespace)
      local __ctor__
      __ctor__ = function (this, index, effect, locations)
        this.Index = index
        this.Effect = effect
        this.Locations = locations
      end
      return {
        Index = 0,
        Effect = false,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("BeaconCandidate", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, OriginalIndex, Center, CollisionArea, EffectsGivenCount, AverageDistanceToEntities, NumberOfOverlaps, EffectsGiven)
        this.OriginalIndex = OriginalIndex
        this.Center = Center
        this.CollisionArea = CollisionArea
        this.EffectsGivenCount = EffectsGivenCount
        this.AverageDistanceToEntities = AverageDistanceToEntities
        this.NumberOfOverlaps = NumberOfOverlaps
        this.EffectsGiven = EffectsGiven
      end
      __members__ = function ()
        return "BeaconCandidate", "OriginalIndex", "Center", "CollisionArea", "EffectsGivenCount", "AverageDistanceToEntities", "NumberOfOverlaps", "EffectsGiven"
      end
      return {
        OriginalIndex = 0,
        EffectsGivenCount = 0,
        AverageDistanceToEntities = 0,
        NumberOfOverlaps = 0,
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.PlanBeaconsFbe.BeaconCandidate)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    Execute = function (context, strategy)
      local entityAreas = GetEntityAreas(context)
      local occupiedPositions = GetOccupiedPositions(context, entityAreas)

      -- Visualizer.Show(context.Grid, occupiedPositions.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

      local possibleBeaconAreas = GetPossibleBeaconAreas(context, occupiedPositions)

      -- Visualizer.Show(context.Grid, possibleBeaconAreas.SelectMany(l => l).Distinct(context).Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());

      local pointToBeaconCount = GetPointToBeaconCount(context, possibleBeaconAreas)
      local effectEntityAreas = GetEffectEntityAreas(entityAreas)
      local pointToEntityArea = GetPointToEntityArea(context, effectEntityAreas)

      -- GENERATE POSSIBLE BEACONS
      local possibleBeacons = GetPossibleBeacons(context, effectEntityAreas, possibleBeaconAreas, pointToBeaconCount, pointToEntityArea)

      if strategy == 1 --[[BeaconStrategy.Fbe]] then
        possibleBeacons = SortPossibleBeacons(context, possibleBeacons)
      elseif strategy == 0 --[[BeaconStrategy.FbeOriginal]] then
        SortPossibleBeaconsOriginal(possibleBeacons)
      end

      -- GENERATE BEACONS
      return GetBeacons(context, effectEntityAreas, possibleBeacons)
    end
    GetBeacons = function (context, effectEntityAreas, possibleBeacons)
      local beacons = ListLocation()
      local effects = 0
      local collisionArea = context:GetLocationSet1()
      local default
      if context.Options.OverlapBeacons then
        default = nil
      else
        default = System.new(KnapcodeOilField.CustomCountedBitArray, 2, #effectEntityAreas)
      end
      local coveredEntityAreas = default
      while #possibleBeacons > 0 do
        local continue
        repeat
          local beacon = possibleBeacons:get(#possibleBeacons - 1)
          possibleBeacons:RemoveAt(#possibleBeacons - 1)

          if collisionArea:Overlaps(beacon.CollisionArea) then
            continue = true
            break
          end

          if not context.Options.OverlapBeacons then
            local overlapping = KnapcodeOilField.CustomCountedBitArray(beacon.EffectsGiven)
            overlapping:And(coveredEntityAreas)

            if overlapping.TrueCount > 0 then
              continue = true
              break
            end

            coveredEntityAreas:Or(beacon.EffectsGiven)

            if coveredEntityAreas.TrueCount == coveredEntityAreas.Count then
              break
            end
          end

          beacons:Add(beacon.Center)
          effects = effects + beacon.EffectsGivenCount
          -- Console.WriteLine($"{beacon.Center} --- {beacon.EffectsGivenCount}");
          collisionArea:UnionWith(beacon.CollisionArea)
          continue = true
        until 1
        if not continue then
          break
        end
      end

      return KnapcodeOilField.BeaconPlannerResult(beacons, effects)
    end
    SortPossibleBeacons = function (context, possibleBeacons)
      local candidateToDistance = context:GetLocationDictionary(System.Double)

      possibleBeacons:Sort(function (a, b)
        local c = System.Int32.CompareTo(a.EffectsGivenCount, b.EffectsGivenCount)
        if c ~= 0 then
          return c
        end

        local aC = (a.EffectsGivenCount == 1) and - a.AverageDistanceToEntities or a.NumberOfOverlaps
        local bC = (b.EffectsGivenCount == 1) and - b.AverageDistanceToEntities or b.NumberOfOverlaps
        c = System.Double.CompareTo(bC, aC)
        if c ~= 0 then
          return c
        end

        local default
        default, aC = candidateToDistance:TryGetValue(a.Center)
        if not default then
          aC = a.Center:GetEuclideanDistance(context.Grid.Middle)
          candidateToDistance:Add(a.Center, aC)
        end

        local extern
        extern, bC = candidateToDistance:TryGetValue(b.Center)
        if not extern then
          bC = b.Center:GetEuclideanDistance(context.Grid.Middle)
          candidateToDistance:Add(b.Center, bC)
        end

        c = System.Double.CompareTo(aC, bC)
        if c ~= 0 then
          return c
        end

        return System.Int32.CompareTo(b.OriginalIndex, a.OriginalIndex)
      end)

      return possibleBeacons
    end
    SortPossibleBeaconsOriginal = function (possibleBeacons)
      --[[
        possibleBeacons
            .Sort((a, b) =>
            {
                var c = b.EffectsGivenCount.CompareTo(a.EffectsGivenCount);
                if (c != 0)
                {
                    return c;
                }

                var aN = a.EffectsGivenCount == 1 ? -a.AverageDistanceToEntities : a.NumberOfOverlaps;
                var bN = b.EffectsGivenCount == 1 ? -b.AverageDistanceToEntities : b.NumberOfOverlaps;
                c = aN.CompareTo(bN);
                if (c != 0)
                {
                    return c;
                }

                aN = a.Center.GetEuclideanDistance(context.Grid.Middle);
                bN = b.Center.GetEuclideanDistance(context.Grid.Middle);
                return bN.CompareTo(aN);
            });
        ]]

      -- This is not exactly like FBE because it causes inconsistent sorting results causing an exception.
      -- The original is here. The original comparer violates expectations held by List<T>.Sort in .NET:
      -- https://github.com/teoxoy/factorio-blueprint-editor/blob/83343e6a6c91608c43a823326fb16c01c934b4bd/packages/editor/src/core/generators/beacon.ts#L177-L183
      possibleBeacons:Sort(function (a, b)
        local c = System.Int32.CompareTo(a.EffectsGivenCount, b.EffectsGivenCount)
        if c ~= 0 then
          return c
        end

        c = System.Int32.CompareTo(b.NumberOfOverlaps, a.NumberOfOverlaps)
        if c ~= 0 then
          return c
        end

        c = System.Double.CompareTo(a.AverageDistanceToEntities, b.AverageDistanceToEntities)
        if c ~= 0 then
          return c
        end

        return System.Int32.CompareTo(a.OriginalIndex, b.OriginalIndex)
      end)

      --[[
        possibleBeacons
            .Sort((a, b) =>
            {
                if (a.EffectsGivenCount == 1 || b.EffectsGivenCount == 1)
                {
                    return b.AverageDistanceToEntities.CompareTo(a.AverageDistanceToEntities);
                }

                return a.NumberOfOverlaps.CompareTo(b.NumberOfOverlaps);
            });

        possibleBeacons.Sort((a, b) => b.EffectsGivenCount.CompareTo(a.EffectsGivenCount));
        ]]

      -- possibleBeacons.Reverse();
    end
    GetPossibleBeacons = function (context, effectEntityAreas, possibleBeaconAreas, pointToBeaconCount, pointToEntityArea)
      local entityMinX, entityMinY, entityMaxX, entityMaxY = GetBounds(pointToEntityArea:getKeys()):Deconstruct()

      local centerX = System.div((context.Options.BeaconWidth - 1), 2)
      local centerY = System.div((context.Options.BeaconHeight - 1), 2)
      local centerIndex = centerY * context.Options.BeaconWidth + centerX

      local possibleBeacons = ListBeaconCandidate(#possibleBeaconAreas)
      local effectsGiven = System.new(KnapcodeOilField.CustomCountedBitArray, 2, #effectEntityAreas)
      for i = 0, #possibleBeaconAreas - 1 do
        local continue
        repeat
          local collisionArea = possibleBeaconAreas:get(i)
          local center = collisionArea:get(centerIndex)

          local minX = math.Max(entityMinX, center.X - (System.div(context.Options.BeaconSupplyWidth, 2)))
          local minY = math.Max(entityMinY, center.Y - (System.div(context.Options.BeaconSupplyHeight, 2)))
          local maxX = math.Min(entityMaxX, center.X + (System.div(context.Options.BeaconSupplyWidth, 2)))
          local maxY = math.Min(entityMaxY, center.Y + (System.div(context.Options.BeaconSupplyHeight, 2)))

          for x = minX, maxX do
            for y = minY, maxY do
              local location = KnapcodeOilField.Location(x, y)
              local default, area = pointToEntityArea:TryGetValue(location)
              if default then
                effectsGiven:set(area.Index, true)
              end
            end
          end

          if effectsGiven.TrueCount < 1 --[[PlanBeaconsFbe.MinAffectedEntities]] then
            effectsGiven:SetAll(false)
            continue = true
            break
          end

          local sumDistance = 0
          for j = 0, effectsGiven.Count - 1 do
            if effectsGiven:get(j) then
              sumDistance = sumDistance + effectEntityAreas:get(j).Locations:get(centerIndex):GetManhattanDistance(center)
            end
          end

          local averageDistanceToEntities = sumDistance / effectsGiven.TrueCount

          local numberOfOverlaps = 0
          for j = 0, #collisionArea - 1 do
            numberOfOverlaps = numberOfOverlaps + pointToBeaconCount:get(collisionArea:get(j))
          end

          local originalIndex = #possibleBeacons
          local default
          if context.Options.OverlapBeacons then
            default = nil
          else
            default = KnapcodeOilField.CustomCountedBitArray(effectsGiven)
          end
          possibleBeacons:Add(class.BeaconCandidate(originalIndex, center, collisionArea, effectsGiven.TrueCount, averageDistanceToEntities, numberOfOverlaps, default))

          effectsGiven:SetAll(false)
          continue = true
        until 1
        if not continue then
          break
        end
      end

      return possibleBeacons
    end
    GetBounds = function (locations)
      local entityMinX = 2147483647 --[[Int32.MaxValue]]
      local entityMinY = 2147483647 --[[Int32.MaxValue]]
      local entityMaxX = -2147483648 --[[Int32.MinValue]]
      local entityMaxY = -2147483648 --[[Int32.MinValue]]

      for _, location in System.each(locations) do
        if location.X < entityMinX then
          entityMinX = location.X
        end

        if location.Y < entityMinY then
          entityMinY = location.Y
        end

        if location.X > entityMaxX then
          entityMaxX = location.X
        end
        if location.Y > entityMaxY then
          entityMaxY = location.Y
        end
      end

      return class.Bounds(entityMinX, entityMinY, entityMaxX, entityMaxY)
    end
    GetOccupiedPositions = function (context, entityAreas)
      local occupiedPositions = context:GetLocationSet1()
      for i = 0, #entityAreas - 1 do
        local area = entityAreas:get(i)
        for j = 0, #area.Locations - 1 do
          occupiedPositions:Add(area.Locations:get(j))
        end
      end

      return occupiedPositions
    end
    GetEntityAreas = function (context)
      local areas = ListArea(context.Grid:getEntityLocations():getCount())

      for _, location in System.each(context.Grid:getEntityLocations():EnumerateItems()) do
        local continue
        repeat
          local entity = context.Grid:get(location)
          local width
          local height
          local effect

          if System.is(entity, KnapcodeOilField.TemporaryEntity) or System.is(entity, KnapcodeOilField.AvoidEntity) then
            width = 1
            height = 1
            effect = false
          elseif System.is(entity, KnapcodeOilField.ElectricPoleCenter) then
            width = context.Options.ElectricPoleWidth
            height = context.Options.ElectricPoleHeight
            effect = false
          elseif System.is(entity, KnapcodeOilField.PumpjackCenter) then
            width = 3 --[[Helpers.PumpjackWidth]]
            height = 3 --[[Helpers.PumpjackHeight]]
            effect = true
          elseif System.is(entity, KnapcodeOilField.BeaconCenter) or System.is(entity, KnapcodeOilField.BeaconSide) or System.is(entity, KnapcodeOilField.ElectricPoleSide) or System.is(entity, KnapcodeOilField.PumpjackSide) then
            continue = true
            break
          else
            System.throw(System.NotImplementedException())
          end

          local minX = location.X - (System.div((width - 1), 2))
          local maxX = location.X + (System.div(width, 2))
          local minY = location.Y - (System.div((height - 1), 2))
          local maxY = location.Y + (System.div(height, 2))

          local area = ArrayLocation(width * height)
          local i = 0
          for x = minX, maxX do
            for y = minY, maxY do
              local default = i
              i = default + 1
              area:set(default, KnapcodeOilField.Location(x, y))
            end
          end

          local index = #areas
          areas:Add(class.Area(index, effect, area))
          continue = true
        until 1
        if not continue then
          break
        end
      end

      return areas
    end
    GetEffectEntityAreas = function (entityAreas)
      local effectEntityArea = ListArea()
      for i = 0, #entityAreas - 1 do
        local area = entityAreas:get(i)
        if area.Effect then
          area.Index = #effectEntityArea
          effectEntityArea:Add(area)
        else
          area.Index = - 1
        end
      end

      return effectEntityArea
    end
    GetPossibleBeaconAreas = function (context, occupiedPositions)
      local validBeaconCenters = context:GetLocationSet1()
      local possibleBeaconAreas = ListArrayLocation()

      local gridMinX = System.div((context.Options.BeaconWidth - 1), 2)
      local gridMinY = System.div((context.Options.BeaconHeight - 1), 2)
      local gridMaxX = context.Grid.Width - (System.div(context.Options.BeaconWidth, 2)) - 1
      local gridMaxY = context.Grid.Height - (System.div(context.Options.BeaconHeight, 2)) - 1

      local supplyLeft = (1 --[[(PumpjackWidth - 1) / 2]]) + (System.div(context.Options.BeaconSupplyWidth, 2))
      local supplyUp = (1 --[[(PumpjackHeight - 1) / 2]]) + (System.div(context.Options.BeaconSupplyHeight, 2))
      local supplyRight = (1 --[[PumpjackWidth / 2]]) + (System.div((context.Options.BeaconSupplyWidth - 1), 2))
      local supplyDown = (1 --[[PumpjackHeight / 2]]) + (System.div((context.Options.BeaconSupplyHeight - 1), 2))

      local beaconLeft = System.div((context.Options.BeaconWidth - 1), 2)
      local beaconUp = System.div((context.Options.BeaconHeight - 1), 2)
      local beaconRight = System.div(context.Options.BeaconWidth, 2)
      local beaconDown = System.div(context.Options.BeaconHeight, 2)

      local area = ArrayLocation(context.Options.BeaconWidth * context.Options.BeaconHeight)

      for _, center in System.each(context.Centers) do
        local continue
        repeat
          local supplyMinX = math.Max(gridMinX, center.X - supplyLeft)
          local supplyMinY = math.Max(gridMinY, center.Y - supplyUp)
          local supplyMaxX = math.Min(gridMaxX, center.X + supplyRight)
          local supplyMaxY = math.Min(gridMaxY, center.Y + supplyDown)

          for centerX = supplyMinX, supplyMaxX do
            local continue
            repeat
              for centerY = supplyMinY, supplyMaxY do
                local continue
                repeat
                  local beaconCenter = KnapcodeOilField.Location(centerX, centerY)
                  if not validBeaconCenters:Add(beaconCenter) then
                    continue = true
                    break
                  end

                  local minX = beaconCenter.X - beaconLeft
                  local minY = beaconCenter.Y - beaconUp
                  local maxX = beaconCenter.X + beaconRight
                  local maxY = beaconCenter.Y + beaconDown
                  local fits = true

                  local i = 0
                  do
                    local y = minY
                    while fits and y <= maxY do
                      do
                        local x = minX
                        while fits and x <= maxX do
                          local location = KnapcodeOilField.Location(x, y)
                          if occupiedPositions:Contains(location) then
                            fits = false
                          else
                            local default = i
                            i = default + 1
                            area:set(default, location)
                          end
                          x = x + 1
                        end
                      end
                      y = y + 1
                    end
                  end

                  if fits then
                    possibleBeaconAreas:Add(area)
                    area = ArrayLocation(context.Options.BeaconWidth * context.Options.BeaconHeight)
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

      return possibleBeaconAreas
    end
    GetPointToBeaconCount = function (context, possibleBeaconAreas)
      local pointToBeaconCount = context:GetLocationDictionary(System.Int32)
      for i = 0, #possibleBeaconAreas - 1 do
        local areas = possibleBeaconAreas:get(i)
        for j = 0, #areas - 1 do
          local point = areas:get(j)
          local default, sum = pointToBeaconCount:TryGetValue(point)
          if not default then
            pointToBeaconCount:Add(point, 1)
          else
            pointToBeaconCount:set(point, sum + 1)
          end
        end
      end

      return pointToBeaconCount
    end
    GetPointToEntityArea = function (context, effectEntityAreas)
      local pointToEntityArea = context:GetLocationDictionary(class.Area)
      for i = 0, #effectEntityAreas - 1 do
        local area = effectEntityAreas:get(i)
        for j = 0, #area.Locations - 1 do
          pointToEntityArea:Add(area.Locations:get(j), area)
        end
      end

      return pointToEntityArea
    end
    class = {
      Execute = Execute
    }
    return class
  end)
end)