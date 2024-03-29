-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioTools
local KnapcodeOilField
local AddPipesConnectedCenters
local ListTrunk
local ListLocation
local ArrayLocation
local ListPumpjackGroup
local QueueExploreCenter
local ListTerminalLocation
local KeyValuePairLocationILocationSet
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  AddPipesConnectedCenters = Knapcode.FactorioTools.OilField.AddPipesConnectedCenters
  ListTrunk = System.List(AddPipesConnectedCenters.Trunk)
  ListLocation = System.List(KnapcodeOilField.Location)
  ArrayLocation = System.Array(KnapcodeOilField.Location)
  ListPumpjackGroup = System.List(AddPipesConnectedCenters.PumpjackGroup)
  QueueExploreCenter = System.Queue(AddPipesConnectedCenters.ExploreCenter)
  ListTerminalLocation = System.List(KnapcodeOilField.TerminalLocation)
  KeyValuePairLocationILocationSet = System.KeyValuePair(KnapcodeOilField.Location, KnapcodeOilField.ILocationSet)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("AddPipesConnectedCenters", function (namespace)
    local Translations, GetConnectedPumpjacks, FindTrunksAndConnect, GetShortestPathToGroup, GetCentroidDistanceSquared, FindTrunks, ConnectTwoClosestPumpjacks, GetChildCenters, 
    GetTrunkCandidates, class, static
    namespace.class("GroupCandidate", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, Group, Center, IncludedCenter, Terminal, Path)
        this.Group = Group
        this.Center = Center
        this.IncludedCenter = IncludedCenter
        this.Terminal = Terminal
        this.Path = Path
      end
      __members__ = function ()
        return "GroupCandidate", "Group", "Center", "IncludedCenter", "Terminal", "Path"
      end
      return {
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipesConnectedCenters.GroupCandidate)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("BestConnection", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, Path, Terminal, BestTerminal)
        this.Path = Path
        this.Terminal = Terminal
        this.BestTerminal = BestTerminal
      end
      __members__ = function ()
        return "BestConnection", "Path", "Terminal", "BestTerminal"
      end
      return {
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipesConnectedCenters.BestConnection)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("ExploreCenter", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, Location, ShouldRecurse)
        this.Location = Location
        this.ShouldRecurse = ShouldRecurse
      end
      __members__ = function ()
        return "ExploreCenter", "Location", "ShouldRecurse"
      end
      return {
        ShouldRecurse = false,
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipesConnectedCenters.ExploreCenter)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("Trunk", function (namespace)
      local getLength, getStart, getEnd, GetTrunkEndDistance, ToString, __ctor__
      __ctor__ = function (this, context, startingTerminal, center)
        this.Terminals = ListTerminalLocation(2)
        this._context = context
        this.TerminalLocations = context:GetLocationSet7(startingTerminal.Terminal, 2)
        this.Terminals:Add(startingTerminal)
        this.Centers = context:GetLocationSet8(center, 2, true)
      end
      getLength = function (this)
        return getStart(this):GetManhattanDistance(getEnd(this)) + 1
      end
      getStart = function (this)
        return this.Terminals:get(0).Terminal
      end
      getEnd = function (this)
        return this.Terminals:get(#this.Terminals - 1).Terminal
      end
      GetTrunkEndDistance = function (this, centerToConnectedCenters)
        if (this._trunkEndDistance ~= nil) then
          return System.Nullable.Value(this._trunkEndDistance)
        end

        local neighbors = this._context:GetLocationSet2(true)
        for _, center in System.each(this.Centers:EnumerateItems()) do
          for _, otherCenter in System.each(centerToConnectedCenters:get(center):EnumerateItems()) do
            neighbors:Add(otherCenter)
          end
        end

        neighbors:ExceptWith(this.Centers)

        if neighbors:getCount() == 0 then
          this._trunkEndDistance = 0
          return System.Nullable.Value(this._trunkEndDistance)
        end

        local centroidX = KnapcodeFactorioTools.CollectionExtensions.Average(neighbors:EnumerateItems(), function (l)
          return l.X
        end, KnapcodeOilField.Location)
        local centroidY = KnapcodeFactorioTools.CollectionExtensions.Average(neighbors:EnumerateItems(), function (l)
          return l.Y
        end, KnapcodeOilField.Location)
        this._trunkEndDistance = getStart(this):GetEuclideanDistance1(centroidX, centroidY) + getEnd(this):GetEuclideanDistance1(centroidX, centroidY)
        return System.Nullable.Value(this._trunkEndDistance)
      end
      ToString = function (this)
        return System.toString(getStart(this)) .. " -> " .. System.toString(getEnd(this))
      end
      return {
        OriginalIndex = 0,
        getLength = getLength,
        getStart = getStart,
        getEnd = getEnd,
        GetTrunkEndDistance = GetTrunkEndDistance,
        ToString = ToString,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("PumpjackGroup", function (namespace)
      local GetChildCentroidDistanceSquared, ConnectPumpjack, MergeGroup, UpdateFrontierCenters, UpdateIncludedCenterToChildCenters, __ctor1__, __ctor2__
      __ctor1__ = function (this, context, centerToConnectedCenters, allIncludedCenters, trunk)
        __ctor2__(this, context, centerToConnectedCenters, allIncludedCenters, trunk.Centers:EnumerateItems(), KnapcodeOilField.Helpers.MakeStraightLine(trunk:getStart(), trunk:getEnd()))
      end
      __ctor2__ = function (this, context, centerToConnectedCenters, allIncludedCenters, includedCenters, pipes)
        this._context = context
        this._centerToConnectedCenters = centerToConnectedCenters
        this._allIncludedCenters = allIncludedCenters

        this.IncludedCenters = KnapcodeFactorioTools.CollectionExtensions.ToReadOnlySet1(includedCenters, context, true)

        this.FrontierCenters = context:GetLocationSet2(true)
        this.IncludedCenterToChildCenters = context:GetLocationDictionary(KnapcodeOilField.ILocationSet)

        this.Pipes = KnapcodeFactorioTools.CollectionExtensions.ToSet(pipes, context, true)

        UpdateFrontierCenters(this)
        UpdateIncludedCenterToChildCenters(this)
      end
      GetChildCentroidDistanceSquared = function (this, includedCenter, terminalCandidate)
        local sumX = 0
        local sumY = 0
        local count = 0
        for _, center in System.each(this.IncludedCenterToChildCenters:get(includedCenter):EnumerateItems()) do
          sumX = sumX + center.X
          sumY = sumY + center.Y
          count = count + 1
        end

        local centroidX = sumX / count
        local centroidY = sumY / count

        return terminalCandidate:GetEuclideanDistanceSquared2(centroidX, centroidY)
      end
      ConnectPumpjack = function (this, center, path)
        this._allIncludedCenters:Add(center)
        this.IncludedCenters:Add(center)
        this.Pipes:UnionWith(path)
        UpdateFrontierCenters(this)
        UpdateIncludedCenterToChildCenters(this)
      end
      MergeGroup = function (this, other, path)
        this.IncludedCenters:UnionWith1(other.IncludedCenters)
        this.Pipes:UnionWith(path)
        this.Pipes:UnionWith1(other.Pipes)
        UpdateFrontierCenters(this)
        UpdateIncludedCenterToChildCenters(this)
      end
      UpdateFrontierCenters = function (this)
        this.FrontierCenters:Clear()

        for _, center in System.each(this.IncludedCenters:EnumerateItems()) do
          this.FrontierCenters:UnionWith1(this._centerToConnectedCenters:get(center))
        end

        this.FrontierCenters:ExceptWith(this.IncludedCenters)
      end
      UpdateIncludedCenterToChildCenters = function (this)
        this.IncludedCenterToChildCenters:Clear()

        for _, center in System.each(this.IncludedCenters:EnumerateItems()) do
          local visited = GetChildCenters(this._context, this._centerToConnectedCenters, this.IncludedCenters, this._allIncludedCenters, center)

          this.IncludedCenterToChildCenters:Add(center, visited)
        end
      end
      return {
        GetChildCentroidDistanceSquared = GetChildCentroidDistanceSquared,
        ConnectPumpjack = ConnectPumpjack,
        MergeGroup = MergeGroup,
        __ctor__ = {
          __ctor1__,
          __ctor2__
        }
      }
    end)
    static = function (this)
      Translations = ArrayLocation(2, {
        KnapcodeOilField.Location(1, 0),
        KnapcodeOilField.Location(0, 1)
      })
    end
    GetConnectedPumpjacks = function (context, strategy)
      local centers = context.Centers

      if #centers == 2 then
        local simpleConnectedCenters = context:GetLocationDictionary(KnapcodeOilField.ILocationSet)
        simpleConnectedCenters:Add(centers:get(0), context:GetSingleLocationSet(centers:get(1)))
        simpleConnectedCenters:Add(centers:get(1), context:GetSingleLocationSet(centers:get(0)))
        return simpleConnectedCenters
      end

      -- Check that nodes are not collinear
      if KnapcodeOilField.Helpers.AreLocationsCollinear(centers) then
        local connected = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(centers, context, function (c)
          return c
        end, function (c)
          return context:GetLocationSet2(true)
        end, KnapcodeOilField.Location, KnapcodeOilField.ILocationSet)
        for j = 1, #centers - 1 do
          connected:get(centers:get(j - 1)):Add(centers:get(j))
          connected:get(centers:get(j)):Add(centers:get(j - 1))
        end

        return connected
      end

      local default
      local extern = strategy
      if extern == 2 --[[PipeStrategy.ConnectedCentersDelaunay]] then
        default = KnapcodeOilField.AddPipesConnectedCentersDT.ExecuteWithDelaunay(context, centers)
      elseif extern == 3 --[[PipeStrategy.ConnectedCentersDelaunayMst]] then
        default = KnapcodeOilField.AddPipesConnectedCentersDT.ExecuteWithDelaunayMst(context, centers)
      elseif extern == 4 --[[PipeStrategy.ConnectedCentersFlute]] then
        default = KnapcodeOilField.AddPipesConnectedCentersFLUTE.Execute(context)
      else
        default = System.throw(System.NotImplementedException())
      end
      local connectedCenters = default

      -- check if all connected centers have edges in both directions
      if context.Options.ValidateSolution then
        for _, ref in System.each(connectedCenters:EnumeratePairs()) do
          local center, others = ref:Deconstruct()
          for _, other in System.each(others:EnumerateItems()) do
            if not connectedCenters:get(other):Contains(center) then
              System.throw(KnapcodeFactorioTools.FactorioToolsException("The edges in the connected centers graph are not bidirectional."))
            end
          end
        end
      end

      -- VisualizeConnectedCenters(context, connectedCenters);

      return connectedCenters
    end
    FindTrunksAndConnect = function (context, centerToConnectedCenters)
      local selectedTrunks = FindTrunks(context, centerToConnectedCenters)

      local allIncludedCenters = context:GetLocationSet5(#selectedTrunks * 2)
      for i = 0, #selectedTrunks - 1 do
        for _, center in System.each(selectedTrunks:get(i).Centers:EnumerateItems()) do
          allIncludedCenters:Add(center)
        end
      end

      local groups = ListPumpjackGroup(#selectedTrunks)
      for i = 0, #selectedTrunks - 1 do
        groups:Add(class.PumpjackGroup(context, centerToConnectedCenters, allIncludedCenters, selectedTrunks:get(i)))
      end

      if #groups == 0 then
        local group = ConnectTwoClosestPumpjacks(context, centerToConnectedCenters, allIncludedCenters)

        groups:Add(group)
      end

      --[[
        var clone = new PipeGrid(context.Grid);
        Visualizer.Show(clone, groups.SelectMany(g => g.Pipes).Distinct(context).Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
        ]]

      while #groups > 1 or groups:get(0).IncludedCenters:getCount() < context.CenterToTerminals:getCount() do
        local continue
        repeat
          local shortestDistance = nil
          local candidate = nil

          for _, group in System.each(groups) do
            local continue
            repeat
              local centroidX = KnapcodeFactorioTools.CollectionExtensions.Average(group.Pipes:EnumerateItems(), function (l)
                return l.X
              end, KnapcodeOilField.Location)
              local centroidY = KnapcodeFactorioTools.CollectionExtensions.Average(group.Pipes:EnumerateItems(), function (l)
                return l.Y
              end, KnapcodeOilField.Location)

              for _, center in System.each(group.FrontierCenters:EnumerateItems()) do
                local continue
                repeat
                  local includedCenter = KnapcodeFactorioTools.CollectionExtensions.First1(group.IncludedCenterToChildCenters:EnumeratePairs(), function (p)
                    return p[2]:Contains(center)
                  end, KeyValuePairLocationILocationSet)[1]

                  -- Prefer the terminal that has the shortest path, then prefer the terminal closer to the centroid
                  -- of the child (unconnected) pumpjacks.
                  for _, terminal in System.each(context.CenterToTerminals:get(center)) do
                    local continue
                    repeat
                      local result = GetShortestPathToGroup(context, terminal, group, centroidX, centroidY)
                      if result.Exception ~= nil then
                        return KnapcodeFactorioTools.Result.NewException(result.Exception, KnapcodeOilField.ILocationSet)
                      end

                      local path = result.Data

                      if candidate == nil then
                        candidate = class.GroupCandidate(group, center, includedCenter, terminal, path)
                        continue = true
                        break
                      end

                      local comparison = System.Int32.CompareTo((#path), #candidate.Path)
                      if comparison < 0 then
                        candidate = class.GroupCandidate(group, center, includedCenter, terminal, path)
                        shortestDistance = nil
                        continue = true
                        break
                      end

                      if comparison > 0 then
                        continue = true
                        break
                      end

                      if not (shortestDistance ~= nil) then
                        shortestDistance = candidate.Group:GetChildCentroidDistanceSquared(candidate.IncludedCenter, candidate.Terminal.Terminal)
                      end

                      local distance = group:GetChildCentroidDistanceSquared(includedCenter, terminal.Terminal)
                      comparison = System.Double.CompareTo(distance, System.Nullable.Value(shortestDistance))
                      if comparison < 0 then
                        candidate = class.GroupCandidate(group, center, includedCenter, terminal, path)
                        shortestDistance = distance
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

          if candidate == nil then
            System.throw(KnapcodeFactorioTools.FactorioToolsException("No group candidate was found."))
          end

          if allIncludedCenters:Contains(candidate.Terminal.Center) then
            local otherGroup = KnapcodeFactorioTools.CollectionExtensions.Single1(groups, function (g)
              return g.IncludedCenters:Contains(candidate.Terminal.Center)
            end, class.PumpjackGroup)
            candidate.Group:MergeGroup(otherGroup, candidate.Path)
            groups:Remove(otherGroup)

            --[[
                var clone2 = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone2, new(), context.CenterToTerminals, groups.SelectMany(g => g.Pipes).ToSet(), allowMultipleTerminals: true);
                Visualizer.Show(clone2, path.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                ]]
          else
            -- Add the newly connected pumpjack to the current group.
            candidate.Group:ConnectPumpjack(candidate.Center, candidate.Path)
            KnapcodeOilField.Helpers.EliminateOtherTerminals(context, candidate.Terminal)

            --[[
                var clone2 = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone2, new(), context.CenterToTerminals, groups.SelectMany(g => g.Pipes).ToSet(), allowMultipleTerminals: true);
                Visualizer.Show(clone2, path.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
                ]]
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      return KnapcodeFactorioTools.Result.NewData(KnapcodeFactorioTools.CollectionExtensions.Single(groups, class.PumpjackGroup).Pipes, KnapcodeOilField.ILocationSet)
    end
    GetShortestPathToGroup = function (context, terminal, group, groupCentroidX, groupCentroidY)
      local aStarResultV = KnapcodeOilField.AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, true, 2, 1)
      local aStarResultH = KnapcodeOilField.AStar.GetShortestPath(context, context.Grid, terminal.Terminal, group.Pipes, true, 1, 2)
      if not aStarResultV.Success then
        return KnapcodeFactorioTools.Result.NewException(KnapcodeFactorioTools.NoPathBetweenTerminalsException(terminal.Terminal, KnapcodeFactorioTools.CollectionExtensions.First(group.Pipes:EnumerateItems(), KnapcodeOilField.Location)), ListLocation)
      end

      if KnapcodeFactorioTools.CollectionExtensions.SequenceEqual(aStarResultV:getPath(), aStarResultH:getPath(), KnapcodeOilField.Location) then
        return KnapcodeFactorioTools.Result.NewData(KnapcodeFactorioTools.CollectionExtensions.ToList(aStarResultV:getPath(), KnapcodeOilField.Location), ListLocation)
      end

      local adjacentPipesV = 0
      local centroidDistanceSquaredV = 0

      local adjacentPipesH = 0
      local centroidDistanceSquaredH = 0

      local sizeEstimate = #aStarResultV:getPath() + #aStarResultH:getPath()


      local locationToCentroidDistanceSquared = context:GetLocationDictionary1(sizeEstimate, System.Double)
      local width = context.Grid.Width
      do
        local i = 0
        while i < math.Max(#aStarResultV:getPath(), #aStarResultH:getPath()) do
          if i < #aStarResultV:getPath() then
            local location = aStarResultV:getPath():get(i)
            if context.LocationToAdjacentCount:get(location.Y * width + location.X) > 0 then
              adjacentPipesV = adjacentPipesV + 1
            end

            centroidDistanceSquaredV = centroidDistanceSquaredV + GetCentroidDistanceSquared(groupCentroidX, groupCentroidY, locationToCentroidDistanceSquared, location)
          end

          if i < #aStarResultH:getPath() then
            local location = aStarResultH:getPath():get(i)
            if context.LocationToAdjacentCount:get(location.Y * width + location.X) > 0 then
              adjacentPipesH = adjacentPipesH + 1
            end

            centroidDistanceSquaredH = centroidDistanceSquaredH + GetCentroidDistanceSquared(groupCentroidX, groupCentroidY, locationToCentroidDistanceSquared, location)
          end
          i = i + 1
        end
      end

      if adjacentPipesV > adjacentPipesH then
        return KnapcodeFactorioTools.Result.NewData(KnapcodeFactorioTools.CollectionExtensions.ToList(aStarResultV:getPath(), KnapcodeOilField.Location), ListLocation)
      elseif adjacentPipesV < adjacentPipesH then
        return KnapcodeFactorioTools.Result.NewData(KnapcodeFactorioTools.CollectionExtensions.ToList(aStarResultH:getPath(), KnapcodeOilField.Location), ListLocation)
      elseif centroidDistanceSquaredV < centroidDistanceSquaredH then
        return KnapcodeFactorioTools.Result.NewData(KnapcodeFactorioTools.CollectionExtensions.ToList(aStarResultV:getPath(), KnapcodeOilField.Location), ListLocation)
      else
        return KnapcodeFactorioTools.Result.NewData(KnapcodeFactorioTools.CollectionExtensions.ToList(aStarResultH:getPath(), KnapcodeOilField.Location), ListLocation)
      end
    end
    GetCentroidDistanceSquared = function (groupCentroidX, groupCentroidY, locationToCentroidDistanceSquared, location)
      local default, centroidDistanceSquared = locationToCentroidDistanceSquared:TryGetValue(location)
      if not default then
        centroidDistanceSquared = location:GetEuclideanDistanceSquared2(groupCentroidX, groupCentroidY)
        locationToCentroidDistanceSquared:Add(location, centroidDistanceSquared)
      end

      return centroidDistanceSquared
    end
    FindTrunks = function (context, centerToConnectedCenters)
      --[[
        Visualizer.Show(context.Grid, Array.Empty<IPoint>(), centerToConnectedCenters
            .SelectMany(p => p.Value.Select(o => (p.Key, o))
            .Select(p => (IEdge)new Edge(0, new Point(p.Key.X, p.Key.Y), new Point(p.o.X, p.o.Y)))
            .Distinct()));
        ]]

      local trunkCandidates = GetTrunkCandidates(context, centerToConnectedCenters)

      trunkCandidates:Sort(function (a, b)
        local c = System.Int32.CompareTo(b.TerminalLocations:getCount(), a.TerminalLocations:getCount())
        if c ~= 0 then
          return c
        end

        c = System.Int32.CompareTo(a:getLength(), b:getLength())
        if c ~= 0 then
          return c
        end

        local aC = a:GetTrunkEndDistance(centerToConnectedCenters)
        local bC = b:GetTrunkEndDistance(centerToConnectedCenters)
        c = System.Double.CompareTo(aC, bC)
        if c ~= 0 then
          return c
        end

        return System.Int32.CompareTo(a.OriginalIndex, b.OriginalIndex)
      end)

      -- Eliminate lower priority trunks that have any pipes shared with higher priority trunks.
      local includedPipes = context:GetLocationSet1()
      local includedCenters = context:GetLocationSet2(true)
      local selectedTrunks = ListTrunk()
      for _, trunk in System.each(trunkCandidates) do
        local path = KnapcodeOilField.Helpers.MakeStraightLine(trunk:getStart(), trunk:getEnd())
        if not includedPipes:Overlaps(path) and not includedCenters:Overlaps(trunk.Centers:EnumerateItems()) then
          selectedTrunks:Add(trunk)
          includedPipes:UnionWith(path)
          includedCenters:UnionWith1(trunk.Centers)
        end
      end

      --[[
        for (var i = 1; i <= selectedTrunks.Count; i++)
        {
            Visualizer.Show(context.Grid, selectedTrunks.Take(i).SelectMany(t => t.Centers).Distinct(context).Select(l => (IPoint)new Point(l.X, l.Y)), selectedTrunks
                .Take(i)
                .Select(t => (IEdge)new Edge(0, new Point(t.Start.X, t.Start.Y), new Point(t.End.X, t.End.Y)))
                .ToList());
        }
        ]]

      -- Eliminate unused terminals for pumpjacks included in all of the trunks. A pumpjack connected to a trunk has
      -- its terminal selected.
      for _, trunk in System.each(selectedTrunks) do
        for _, terminal in System.each(trunk.Terminals) do
          KnapcodeOilField.Helpers.EliminateOtherTerminals(context, terminal)
        end
      end

      -- Visualize(context, locationToPoint, selectedTrunks.SelectMany(t => MakeStraightLine(t.Start, t.End)).ToSet());

      -- Find the "child" unconnected pumpjacks of each connected pumpjack. These are pumpjacks are connected via the
      -- given connected pumpjack.
      return selectedTrunks
    end
    ConnectTwoClosestPumpjacks = function (context, centerToConnectedCenters, allIncludedCenters)
      local centerToGoals = context:GetLocationDictionary(KnapcodeOilField.ILocationSet)
      local bestConnection = nil
      local bestConnectedCentersCount = 0
      local bestOtherConnectedCentersCount = nil
      local bestTerminalDistance = nil
      local bestOtherTerminalDistance = nil

      for i = 0, #context.Centers - 1 do
        local continue
        repeat
          local center = context.Centers:get(i)
          local terminals = context.CenterToTerminals:get(center)

          for j = 0, #terminals - 1 do
            local continue
            repeat
              local terminal = terminals:get(j)
              local connectedCenters = centerToConnectedCenters:get(center)

              for _, otherCenter in System.each(connectedCenters:EnumerateItems()) do
                local continue
                repeat
                  local otherTerminals = context.CenterToTerminals:get(otherCenter)

                  local default, goals = centerToGoals:TryGetValue(otherCenter)
                  if not default then
                    goals = context:GetLocationSet2(true)
                    for k = 0, #otherTerminals - 1 do
                      goals:Add(otherTerminals:get(k).Terminal)
                    end

                    centerToGoals:Add(otherCenter, goals)
                  end

                  local result = KnapcodeOilField.AStar.GetShortestPath(context, context.Grid, terminal.Terminal, goals, true, 1, 1)
                  if not result.Success then
                    System.throw(KnapcodeFactorioTools.NoPathBetweenTerminalsException(terminal.Terminal, KnapcodeFactorioTools.CollectionExtensions.First(goals:EnumerateItems(), KnapcodeOilField.Location)))
                  end

                  local reachedGoal = result.ReachedGoal
                  local closestTerminal = KnapcodeFactorioTools.CollectionExtensions.Single1(otherTerminals, function (t)
                    return KnapcodeOilField.Location.op_Equality(t.Terminal, reachedGoal)
                  end, KnapcodeOilField.TerminalLocation)
                  local path = result:getPath()

                  if bestConnection == nil then
                    bestConnection = class.BestConnection(path, terminal, closestTerminal)
                    bestConnectedCentersCount = connectedCenters:getCount()
                    bestOtherConnectedCentersCount = nil
                    bestTerminalDistance = nil
                    bestOtherTerminalDistance = nil
                    continue = true
                    break
                  end

                  local c = System.Int32.CompareTo((#path), #bestConnection.Path)
                  if c < 0 then
                    bestConnection = class.BestConnection(path, terminal, closestTerminal)
                    bestConnectedCentersCount = connectedCenters:getCount()
                    bestOtherConnectedCentersCount = nil
                    bestTerminalDistance = nil
                    bestOtherTerminalDistance = nil
                    continue = true
                    break
                  elseif c > 0 then
                    continue = true
                    break
                  end

                  c = System.Int32.CompareTo(connectedCenters:getCount(), bestConnectedCentersCount)
                  if c > 0 then
                    bestConnection = class.BestConnection(path, terminal, closestTerminal)
                    bestConnectedCentersCount = connectedCenters:getCount()
                    bestOtherConnectedCentersCount = nil
                    bestTerminalDistance = nil
                    bestOtherTerminalDistance = nil
                    continue = true
                    break
                  elseif c < 0 then
                    continue = true
                    break
                  end

                  if not (bestOtherConnectedCentersCount ~= nil) then
                    bestOtherConnectedCentersCount = centerToConnectedCenters:get(bestConnection.BestTerminal.Center):getCount()
                  end

                  local otherConnectedCentersCount = centerToConnectedCenters:get(otherCenter):getCount()
                  c = System.Int32.CompareTo(otherConnectedCentersCount, System.Nullable.Value(bestOtherConnectedCentersCount))
                  if c > 0 then
                    bestConnection = class.BestConnection(path, terminal, closestTerminal)
                    bestConnectedCentersCount = connectedCenters:getCount()
                    bestOtherConnectedCentersCount = otherConnectedCentersCount
                    bestTerminalDistance = nil
                    bestOtherTerminalDistance = nil
                    continue = true
                    break
                  elseif c < 0 then
                    continue = true
                    break
                  end

                  if not (bestTerminalDistance ~= nil) then
                    bestTerminalDistance = bestConnection.Terminal.Terminal:GetEuclideanDistanceSquared(context.Grid.Middle)
                  end

                  local terminalDistance = terminal.Terminal:GetEuclideanDistance(context.Grid.Middle)
                  c = System.Double.CompareTo(terminalDistance, System.Nullable.Value(bestTerminalDistance))
                  if c < 0 then
                    bestConnection = class.BestConnection(path, terminal, closestTerminal)
                    bestConnectedCentersCount = connectedCenters:getCount()
                    bestOtherConnectedCentersCount = otherConnectedCentersCount
                    bestTerminalDistance = terminalDistance
                    bestOtherTerminalDistance = nil
                    continue = true
                    break
                  elseif c > 0 then
                    continue = true
                    break
                  end

                  if not (bestOtherTerminalDistance ~= nil) then
                    bestOtherTerminalDistance = bestConnection.BestTerminal.Terminal:GetEuclideanDistanceSquared(context.Grid.Middle)
                  end

                  local otherTerminalDistance = closestTerminal.Terminal:GetEuclideanDistance(context.Grid.Middle)
                  c = System.Double.CompareTo(otherTerminalDistance, System.Nullable.Value(bestOtherTerminalDistance))
                  if c < 0 then
                    bestConnection = class.BestConnection(path, terminal, closestTerminal)
                    bestConnectedCentersCount = connectedCenters:getCount()
                    bestOtherConnectedCentersCount = otherConnectedCentersCount
                    bestTerminalDistance = terminalDistance
                    bestOtherTerminalDistance = otherTerminalDistance
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

      if bestConnection == nil then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("A new connection should have been found."))
      end

      KnapcodeOilField.Helpers.EliminateOtherTerminals(context, bestConnection.Terminal)
      KnapcodeOilField.Helpers.EliminateOtherTerminals(context, bestConnection.BestTerminal)

      local group = System.new(class.PumpjackGroup, 2, context, centerToConnectedCenters, allIncludedCenters, ArrayLocation(2, {
        bestConnection.Terminal.Center,
        bestConnection.BestTerminal.Center
      }), bestConnection.Path)

      return group
    end
    GetChildCenters = function (context, centerToConnectedCenters, ignoreCenters, shallowExploreCenters, startingCenter)
      local queue = QueueExploreCenter()
      local visited = context:GetLocationSet2(true)
      queue:Enqueue(class.ExploreCenter(startingCenter, true))

      while #queue > 0 do
        local continue
        repeat
          local current, shouldRecurse = queue:Dequeue():Deconstruct()
          if not visited:Add(current) or not shouldRecurse then
            continue = true
            break
          end

          for _, other in System.each(centerToConnectedCenters:get(current):EnumerateItems()) do
            local continue
            repeat
              if ignoreCenters:Contains(other) then
                continue = true
                break
              end

              -- If the other center is in another group, don't recursively explore.
              queue:Enqueue(class.ExploreCenter(other, not shallowExploreCenters:Contains(other)))
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

      visited:Remove(startingCenter)
      return visited
    end
    GetTrunkCandidates = function (context, centerToConnectedCenters)
      local centerToMaxX = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(context.Centers, context, function (c)
        return c
      end, function (c)
        return KnapcodeFactorioTools.CollectionExtensions.Max(centerToConnectedCenters:get(c):EnumerateItems(), function (c)
          return KnapcodeFactorioTools.CollectionExtensions.Max(context.CenterToTerminals:get(c), function (t)
            return t.Terminal.X
          end, KnapcodeOilField.TerminalLocation, System.Int32)
        end, KnapcodeOilField.Location, System.Int32)
      end, KnapcodeOilField.Location, System.Int32)
      local centerToMaxY = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(context.Centers, context, function (c)
        return c
      end, function (c)
        return KnapcodeFactorioTools.CollectionExtensions.Max(centerToConnectedCenters:get(c):EnumerateItems(), function (c)
          return KnapcodeFactorioTools.CollectionExtensions.Max(context.CenterToTerminals:get(c), function (t)
            return t.Terminal.Y
          end, KnapcodeOilField.TerminalLocation, System.Int32)
        end, KnapcodeOilField.Location, System.Int32)
      end, KnapcodeOilField.Location, System.Int32)

      -- Find paths that connect the most terminals of neighboring pumpjacks.
      local trunkCandidates = ListTrunk()
      for _, translation in System.each(Translations) do
        for _, startingCenter in System.each(context.Centers) do
          for _, terminal in System.each(context.CenterToTerminals:get(startingCenter)) do
            local currentCenter = startingCenter
            local expandedChildCenters = false
            local nextCenters = centerToConnectedCenters:get(currentCenter)
            local maxX = centerToMaxX:get(currentCenter)
            local maxY = centerToMaxY:get(currentCenter)

            local location = terminal.Terminal:Translate1(translation)

            local trunk = nil

            while location.X <= maxX and location.Y <= maxY and context.Grid:IsEmpty(location) do
              local default, terminals = context.LocationToTerminals:TryGetValue(location)
              if default then
                local nextCenter = KnapcodeOilField.Location.getInvalid()
                local hasMatch = false
                for _, nextTerminal in System.each(terminals) do
                  if nextCenters:Contains(nextTerminal.Center) then
                    nextCenter = nextTerminal.Center
                    hasMatch = true
                    break
                  end
                end

                if not hasMatch then
                  -- The pumpjack terminal we ran into does not belong to the a pumpjack that the current
                  -- pumpjack should be connected to.
                  break
                end

                if not expandedChildCenters then
                  nextCenters = GetChildCenters(context, centerToConnectedCenters, context:GetSingleLocationSet(currentCenter), context:GetSingleLocationSet(nextCenter), nextCenter)

                  if nextCenters:getCount() == 0 then
                    break
                  end

                  maxX = KnapcodeFactorioTools.CollectionExtensions.Max(nextCenters:EnumerateItems(), function (c)
                    return KnapcodeFactorioTools.CollectionExtensions.Max(context.CenterToTerminals:get(c), function (t)
                      return t.Terminal.X
                    end, KnapcodeOilField.TerminalLocation, System.Int32)
                  end, KnapcodeOilField.Location, System.Int32)
                  maxY = KnapcodeFactorioTools.CollectionExtensions.Max(nextCenters:EnumerateItems(), function (c)
                    return KnapcodeFactorioTools.CollectionExtensions.Max(context.CenterToTerminals:get(c), function (t)
                      return t.Terminal.Y
                    end, KnapcodeOilField.TerminalLocation, System.Int32)
                  end, KnapcodeOilField.Location, System.Int32)
                  expandedChildCenters = true
                end

                if trunk == nil then
                  trunk = class.Trunk(context, terminal, currentCenter)
                end

                trunk.Terminals:AddRange(terminals)
                for _, other in System.each(terminals) do
                  trunk.TerminalLocations:Add(other.Terminal)
                end
                for _, nextTerminal in System.each(terminals) do
                  trunk.Centers:Add(nextTerminal.Center)
                end

                currentCenter = nextCenter
              end

              location = location:Translate1(translation)
            end

            if trunk ~= nil and trunk.Centers:getCount() > 1 then
              trunk.OriginalIndex = #trunkCandidates
              trunkCandidates:Add(trunk)
            end
          end
        end
      end

      return trunkCandidates
    end
    class = {
      GetConnectedPumpjacks = GetConnectedPumpjacks,
      FindTrunksAndConnect = FindTrunksAndConnect,
      static = static
    }
    return class
  end)
end)
