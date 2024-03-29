-- Generated by CSharp.lua Compiler
local System = System
local ListBoolean = System.List(System.Boolean)
local KnapcodeFactorioTools
local KnapcodeOilField
local KnapcodeAddPipes
local ListInt32
local ListPlanInfo
local ListSolution
local NullableInt32
local ListOilFieldPlan
local ListTerminalLocation
local Comparer_1NullableInt32
local ILocationDictionary_1ILocationSet
local DictILocationSetSolutionsAndGroupNumber
local KeyValuePairLocationListTerminalLocation
local IReadOnlyCollection_1SolutionsAndGroupNumber
local DictILocationDictionary_1ILocationSetListSolution
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  KnapcodeAddPipes = Knapcode.FactorioTools.OilField.AddPipes
  ListInt32 = System.List(System.Int32)
  ListPlanInfo = System.List(KnapcodeAddPipes.PlanInfo)
  ListSolution = System.List(KnapcodeAddPipes.Solution)
  NullableInt32 = System.Nullable(System.Int32)
  ListOilFieldPlan = System.List(KnapcodeOilField.OilFieldPlan)
  ListTerminalLocation = System.List(KnapcodeOilField.TerminalLocation)
  Comparer_1NullableInt32 = System.Comparer_1(NullableInt32)
  ILocationDictionary_1ILocationSet = KnapcodeOilField.ILocationDictionary_1(KnapcodeOilField.ILocationSet)
  DictILocationSetSolutionsAndGroupNumber = System.Dictionary(KnapcodeOilField.ILocationSet, KnapcodeAddPipes.SolutionsAndGroupNumber)
  KeyValuePairLocationListTerminalLocation = System.KeyValuePair(KnapcodeOilField.Location, ListTerminalLocation)
  IReadOnlyCollection_1SolutionsAndGroupNumber = System.IReadOnlyCollection_1(KnapcodeAddPipes.SolutionsAndGroupNumber)
  DictILocationDictionary_1ILocationSetListSolution = System.Dictionary(ILocationDictionary_1ILocationSet, ListSolution)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("AddPipes", function (namespace)
    local Execute, GetBestSolution, GetAllPlans, GetSolutionGroups, OptimizeAndAddSolutions, GetSolution, EliminateStrandedTerminals, class
    namespace.class("SolutionInfo", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, SelectedPlans, AltnernatePlans, UnusedPlans, BestSolution, BestBeacons)
        this.SelectedPlans = SelectedPlans
        this.AltnernatePlans = AltnernatePlans
        this.UnusedPlans = UnusedPlans
        this.BestSolution = BestSolution
        this.BestBeacons = BestBeacons
      end
      __members__ = function ()
        return "SolutionInfo", "SelectedPlans", "AltnernatePlans", "UnusedPlans", "BestSolution", "BestBeacons"
      end
      return {
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipes.SolutionInfo)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("SolutionsAndGroupNumber", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, Solutions, GroupNumber)
        this.Solutions = Solutions
        this.GroupNumber = GroupNumber
      end
      __members__ = function ()
        return "SolutionsAndGroupNumber", "Solutions", "GroupNumber"
      end
      return {
        GroupNumber = 0,
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipes.SolutionsAndGroupNumber)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("Solution", function (namespace)
      return {
        PipeCountWithoutUnderground = 0
      }
    end)
    namespace.class("LocationSetComparer", function (namespace)
      local Instance, EqualsOf, GetHashCodeOf, class, static
      static = function (this)
        Instance = class()
        this.Instance = Instance
      end
      EqualsOf = function (this, x, y)
        if x == nil and y == nil then
          return true
        end

        if x == nil then
          return false
        end

        if y == nil then
          return false
        end

        if x:getCount() ~= y:getCount() then
          return false
        end

        return x:SetEquals(y)
      end
      GetHashCodeOf = function (this, obj)
        local sumX = 0
        local minX = 2147483647 --[[Int32.MaxValue]]
        local maxX = -2147483648 --[[Int32.MinValue]]
        local sumY = 0
        local minY = 2147483647 --[[Int32.MaxValue]]
        local maxY = -2147483648 --[[Int32.MinValue]]

        for _, l in System.each(obj:EnumerateItems()) do
          sumX = sumX + l.X

          if l.X < minX then
            minX = l.X
          end

          if l.X > maxX then
            maxX = l.X
          end

          sumY = sumY + l.Y

          if l.Y < minY then
            minY = l.Y
          end

          if l.Y > maxY then
            maxY = l.Y
          end
        end

        local hash = 17
        hash = hash * 23 + obj:getCount()
        hash = hash * 23 + sumX
        hash = hash * 23 + minX
        hash = hash * 23 + maxX
        hash = hash * 23 + sumY
        hash = hash * 23 + minY
        hash = hash * 23 + maxY

        return hash
      end
      class = {
        base = function (out)
          return {
            System.IEqualityComparer_1(out.Knapcode.FactorioTools.OilField.ILocationSet)
          }
        end,
        EqualsOf = EqualsOf,
        GetHashCodeOf = GetHashCodeOf,
        static = static
      }
      return class
    end)
    namespace.class("ConnectedCentersComparer", function (namespace)
      local Instance, EqualsOf, GetHashCodeOf, class, static
      static = function (this)
        Instance = class()
        this.Instance = Instance
      end
      EqualsOf = function (this, x, y)
        if x == nil and y == nil then
          return true
        end

        if x == nil then
          return false
        end

        if y == nil then
          return false
        end

        if x:getCount() ~= y:getCount() then
          return false
        end

        for _, default in System.each(x:EnumeratePairs()) do
          local key, xValue = default:Deconstruct()
          local extern, yValue = y:TryGetValue(key)
          if not extern then
            return false
          end

          if not xValue:SetEquals(yValue) then
            return false
          end
        end

        return true
      end
      GetHashCodeOf = function (this, obj)
        local sumX = 0
        local minX = 2147483647 --[[Int32.MaxValue]]
        local maxX = -2147483648 --[[Int32.MinValue]]
        local sumY = 0
        local minY = 2147483647 --[[Int32.MaxValue]]
        local maxY = -2147483648 --[[Int32.MinValue]]
        local locationSum = 0

        for _, default in System.each(obj:EnumeratePairs()) do
          local l, s = default:Deconstruct()
          sumX = sumX + l.X

          if l.X < minX then
            minX = l.X
          end

          if l.X > maxX then
            maxX = l.X
          end

          sumY = sumY + l.Y

          if l.Y < minY then
            minY = l.Y
          end

          if l.Y > maxY then
            maxY = l.Y
          end

          locationSum = s:getCount()
        end

        local hash = 17
        hash = hash * 23 + obj:getCount()
        hash = hash * 23 + sumX
        hash = hash * 23 + minX
        hash = hash * 23 + maxX
        hash = hash * 23 + sumY
        hash = hash * 23 + minY
        hash = hash * 23 + maxY
        hash = hash * 23 + locationSum

        return hash
      end
      class = {
        base = function (out)
          return {
            System.IEqualityComparer_1(out.Knapcode.FactorioTools.OilField.ILocationDictionary_1(out.Knapcode.FactorioTools.OilField.ILocationSet))
          }
        end,
        EqualsOf = EqualsOf,
        GetHashCodeOf = GetHashCodeOf,
        static = static
      }
      return class
    end)
    namespace.class("PlanInfo", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, GroupNumber, GroupSize, Plan, Pipes, Beacons)
        this.GroupNumber = GroupNumber
        this.GroupSize = GroupSize
        this.Plan = Plan
        this.Pipes = Pipes
        this.Beacons = Beacons
      end
      __members__ = function ()
        return "PlanInfo", "GroupNumber", "GroupSize", "Plan", "Pipes", "Beacons"
      end
      return {
        GroupNumber = 0,
        GroupSize = 0,
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipes.PlanInfo)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    Execute = function (context, eliminateStrandedTerminals)
      if eliminateStrandedTerminals then
        EliminateStrandedTerminals(context)
      end

      local selectedPlans
      local alternatePlans
      local unusedPlans
      local bestSolution
      local bestBeacons

      local result = GetBestSolution(context)
      if System.is(result.Exception, KnapcodeFactorioTools.NoPathBetweenTerminalsException) and not eliminateStrandedTerminals then
        EliminateStrandedTerminals(context)
        result = GetBestSolution(context)
        if result.Exception ~= nil then
          System.throw(result.Exception)
        end
      end

      selectedPlans, alternatePlans, unusedPlans, bestSolution, bestBeacons = result.Data:Deconstruct()

      context.CenterToTerminals = bestSolution.CenterToTerminals
      context.LocationToTerminals = bestSolution.LocationToTerminals

      KnapcodeOilField.AddPipeEntities.Execute(context, bestSolution.Pipes, bestSolution.UndergroundPipes)

      if bestBeacons ~= nil then
        -- Visualizer.Show(context.Grid, bestSolution.Beacons.Select(c => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(c.X, c.Y)), Array.Empty<DelaunatorSharp.IEdge>());
        KnapcodeOilField.Helpers.AddBeaconsToGrid(context.Grid, context.Options, bestBeacons.Beacons)
      end

      return System.ValueTuple(selectedPlans, alternatePlans, unusedPlans)
    end
    GetBestSolution = function (context)
      local result = GetAllPlans(context)
      if result.Exception ~= nil then
        return KnapcodeFactorioTools.Result.NewException(result.Exception, class.SolutionInfo)
      end

      local sortedPlans = result.Data
      sortedPlans:Sort(function (a, b)
        -- more effects = better
        local c = System.Int32.CompareTo(b.Plan.BeaconEffectCount, a.Plan.BeaconEffectCount)
        if c ~= 0 then
          return c
        end

        -- fewer beacons = better (less power)
        c = System.Int32.CompareTo(a.Plan.BeaconCount, b.Plan.BeaconCount)
        if c ~= 0 then
          return c
        end

        -- fewer pipes = better
        c = System.Int32.CompareTo(a.Plan.PipeCount, b.Plan.PipeCount)
        if c ~= 0 then
          return c
        end

        -- prefer solutions that more algorithms find
        c = System.Int32.CompareTo(b.GroupSize, a.GroupSize)
        if c ~= 0 then
          return c
        end

        -- the rest of the sorting is for arbitrary tie breaking
        c = System.Enum.CompareToObj(a.Plan.PipeStrategy, b.Plan.PipeStrategy)
        if c ~= 0 then
          return c
        end

        c = System.Boolean.CompareTo(a.Plan.OptimizePipes, b.Plan.OptimizePipes)
        if c ~= 0 then
          return c
        end

        c = Comparer_1NullableInt32.getDefault():Compare(a.Plan.BeaconStrategy, b.Plan.BeaconStrategy)
        if c ~= 0 then
          return c
        end

        return System.Int32.CompareTo(a.GroupNumber, b.GroupNumber)
      end)

      local bestPlanInfo = nil
      local noMoreAlternates = false
      local selectedPlans = ListOilFieldPlan()
      local alternatePlans = ListOilFieldPlan()
      local unusedPlans = ListOilFieldPlan()

      for _, planInfo in System.each(sortedPlans) do
        local continue
        repeat
          if noMoreAlternates then
            unusedPlans:Add(planInfo.Plan)
            continue = true
            break
          elseif bestPlanInfo == nil then
            bestPlanInfo = planInfo
            selectedPlans:Add(planInfo.Plan)
            continue = true
            break
          end

          local bestGroupNumber, _, bestPlan, _, _ = bestPlanInfo:Deconstruct()
          if planInfo.Plan:IsEquivalent(bestPlan) then
            if planInfo.GroupNumber == bestGroupNumber then
              selectedPlans:Add(planInfo.Plan)
            else
              alternatePlans:Add(planInfo.Plan)
            end
          else
            noMoreAlternates = true
            unusedPlans:Add(planInfo.Plan)
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      if bestPlanInfo == nil then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("At least one pipe strategy must be used."))
      end

      return KnapcodeFactorioTools.Result.NewData(class.SolutionInfo(selectedPlans, alternatePlans, unusedPlans, bestPlanInfo.Pipes, bestPlanInfo.Beacons), class.SolutionInfo)
    end
    GetAllPlans = function (context)
      local result = GetSolutionGroups(context)
      if result.Exception ~= nil then
        return KnapcodeFactorioTools.Result.NewException(result.Exception, ListPlanInfo)
      end

      local solutionGroups = result.Data
      local plans = ListPlanInfo()
      for _, default in System.each(solutionGroups) do
        local solutionGroup, groupNumber = default:Deconstruct()
        for _, solution in System.each(solutionGroup) do
          if solution.BeaconSolutions == nil then
            for _, strategy in System.each(solution.Strategies) do
              for _, optimized in System.each(solution.Optimized) do
                local plan = KnapcodeOilField.OilFieldPlan(strategy, optimized, nil, 0, 0, solution.Pipes:getCount(), solution.PipeCountWithoutUnderground)

                plans:Add(class.PlanInfo(groupNumber, #solutionGroup, plan, solution))
              end
            end
          else
            for _, beacons in System.each(solution.BeaconSolutions) do
              for _, strategy in System.each(solution.Strategies) do
                for _, optimized in System.each(solution.Optimized) do
                  local plan = KnapcodeOilField.OilFieldPlan(strategy, optimized, beacons.Strategy, beacons.Effects, #beacons.Beacons, solution.Pipes:getCount(), solution.PipeCountWithoutUnderground)

                  plans:Add(class.PlanInfo(groupNumber, #solutionGroup, plan, solution, beacons))
                end
              end
            end
          end
        end
      end

      return KnapcodeFactorioTools.Result.NewData(plans, ListPlanInfo)
    end
    GetSolutionGroups = function (context)
      local originalCenterToTerminals = context.CenterToTerminals
      local originalLocationToTerminals = context.LocationToTerminals

      local pipesToSolutions = DictILocationSetSolutionsAndGroupNumber(class.LocationSetComparer.Instance)
      local connectedCentersToSolutions = DictILocationDictionary_1ILocationSetListSolution(class.ConnectedCentersComparer.Instance)

      if context.CenterToTerminals:getCount() == 1 then
        local terminal = KnapcodeFactorioTools.CollectionExtensions.Single(context.CenterToTerminals:EnumeratePairs(), KeyValuePairLocationListTerminalLocation)[2]:get(0)
        KnapcodeOilField.Helpers.EliminateOtherTerminals(context, terminal)
        local pipes = context:GetSingleLocationSet(terminal.Terminal)
        local solutions = OptimizeAndAddSolutions(context, pipesToSolutions, 0, pipes)
        local solution = KnapcodeFactorioTools.CollectionExtensions.Single(solutions, class.Solution)
        solution.Strategies:Clear()
        solution.Strategies:AddRange(context.Options.PipeStrategies)
      else
        local completedStrategies = System.new(KnapcodeOilField.CustomCountedBitArray, 2, 5 --[[(int)PipeStrategy.ConnectedCentersFlute + 1]])
        -- max value
        for _, strategy in System.each(context.Options.PipeStrategies) do
          local continue
          repeat
            if completedStrategies:get(strategy) then
              continue = true
              break
            end

            context.CenterToTerminals = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(originalCenterToTerminals:EnumeratePairs(), context, function (x)
              return x[1]
            end, function (x)
              return KnapcodeFactorioTools.CollectionExtensions.ToList(x[2], KnapcodeOilField.TerminalLocation)
            end, KeyValuePairLocationListTerminalLocation, ListTerminalLocation)
            context.LocationToTerminals = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(originalLocationToTerminals:EnumeratePairs(), context, function (x)
              return x[1]
            end, function (x)
              return KnapcodeFactorioTools.CollectionExtensions.ToList(x[2], KnapcodeOilField.TerminalLocation)
            end, KeyValuePairLocationListTerminalLocation, ListTerminalLocation)

            repeat
              if strategy == 0 --[[PipeStrategy.FbeOriginal]] or strategy == 1 --[[PipeStrategy.Fbe]] then
                do
                  local result = KnapcodeOilField.AddPipesFbe.Execute(context, strategy)
                  if result.Exception ~= nil then
                    return KnapcodeFactorioTools.Result.NewException(result.Exception, IReadOnlyCollection_1SolutionsAndGroupNumber)
                  end

                  local pipes, finalStrategy = result.Data:Deconstruct()
                  completedStrategies:set(finalStrategy, true)

                  OptimizeAndAddSolutions(context, pipesToSolutions, finalStrategy, pipes)
                end
                break
              elseif strategy == 2 --[[PipeStrategy.ConnectedCentersDelaunay]] or strategy == 3 --[[PipeStrategy.ConnectedCentersDelaunayMst]] or strategy == 4 --[[PipeStrategy.ConnectedCentersFlute]] then
                do
                  local centerToConnectedCenters = KnapcodeOilField.AddPipesConnectedCenters.GetConnectedPumpjacks(context, strategy)
                  completedStrategies:set(strategy, true)

                  local default, solutions = connectedCentersToSolutions:TryGetValue(centerToConnectedCenters, nil)
                  if default then
                    for _, solution in System.each(solutions) do
                      solution.Strategies:Add(strategy)
                    end
                    continue = true
                    break
                  end

                  local result = KnapcodeOilField.AddPipesConnectedCenters.FindTrunksAndConnect(context, centerToConnectedCenters)
                  if result.Exception ~= nil then
                    return KnapcodeFactorioTools.Result.NewException(result.Exception, IReadOnlyCollection_1SolutionsAndGroupNumber)
                  end

                  local pipes = result.Data
                  solutions = OptimizeAndAddSolutions(context, pipesToSolutions, strategy, pipes, centerToConnectedCenters)
                  connectedCentersToSolutions:AddKeyValue(centerToConnectedCenters, solutions)
                end
                break
              else
                System.throw(System.NotImplementedException())
              end
            until 1
            continue = true
          until 1
          if not continue then
            break
          end
        end
      end

      return KnapcodeFactorioTools.Result.NewData(pipesToSolutions:getValues(), IReadOnlyCollection_1SolutionsAndGroupNumber)
    end
    OptimizeAndAddSolutions = function (context, pipesToSolutions, strategy, pipes, centerToConnectedCenters)
      local solutionsAndIndex
      local default
      default, solutionsAndIndex = pipesToSolutions:TryGetValue(pipes, nil)
      if default then
        for _, solution in System.each(solutionsAndIndex.Solutions) do
          solution.Strategies:Add(strategy)
        end

        return solutionsAndIndex.Solutions
      end

      -- Visualizer.Show(context.Grid, pipes.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());

      local originalCenterToTerminals = context.CenterToTerminals
      local originalLocationToTerminals = context.LocationToTerminals

      local optimizedPipes = pipes
      if context.Options.OptimizePipes then
        context.CenterToTerminals = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(originalCenterToTerminals:EnumeratePairs(), context, function (x)
          return x[1]
        end, function (x)
          return KnapcodeFactorioTools.CollectionExtensions.ToList(x[2], KnapcodeOilField.TerminalLocation)
        end, KeyValuePairLocationListTerminalLocation, ListTerminalLocation)
        context.LocationToTerminals = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(originalLocationToTerminals:EnumeratePairs(), context, function (x)
          return x[1]
        end, function (x)
          return KnapcodeFactorioTools.CollectionExtensions.ToList(x[2], KnapcodeOilField.TerminalLocation)
        end, KeyValuePairLocationListTerminalLocation, ListTerminalLocation)
        optimizedPipes = context:GetLocationSet(pipes)
        KnapcodeOilField.RotateOptimize.Execute(context, optimizedPipes)

        -- Visualizer.Show(context.Grid, optimizedPipes.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
      end

      local solutions
      if pipes:SetEquals(optimizedPipes) then
        local extern
        if context.Options.UseUndergroundPipes then
          extern = context:GetLocationSet(pipes)
        else
          extern = pipes
        end
        optimizedPipes = extern
        local ref = ListSolution()
        ref:Add(GetSolution(context, strategy, false, centerToConnectedCenters, optimizedPipes))
        solutions = ref

        if context.Options.OptimizePipes then
          solutions:get(0).Optimized:Add(true)
        end
      else
        local solutionA = GetSolution(context, strategy, true, centerToConnectedCenters, optimizedPipes)

        context.CenterToTerminals = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(originalCenterToTerminals:EnumeratePairs(), context, function (x)
          return x[1]
        end, function (x)
          return KnapcodeFactorioTools.CollectionExtensions.ToList(x[2], KnapcodeOilField.TerminalLocation)
        end, KeyValuePairLocationListTerminalLocation, ListTerminalLocation)
        context.LocationToTerminals = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(originalLocationToTerminals:EnumeratePairs(), context, function (x)
          return x[1]
        end, function (x)
          return KnapcodeFactorioTools.CollectionExtensions.ToList(x[2], KnapcodeOilField.TerminalLocation)
        end, KeyValuePairLocationListTerminalLocation, ListTerminalLocation)
        local extern
        if context.Options.UseUndergroundPipes then
          extern = context:GetLocationSet(pipes)
        else
          extern = pipes
        end
        local pipesB = extern
        local solutionB = GetSolution(context, strategy, false, centerToConnectedCenters, pipesB)

        KnapcodeOilField.Validate.PipesDoNotMatch(context, solutionA.Pipes, solutionB.Pipes)

        local ref = ListSolution()
        ref:Add(solutionA)
        ref:Add(solutionB)
        solutions = ref
      end

      pipesToSolutions:AddKeyValue(pipes, class.SolutionsAndGroupNumber(solutions, pipesToSolutions:getCount() + 1))

      return solutions
    end
    GetSolution = function (context, strategy, optimized, centerToConnectedCenters, optimizedPipes)
      KnapcodeOilField.Validate.PipesAreConnected(context, optimizedPipes)

      local pipeCountBeforeUnderground = optimizedPipes:getCount()

      local undergroundPipes = nil
      if context.Options.UseUndergroundPipes then
        undergroundPipes = KnapcodeOilField.PlanUndergroundPipes.Execute(context, optimizedPipes)
      end

      local beaconSolutions = nil
      if context.Options.AddBeacons then
        beaconSolutions = KnapcodeOilField.PlanBeacons.Execute(context, optimizedPipes)
      end

      KnapcodeOilField.Validate.NoOverlappingEntities(context, optimizedPipes, undergroundPipes, beaconSolutions)

      -- Visualizer.Show(context.Grid, optimizedPipes.Select(p => (DelaunatorSharp.IPoint)new DelaunatorSharp.Point(p.X, p.Y)), Array.Empty<DelaunatorSharp.IEdge>());

      local default = class.Solution()
      local extern = ListInt32()
      extern:Add(strategy)
      default.Strategies = extern
      local extern = ListBoolean()
      extern:Add(optimized)
      default.Optimized = extern
      default.CenterToConnectedCenters = centerToConnectedCenters
      default.CenterToTerminals = context.CenterToTerminals
      default.LocationToTerminals = context.LocationToTerminals
      default.PipeCountWithoutUnderground = pipeCountBeforeUnderground
      default.Pipes = optimizedPipes
      default.UndergroundPipes = undergroundPipes
      default.BeaconSolutions = beaconSolutions
      return default
    end
    EliminateStrandedTerminals = function (context)
      local locationsToExplore = KnapcodeFactorioTools.CollectionExtensions.ToReadOnlySet1(context.LocationToTerminals:getKeys(), context, true)

      while locationsToExplore:getCount() > 0 do
        local goals = context:GetLocationSet(locationsToExplore)
        local start = KnapcodeFactorioTools.CollectionExtensions.First(goals:EnumerateItems(), KnapcodeOilField.Location)
        goals:Remove(start)

        local result = KnapcodeOilField.Dijkstras.GetShortestPaths(context, context.Grid, start, goals, false, true)

        local reachedTerminals = result.ReachedGoals
        reachedTerminals:Add(start)

        local unreachedTerminals = context:GetLocationSet(goals)
        unreachedTerminals:ExceptWith(result.ReachedGoals)

        local reachedPumpjacks = context:GetLocationSet1()
        for _, location in System.each(result.ReachedGoals:EnumerateItems()) do
          local terminals = context.LocationToTerminals:get(location)
          for i = 0, #terminals - 1 do
            reachedPumpjacks:Add(terminals:get(i).Center)
          end
        end

        local terminalsToEliminate
        if reachedPumpjacks:getCount() == context.CenterToTerminals:getCount() then
          terminalsToEliminate = unreachedTerminals
          locationsToExplore:Clear()
        else
          terminalsToEliminate = reachedTerminals
          locationsToExplore = unreachedTerminals
        end

        local strandedTerminal = KnapcodeOilField.Location.getInvalid()
        local foundStranded = false
        for _, location in System.each(terminalsToEliminate:EnumerateItems()) do
          for _, terminal in System.each(context.LocationToTerminals:get(location)) do
            local terminals = context.CenterToTerminals:get(terminal.Center)
            terminals:Remove(terminal)

            if #terminals == 0 then
              strandedTerminal = terminal.Terminal
              foundStranded = true
            end
          end

          context.LocationToTerminals:Remove(location)
        end

        if foundStranded then
          --[[
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, new ILocationSet(), undergroundPipes: null, allowMultipleTerminals: true);
                Visualizer.Show(clone, new[] { strandedTerminal.Value, locationsToExplore.First() }.Select(x => (IPoint)new Point(x.X, x.Y)), Array.Empty<IEdge>());
                ]]

          System.throw(KnapcodeFactorioTools.NoPathBetweenTerminalsException(strandedTerminal, KnapcodeFactorioTools.CollectionExtensions.First(locationsToExplore:EnumerateItems(), KnapcodeOilField.Location)))
        end
      end
    end
    class = {
      Execute = Execute
    }
    return class
  end)
end)
