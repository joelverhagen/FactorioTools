-- Generated by CSharp.lua Compiler
local System = System
local HashSetInt32 = System.HashSet(System.Int32)
local KnapcodeFactorioTools
local KnapcodeOilField
local KnapcodeAddElectricPoles
local SpanLocation
local ArrayLocation
local QueueLocation
local QueueElectricPoleCenter
local SortedBatches_1ElectricPoleCandidateInfo
local QueueSortedBatches_1ElectricPoleCandidateInfo
local ILocationDictionary_1ElectricPoleCandidateInfo
local DictInt32ILocationDictionary_1ElectricPoleCandidateInfo
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  KnapcodeAddElectricPoles = Knapcode.FactorioTools.OilField.AddElectricPoles
  SpanLocation = System.Span(KnapcodeOilField.Location)
  ArrayLocation = System.Array(KnapcodeOilField.Location)
  QueueLocation = System.Queue(KnapcodeOilField.Location)
  QueueElectricPoleCenter = System.Queue(KnapcodeOilField.ElectricPoleCenter)
  SortedBatches_1ElectricPoleCandidateInfo = KnapcodeOilField.SortedBatches_1(KnapcodeOilField.ElectricPoleCandidateInfo)
  QueueSortedBatches_1ElectricPoleCandidateInfo = System.Queue(SortedBatches_1ElectricPoleCandidateInfo)
  ILocationDictionary_1ElectricPoleCandidateInfo = KnapcodeOilField.ILocationDictionary_1(KnapcodeOilField.ElectricPoleCandidateInfo)
  DictInt32ILocationDictionary_1ElectricPoleCandidateInfo = System.Dictionary(System.Int32, ILocationDictionary_1ElectricPoleCandidateInfo)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("AddElectricPoles", function (namespace)
    local Execute, RemoveExtraElectricPoles, ArePolesConnectedWithout, ConnectExistingElectricPoles, AreElectricPolesConnected, GetElectricPoleDistanceSquared, AddElectricPolesAroundEntities, AddElectricPolesAroundEntities1, 
    PopulateCandidateToInfo, UpdateCandidateInfo, AddElectricPole, ConnectElectricPoles, AddSinglePoleForConnection, GetElectricPoleGroups, class
    namespace.class("CandidateFactory", function (namespace)
      local Instance, Create, class, static
      static = function (this)
        Instance = class()
        this.Instance = Instance
      end
      Create = function (this, covered)
        return KnapcodeOilField.ElectricPoleCandidateInfo(covered)
      end
      class = {
        base = function (out)
          return {
            out.Knapcode.FactorioTools.OilField.ICandidateFactory_1(out.Knapcode.FactorioTools.OilField.ElectricPoleCandidateInfo)
          }
        end,
        Create = Create,
        static = static
      }
      return class
    end)
    namespace.class("CandidateComparerForSameCoveredCount", function (namespace)
      local Instance, Compare, CompareWithoutPriorityPowered, class, static
      static = function (this)
        Instance = class()
        this.Instance = Instance
      end
      Compare = function (this, x, y)
        return CompareWithoutPriorityPowered(x, y)
      end
      CompareWithoutPriorityPowered = function (x, y)
        local xi = (x.OthersConnected > 0) and x.OthersConnected or 2147483647 --[[Int32.MaxValue]]
        local yi = (y.OthersConnected > 0) and y.OthersConnected or 2147483647 --[[Int32.MaxValue]]
        local c = System.Int32.CompareTo(xi, yi)
        if c ~= 0 then
          return c
        end

        xi = (x.OthersConnected > 0) and 0 or x.PoleDistance
        yi = (y.OthersConnected > 0) and 0 or y.PoleDistance
        c = System.Int32.CompareTo(xi, yi)
        if c ~= 0 then
          return c
        end

        c = System.Double.CompareTo(x.EntityDistance, y.EntityDistance)
        if c ~= 0 then
          return c
        end

        return System.Int32.CompareTo(x.MiddleDistance, y.MiddleDistance)
      end
      class = {
        base = function (out)
          return {
            System.IComparer_1(out.Knapcode.FactorioTools.OilField.ElectricPoleCandidateInfo)
          }
        end,
        Compare = Compare,
        CompareWithoutPriorityPowered = CompareWithoutPriorityPowered,
        static = static
      }
      return class
    end)
    namespace.class("CandidateComparerForSamePriorityPowered", function (namespace)
      local Instance, Compare, class, static
      static = function (this)
        Instance = class()
        this.Instance = Instance
      end
      Compare = function (this, x, y)
        local c = System.Int32.CompareTo(y.Covered.TrueCount, x.Covered.TrueCount)
        if c ~= 0 then
          return c
        end

        return KnapcodeAddElectricPoles.CandidateComparerForSameCoveredCount.CompareWithoutPriorityPowered(x, y)
      end
      class = {
        base = function (out)
          return {
            System.IComparer_1(out.Knapcode.FactorioTools.OilField.ElectricPoleCandidateInfo)
          }
        end,
        Compare = Compare,
        static = static
      }
      return class
    end)
    Execute = function (context, avoid, allowRetries)
      local avoidEntities = nil
      if avoid:getCount() > 0 then
        avoidEntities = context:GetLocationSet2(true)
        for _, location in System.each(avoid:EnumerateItems()) do
          if context.Grid:IsEmpty(location) then
            if avoidEntities:Add(location) then
              context.Grid:AddEntity(location, KnapcodeOilField.TemporaryEntity(context.Grid:GetId()))
            end
          end
        end
      end

      -- Visualizer.Show(context.Grid, poweredEntities.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.Center.X, c.Center.Y)), Array.Empty<DelaunatorSharp.IEdge>());

      local electricPoleList, poweredEntities = AddElectricPolesAroundEntities(context, allowRetries):Deconstruct()
      if electricPoleList == nil then
        return nil
      end

      local electricPoles = context:GetLocationDictionary(KnapcodeOilField.ElectricPoleCenter)
      for i = 0, electricPoleList:getCount() - 1 do
        local center = electricPoleList:get(i)
        local centerEntity = System.as(context.Grid:get(center), KnapcodeOilField.ElectricPoleCenter)
        if centerEntity == nil then
          AddElectricPole(context, electricPoles, center)
        else
          ConnectExistingElectricPoles(context, electricPoles, center, centerEntity)
          electricPoles:Add(center, centerEntity)
        end
      end

      -- Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

      ConnectElectricPoles(context, electricPoles)

      -- Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

      RemoveExtraElectricPoles(context, poweredEntities, electricPoles)

      -- Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

      -- PruneNeighbors(context, electricPoles);

      -- Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

      if avoidEntities ~= nil and avoidEntities:getCount() > 0 then
        for _, terminal in System.each(avoidEntities:EnumerateItems()) do
          context.Grid:RemoveEntity(terminal)
        end
      end

      return KnapcodeFactorioTools.CollectionExtensions.ToReadOnlySet(electricPoles:getKeys(), context)
    end
    RemoveExtraElectricPoles = function (context, poweredEntities, electricPoles)
      local poleCenterToCoveredCenters, coveredCenterToPoleCenters = KnapcodeOilField.Helpers.GetElectricPoleCoverage(context, poweredEntities, electricPoles:getKeys()):Deconstruct()

      local removeCandidates = context:GetLocationSet2(true)

      for _, p in System.each(coveredCenterToPoleCenters:EnumeratePairs()) do
        -- Consider electric poles covering pumpjacks that are covered by at least one other electric pole.
        if p[2]:getCount() > 2 then
          removeCandidates:UnionWith1(p[2])
        end
      end

      for _, pair in System.each(poleCenterToCoveredCenters:EnumeratePairs()) do
        if pair[2]:getCount() == 0 then
          removeCandidates:Add(pair[1])
        end
      end

      for _, p in System.each(coveredCenterToPoleCenters:EnumeratePairs()) do
        -- Consider electric poles covering pumpjacks that are covered by at least one other electric pole.
        if p[2]:getCount() == 1 then
          removeCandidates:ExceptWith(p[2])
        end
      end

      while removeCandidates:getCount() > 0 do
        local center = KnapcodeFactorioTools.CollectionExtensions.First(removeCandidates:EnumerateItems(), KnapcodeOilField.Location)
        local centerEntity = electricPoles:get(center)
        if ArePolesConnectedWithout(context.Grid, electricPoles, centerEntity) then
          electricPoles:Remove(center)
          KnapcodeOilField.Helpers.RemoveEntity(context.Grid, center, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight)

          for _, coveredCenter in System.each(poleCenterToCoveredCenters:get(center):EnumerateItems()) do
            local poleCenters = coveredCenterToPoleCenters:get(coveredCenter)
            poleCenters:Remove(center)
            if poleCenters:getCount() == 1 then
              removeCandidates:ExceptWith(poleCenters)
            end
          end

          poleCenterToCoveredCenters:Remove(center)
        end

        removeCandidates:Remove(center)
      end
    end
    ArePolesConnectedWithout = function (grid, electricPoles, except)
      local queue = QueueElectricPoleCenter()
      for _, center in System.each(electricPoles:getValues()) do
        if center ~= except then
          queue:Enqueue(center)
          break
        end
      end
      local discovered = HashSetInt32()

      while #queue > 0 do
        local continue
        repeat
          local current = queue:Dequeue()

          if current == except then
            continue = true
            break
          end

          if discovered:Add(current.Id) then
            for _, id in System.each(current:getNeighbors()) do
              queue:Enqueue(grid:GetEntity(id, KnapcodeOilField.ElectricPoleCenter))
            end
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      return discovered:getCount() == electricPoles:getCount() - 1
    end
    ConnectExistingElectricPoles = function (context, electricPoles, center, centerEntity)
      for _, default in System.each(electricPoles:EnumeratePairs()) do
        local other, otherCenter = default:Deconstruct()
        local continue
        repeat
          if KnapcodeOilField.Location.op_Equality(center, other) then
            continue = true
            break
          end

          if AreElectricPolesConnected(center, other, context.Options) then
            centerEntity:AddNeighbor(otherCenter)
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end
    end
    AreElectricPolesConnected = function (a, b, options)
      return GetElectricPoleDistanceSquared(a, b, options) <= options.ElectricPoleWireReachSquared
    end
    GetElectricPoleDistanceSquared = function (a, b, options)
      local offsetX = System.div((options.ElectricPoleWidth - 1), 2)
      local offsetY = System.div((options.ElectricPoleHeight - 1), 2)

      return b:GetEuclideanDistanceSquared1(a.X + offsetX, a.Y + offsetY)
    end
    AddElectricPolesAroundEntities = function (context, allowRetries)
      local retryStrategy = allowRetries and 3 --[[RetryStrategy.PreferUncoveredEntities]] or 0 --[[RetryStrategy.None]]
      local entitiesToPowerFirst = nil

      while true do
        -- Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

        local poweredEntities, hasBeacons = KnapcodeOilField.Helpers.GetPoweredEntities(context):Deconstruct()
        local electricPoleList, coveredEntities = AddElectricPolesAroundEntities1(context, poweredEntities, entitiesToPowerFirst):Deconstruct()

        -- Visualizer.Show(context.Grid, poweredEntities.Where((e, i) => !coveredEntities[i]).Select(e => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(e.Center.X, e.Center.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        if retryStrategy == 0 --[[RetryStrategy.None]] or electricPoleList ~= nil then
          return System.ValueTuple(electricPoleList, poweredEntities)
        end

        -- Visualizer.Show(context.Grid, poweredEntities.Where((e, i) => !coveredEntities[i]).Select(e => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(e.Center.X, e.Center.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        -- Console.WriteLine("Applying retry strategy " + retryStrategy);

        if retryStrategy == 3 --[[RetryStrategy.PreferUncoveredEntities]] then
          entitiesToPowerFirst = coveredEntities
          entitiesToPowerFirst:Not()
        elseif retryStrategy == 2 --[[RetryStrategy.RemoveUnpoweredBeacons]] then
          local centersToPowerFirst = context:GetLocationSet1()
          for i = poweredEntities:getCount() - 1, 0, -1 do
            local entity = poweredEntities:get(i)
            if not coveredEntities:get(i) then
              if System.is(context.Grid:get(entity.Center), KnapcodeOilField.BeaconCenter) then
                poweredEntities:RemoveAt(i)
                KnapcodeOilField.Helpers.RemoveEntity(context.Grid, entity.Center, entity.Width, entity.Height)
              else
                centersToPowerFirst:Add(entity.Center)
              end
            end
          end

          if centersToPowerFirst:getCount() == 0 then
            entitiesToPowerFirst = nil
          else
            entitiesToPowerFirst = System.new(KnapcodeOilField.CustomCountedBitArray, 2, poweredEntities:getCount())
            do
              local i = 0
              while centersToPowerFirst:getCount() > 0 and i < poweredEntities:getCount() do
                if centersToPowerFirst:Remove(poweredEntities:get(i).Center) then
                  entitiesToPowerFirst:set(i, true)
                end
                i = i + 1
              end
            end
          end
        elseif retryStrategy == 1 --[[RetryStrategy.RemoveUnpoweredEntities]] then
          for i = poweredEntities:getCount() - 1, 0, -1 do
            local entity = poweredEntities:get(i)
            if not coveredEntities:get(i) then
              local shouldRemove = retryStrategy == 1 --[[RetryStrategy.RemoveUnpoweredEntities]] or (System.is(context.Grid:get(entity.Center), KnapcodeOilField.BeaconCenter) and retryStrategy == 2 --[[RetryStrategy.RemoveUnpoweredBeacons]])

              if shouldRemove then
                poweredEntities:RemoveAt(i)
                KnapcodeOilField.Helpers.RemoveEntity(context.Grid, entity.Center, entity.Width, entity.Height)
              end
            end
          end

          entitiesToPowerFirst = nil
        end

        -- Visualizer.Show(context.Grid, poweredEntities.Where((e, i) => !coveredEntities[i]).Select(e => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(e.Center.X, e.Center.Y)), Array.Empty<DelaunatorSharp.IEdge>());

        retryStrategy = retryStrategy - 1
        if retryStrategy == 2 --[[RetryStrategy.RemoveUnpoweredBeacons]] and not hasBeacons then
          retryStrategy = retryStrategy - 1
        end
      end
    end
    AddElectricPolesAroundEntities1 = function (context, poweredEntities, entitiesToPowerFirst)
      local allCandidateToInfo, coveredEntities, electricPoles2 = KnapcodeOilField.Helpers.GetElectricPoleCandidateToCovered(context, poweredEntities, class.CandidateFactory.Instance, true, KnapcodeOilField.ElectricPoleCandidateInfo):Deconstruct()

      local electricPoleList = KnapcodeFactorioTools.CollectionExtensions.ToTableArray(electricPoles2:getKeys(), KnapcodeOilField.Location)

      PopulateCandidateToInfo(context, allCandidateToInfo, entitiesToPowerFirst, poweredEntities, electricPoleList)

      local coveredToCandidates = KnapcodeOilField.Helpers.GetCoveredToCandidates(context, allCandidateToInfo, coveredEntities, KnapcodeOilField.ElectricPoleCandidateInfo)

      local allSubsets = QueueSortedBatches_1ElectricPoleCandidateInfo()

      local sorter
      if entitiesToPowerFirst == nil then
        sorter = class.CandidateComparerForSameCoveredCount.Instance
      else
        sorter = class.CandidateComparerForSamePriorityPowered.Instance
        local priorityToLocationToInfo = DictInt32ILocationDictionary_1ElectricPoleCandidateInfo()
        for _, infoPair in System.each(allCandidateToInfo:EnumeratePairs()) do
          local default, locationToInfo = priorityToLocationToInfo:TryGetValue(infoPair[2].PriorityPowered, nil)
          if not default then
            locationToInfo = context:GetLocationDictionary(KnapcodeOilField.ElectricPoleCandidateInfo)
            priorityToLocationToInfo:AddKeyValue(infoPair[2].PriorityPowered, locationToInfo)
          end

          locationToInfo:Add(infoPair[1], infoPair[2])
        end

        allSubsets:Enqueue(SortedBatches_1ElectricPoleCandidateInfo(priorityToLocationToInfo, false))
      end

      local coveredToLocationToInfo = DictInt32ILocationDictionary_1ElectricPoleCandidateInfo()
      for _, infoPair in System.each(allCandidateToInfo:EnumeratePairs()) do
        local default, locationToInfo = coveredToLocationToInfo:TryGetValue(infoPair[2].Covered.TrueCount, nil)
        if not default then
          locationToInfo = context:GetLocationDictionary(KnapcodeOilField.ElectricPoleCandidateInfo)
          coveredToLocationToInfo:AddKeyValue(infoPair[2].Covered.TrueCount, locationToInfo)
        end

        locationToInfo:Add(infoPair[1], infoPair[2])
      end

      local coveredCountBatches = SortedBatches_1ElectricPoleCandidateInfo(coveredToLocationToInfo, false)

      allSubsets:Enqueue(coveredCountBatches)

      local roundedReach = System.ToInt32(math.Ceiling(context.Options:getElectricPoleWireReach()))
      local candidateToInfo = nil

      while coveredEntities:Any(false) do
        local continue
        repeat
          if candidateToInfo == nil then
            if #allSubsets == 0 then
              -- There are no more candidates or the candidates do not fit. No solution exists given the current grid (e.g.
              -- existing pipe placement eliminates all electric pole options).
              return System.ValueTuple(nil, coveredEntities)
            end

            local subsets = allSubsets:Peek()
            if #subsets.Queue == 0 or (#allSubsets > 1 and #subsets.Queue == 1) then
              allSubsets:Dequeue()
              continue = true
              break
            end

            candidateToInfo = subsets.Queue:Peek()
            if candidateToInfo:getCount() == 0 then
              candidateToInfo = nil
              subsets.Queue:Dequeue()
              continue = true
              break
            end
          end

          local candidateInfo = nil
          local candidate = nil
          for _, pair in System.each(candidateToInfo:EnumeratePairs()) do
            local continue
            repeat
              if candidateInfo == nil then
                candidateInfo = pair[2]
                candidate = pair[1]
                continue = true
                break
              end

              local c = sorter:Compare(pair[2], candidateInfo)
              if c < 0 then
                candidateInfo = pair[2]
                candidate = pair[1]
              end
              continue = true
            until 1
            if not continue then
              break
            end
          end

          if candidateInfo == nil then
            System.throw(KnapcodeFactorioTools.FactorioToolsException("A candidate should have been found."))
          end

          if not allCandidateToInfo:ContainsKey(candidate) then
            candidateToInfo:Remove(candidate)

            if candidateToInfo:getCount() == 0 then
              candidateToInfo = nil
            end

            continue = true
            break
          end

          KnapcodeOilField.Validate.CandidateCoversMoreEntities(context, poweredEntities, coveredEntities, candidate, candidateInfo)

          KnapcodeOilField.Helpers.AddProviderAndAllowMultipleProviders(context, candidate, candidateInfo, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight, poweredEntities, coveredEntities, coveredToCandidates, allCandidateToInfo, candidateToInfo, coveredCountBatches, KnapcodeOilField.ElectricPoleCandidateInfo)

          -- Visualizer.Show(context.Grid, Array.Empty<DelaunatorSharp.IPoint>(), Array.Empty<DelaunatorSharp.IEdge>());

          electricPoleList:Add(candidate)

          UpdateCandidateInfo(context, allCandidateToInfo, roundedReach, candidate)

          if candidateToInfo:getCount() == 0 then
            candidateToInfo = nil
            continue = true
            break
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      return System.ValueTuple(electricPoleList, coveredEntities)
    end
    PopulateCandidateToInfo = function (context, candidateToInfo, entitiesToPowerFirst, poweredEntities, electricPoleList)
      for _, default in System.each(candidateToInfo:EnumeratePairs()) do
        local candidate, info = default:Deconstruct()
        if entitiesToPowerFirst ~= nil then
          info.PriorityPowered = KnapcodeOilField.CustomCountedBitArray(entitiesToPowerFirst):And(info.Covered).TrueCount
          if info.PriorityPowered > 0 then
          end
        end

        local othersConnected = 0
        local min = 2147483647 --[[Int32.MaxValue]]
        for i = 0, electricPoleList:getCount() - 1 do
          if AreElectricPolesConnected(candidate, electricPoleList:get(i), context.Options) then
            othersConnected = othersConnected + 1
          end

          local val = electricPoleList:get(i):GetEuclideanDistanceSquared(candidate)
          if val < min then
            min = val
          end
        end

        info.OthersConnected = othersConnected
        info.PoleDistance = min

        info.EntityDistance = KnapcodeOilField.Helpers.GetEntityDistance(poweredEntities, candidate, info.Covered)
        info.MiddleDistance = candidate:GetEuclideanDistanceSquared(context.Grid.Middle)
      end
    end
    UpdateCandidateInfo = function (context, candidateToInfo, roundedReach, candidate)
      local minX = math.Max(0, candidate.X - roundedReach)
      local maxX = math.Min(context.Grid.Width - 1, candidate.X + roundedReach)
      local minY = math.Max(0, candidate.Y - roundedReach)
      local maxY = math.Min(context.Grid.Height - 1, candidate.Y + roundedReach)

      for x = minX, maxX do
        for y = minY, maxY do
          local other = KnapcodeOilField.Location(x, y)
          local distanceSquared = GetElectricPoleDistanceSquared(candidate, other, context.Options)

          local default, info = candidateToInfo:TryGetValue(other)
          if default then
            if distanceSquared <= context.Options.ElectricPoleWireReachSquared then
              local extern = info
              extern.OthersConnected = extern.OthersConnected + 1
            end

            if distanceSquared < info.PoleDistance then
              info.PoleDistance = distanceSquared
            end
          end
        end
      end
    end
    AddElectricPole = function (context, electricPoles, center)
      local centerEntity = KnapcodeOilField.ElectricPoleCenter(context.Grid:GetId())

      KnapcodeOilField.Helpers.AddProviderToGrid(context.Grid, center, centerEntity, function (c)
        return KnapcodeOilField.ElectricPoleSide(context.Grid:GetId(), c)
      end, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight, KnapcodeOilField.ElectricPoleCenter, KnapcodeOilField.ElectricPoleSide)

      electricPoles:Add(center, centerEntity)
      ConnectExistingElectricPoles(context, electricPoles, center, centerEntity)

      return centerEntity
    end
    ConnectElectricPoles = function (context, electricPoles)
      local groups = GetElectricPoleGroups(context, electricPoles)

      while groups:getCount() > 1 do
        local continue
        repeat
          local closest = nil
          local closestDistanceSquared = 0
          local lines = KnapcodeOilField.Helpers.PointsToLines(electricPoles:getKeys())
          for i = 0, lines:getCount() - 1 do
            local continue
            repeat
              local endpoint = lines:get(i)
              local groupA = KnapcodeFactorioTools.CollectionExtensions.Single1(groups:EnumerateItems(), function (g)
                return g:Contains(endpoint.A)
              end, KnapcodeOilField.ILocationSet)
              local groupB = KnapcodeFactorioTools.CollectionExtensions.Single1(groups:EnumerateItems(), function (g)
                return g:Contains(endpoint.B)
              end, KnapcodeOilField.ILocationSet)
              if groupA == groupB then
                continue = true
                break
              end

              local distanceSquared = GetElectricPoleDistanceSquared(endpoint.A, endpoint.B, context.Options)
              if distanceSquared <= context.Options.ElectricPoleWireReachSquared then
                continue = true
                break
              end

              if closest == nil or distanceSquared < closestDistanceSquared then
                closest = endpoint
                closestDistanceSquared = distanceSquared
              end
              continue = true
            until 1
            if not continue then
              break
            end
          end

          if closest == nil then
            System.throw(KnapcodeFactorioTools.FactorioToolsException("No closest electric pole could be found."))
          end

          AddSinglePoleForConnection(context, electricPoles, groups, math.Sqrt(closestDistanceSquared), closest)
          continue = true
        until 1
        if not continue then
          break
        end
      end
    end
    AddSinglePoleForConnection = function (context, electricPoles, groups, distance, endpoints)
      local segments = System.ToInt32(math.Ceiling(distance / context.Options:getElectricPoleWireReach()))
      local idealLine = KnapcodeOilField.BresenhamsLine.GetPath(endpoints.A, endpoints.B)
      local idealIndex = System.div(idealLine:getCount(), segments)
      if not AreElectricPolesConnected(idealLine:get(0), idealLine:get(idealIndex), context.Options) then
        idealIndex = idealIndex - 1
      end
      local idealPoint = idealLine:get(idealIndex)

      local selectedPoint = KnapcodeOilField.Location.getInvalid()
      local matchFound = false


      local candidates = QueueLocation()
      local attempted = context:GetLocationSet1()
      candidates:Enqueue(idealPoint)
      attempted:Add(idealPoint)

      local neighbors = SpanLocation.ctorArray(ArrayLocation(4))


      while #candidates > 0 do
        local candidate = candidates:Dequeue()
        if KnapcodeOilField.Helpers.DoesProviderFit(context.Grid, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight, candidate) and KnapcodeOilField.Helpers.IsProviderInBounds(context.Grid, context.Options.ElectricPoleWidth, context.Options.ElectricPoleHeight, candidate) then
          selectedPoint = candidate
          matchFound = true
          break
        end

        context.Grid:GetAdjacent(neighbors, candidate)
        for i = 0, neighbors:getLength() - 1 do
          if neighbors:get(i).IsValid and AreElectricPolesConnected(idealLine:get(0), neighbors:get(i), context.Options) and attempted:Add(neighbors:get(i)) then
            candidates:Enqueue(neighbors:get(i))
          end
        end
      end

      if not matchFound then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("Could not find a pole that can be connected."))
      end

      local center = AddElectricPole(context, electricPoles, selectedPoint)
      local connectedGroups = KnapcodeOilField.TableArray.New1(groups:getCount(), KnapcodeOilField.ILocationSet)
      for i = 0, groups:getCount() - 1 do
        local group = groups:get(i)
        local match = false
        for _, id in System.each(center:getNeighbors()) do
          local location = context.Grid:getEntityIdToLocation():get(id)
          if group:Contains(location) then
            match = true
            break
          end
        end

        if match then
          connectedGroups:Add(group)
        end
      end

      if connectedGroups:getCount() == 0 then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("Could not find the group containing the selected electric pole."))
      end

      connectedGroups:get(0):Add(selectedPoint)
      for i = 1, connectedGroups:getCount() - 1 do
        connectedGroups:get(0):UnionWith1(connectedGroups:get(i))
        groups:Remove(connectedGroups:get(i))
      end

      -- Visualizer.Show(context.Grid, Array.Empty<IPoint>(), Array.Empty<IEdge>());
    end
    GetElectricPoleGroups = function (context, electricPoles)
      local groups = KnapcodeOilField.TableArray.New(KnapcodeOilField.ILocationSet)
      local remaining = KnapcodeFactorioTools.CollectionExtensions.ToSet1(electricPoles:getKeys(), context, true)
      while remaining:getCount() > 0 do
        local current = KnapcodeFactorioTools.CollectionExtensions.First(remaining:EnumerateItems(), KnapcodeOilField.Location)
        remaining:Remove(current)

        local entityIds = HashSetInt32()
        local explore = QueueElectricPoleCenter()
        explore:Enqueue(electricPoles:get(current))

        while #explore > 0 do
          local entity = explore:Dequeue()
          if entityIds:Add(entity.Id) then
            for _, id in System.each(entity:getNeighbors()) do
              explore:Enqueue(context.Grid:GetEntity(id, KnapcodeOilField.ElectricPoleCenter))
            end
          end
        end

        local group = context:GetLocationSet2(true)
        for _, entityId in System.each(entityIds) do
          local location = context.Grid:getEntityIdToLocation():get(entityId)
          group:Add(location)
        end

        remaining:ExceptWith(group)
        groups:Add(group)
      end

      return groups
    end
    class = {
      Execute = Execute,
      AreElectricPolesConnected = AreElectricPolesConnected
    }
    return class
  end)
end)
