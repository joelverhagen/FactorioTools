-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioTools
local KnapcodeOilField
local ListLocation
local SpanLocation
local ArrayLocation
local QueueLocation
local ListTerminalLocation
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  ListLocation = System.List(KnapcodeOilField.Location)
  SpanLocation = System.Span(KnapcodeOilField.Location)
  ArrayLocation = System.Array(KnapcodeOilField.Location)
  QueueLocation = System.Queue(KnapcodeOilField.Location)
  ListTerminalLocation = System.List(KnapcodeOilField.TerminalLocation)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("RotateOptimize", function (namespace)
    local Execute, UseBestTerminal, UseShortestPath, ExplorePipes, ExplorePaths, class
    namespace.class("ChildContext", function (namespace)
      local getGrid, getLocationToTerminals, getCenterToTerminals, UpdateIntersectionsAndGoals, __ctor__
      __ctor__ = function (this, parentContext, pipes)
        this.ParentContext = parentContext
        this.Pipes = pipes
        this.Intersections = parentContext:GetLocationSet6(pipes:getCount(), true)
        this.Goals = parentContext:GetLocationSet5(pipes:getCount())
        this.ExistingPipeGrid = KnapcodeOilField.ExistingPipeGrid(parentContext.Grid, pipes)

        UpdateIntersectionsAndGoals(this)
      end
      getGrid = function (this)
        return this.ParentContext.Grid
      end
      getLocationToTerminals = function (this)
        return this.ParentContext.LocationToTerminals
      end
      getCenterToTerminals = function (this)
        return this.ParentContext.CenterToTerminals
      end
      UpdateIntersectionsAndGoals = function (this)
        this.Intersections:Clear()

        this.Goals:Clear()
        this.Goals:UnionWith(getLocationToTerminals(this):getKeys())

        for _, pipe in System.each(this.Pipes:EnumerateItems()) do
          local neighbors = 0

          if this.Pipes:Contains(pipe:Translate(1, 0)) then
            neighbors = neighbors + 1
          end

          if this.Pipes:Contains(pipe:Translate(0, - 1)) then
            neighbors = neighbors + 1
          end

          if this.Pipes:Contains(pipe:Translate(- 1, 0)) then
            neighbors = neighbors + 1
          end

          if this.Pipes:Contains(pipe:Translate(0, 1)) then
            neighbors = neighbors + 1
          end

          if neighbors > 2 or getLocationToTerminals(this):ContainsKey(pipe) and neighbors > 1 then
            this.Intersections:Add(pipe)
            this.Goals:Add(pipe)
          end
        end
      end
      return {
        getGrid = getGrid,
        getLocationToTerminals = getLocationToTerminals,
        getCenterToTerminals = getCenterToTerminals,
        UpdateIntersectionsAndGoals = UpdateIntersectionsAndGoals,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("ExploredPaths", function (namespace)
      local AddPath, __ctor__
      __ctor__ = function (this, start, cameFrom, reachedGoals)
        this.Start = start
        this.CameFrom = cameFrom
        this.ReachedGoals = reachedGoals
      end
      AddPath = function (this, goal, outputList)
        KnapcodeOilField.Helpers.AddPath(this.CameFrom, goal, outputList)
      end
      return {
        AddPath = AddPath,
        __ctor__ = __ctor__
      }
    end)
    Execute = function (parentContext, pipes)
      if parentContext.LocationToTerminals:getCount() < 2 then
        return
      end

      local context = class.ChildContext(parentContext, pipes)

      -- VisualizeIntersections(context);

      local modified = true
      local previousPipeCount = 2147483647 --[[Int32.MaxValue]]

      -- Some oil fields have multiple optimal configurations with the same pipe count. Allow up to 5 of these to be
      -- attempted before settling on one.
      local allowedSamePipeCounts = 5

      while modified and (context.Pipes:getCount() < previousPipeCount or allowedSamePipeCounts > 0) do
        local continue
        repeat
          local changedTerminal = false
          for _, terminals in System.each(context:getCenterToTerminals():getValues()) do
            local continue
            repeat
              local currentTerminal = KnapcodeFactorioTools.CollectionExtensions.Single(terminals, KnapcodeOilField.TerminalLocation)

              if #context:getLocationToTerminals():get(currentTerminal.Terminal) > 1 or context.Intersections:Contains(currentTerminal.Terminal) then
                continue = true
                break
              end

              if UseBestTerminal(context, currentTerminal) then
                -- VisualizeIntersections(context);
                changedTerminal = true
              end
              continue = true
            until 1
            if not continue then
              break
            end
          end

          -- VisualizeIntersections(context);

          local shortenedPath = false
          for _, intersection in System.each(KnapcodeFactorioTools.CollectionExtensions.ToList(context.Intersections:EnumerateItems(), KnapcodeOilField.Location)) do
            local continue
            repeat
              if not context.Intersections:Contains(intersection) then
                continue = true
                break
              end

              context.Goals:Remove(intersection)
              local exploredPaths = ExplorePaths(context, intersection)
              context.Goals:Add(intersection)

              for _, goal in System.each(exploredPaths.ReachedGoals) do
                if UseShortestPath(context, exploredPaths, intersection, goal) then
                  -- VisualizeIntersections(context);
                  shortenedPath = true
                end
              end
              continue = true
            until 1
            if not continue then
              break
            end
          end

          modified = changedTerminal or shortenedPath
          if previousPipeCount == context.Pipes:getCount() then
            allowedSamePipeCounts = allowedSamePipeCounts - 1
          else
            previousPipeCount = context.Pipes:getCount()
          end

          -- VisualizeIntersections(context);
          continue = true
        until 1
        if not continue then
          break
        end
      end
    end
    UseBestTerminal = function (context, originalTerminal)
      context.Goals:Remove(originalTerminal.Terminal)
      local exploredPaths = ExplorePaths(context, originalTerminal.Terminal)

      --[[
        if (exploredPaths.ReachedGoals.Count == 0)
        {
            var clone = new PipeGrid(context.ExistingPipeGrid);
            AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, context.Pipes);
            Visualizer.Show(clone, context.Goals.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
        }
        ]]

      local originalGoal = KnapcodeFactorioTools.CollectionExtensions.Single(exploredPaths.ReachedGoals, KnapcodeOilField.Location)


      local originalPath = ListLocation()
      exploredPaths:AddPath(originalGoal, originalPath)

      for i = 1, #originalPath - 1 do
        context.Pipes:Remove(originalPath:get(i))
      end

      local minTerminal = originalTerminal
      local minPath = originalPath
      local minPathTurns = KnapcodeOilField.Helpers.CountTurns(minPath)
      local changedPath = false

      for i = 0, KnapcodeOilField.Helpers.TerminalOffsets:getCount() - 1 do
        local continue
        repeat
          local direction, translation = KnapcodeOilField.Helpers.TerminalOffsets:get(i):Deconstruct()

          local terminalCandidate = originalTerminal.Center:Translate1(translation)
          if not context:getGrid():IsEmpty(terminalCandidate) and not context:getGrid():IsEntityType(terminalCandidate, KnapcodeOilField.Pipe) then
            continue = true
            break
          end

          local newPath = ListLocation()

          local result = KnapcodeOilField.AStar.GetShortestPath(context.ParentContext, context:getGrid(), terminalCandidate, context.Pipes, true, 1, 1, newPath)
          if result.Success then
            local terminal = KnapcodeOilField.TerminalLocation(originalTerminal.Center, terminalCandidate, direction)
            local pathTurns = KnapcodeOilField.Helpers.CountTurns(newPath)

            if #newPath < #minPath or (#newPath == #minPath and pathTurns < minPathTurns) then
              minPath:Clear()

              minTerminal = terminal
              minPath = newPath
              minPathTurns = pathTurns
              changedPath = true
            else
              newPath:Clear()
            end
          else
            newPath:Clear()
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      context.Pipes:UnionWith(minPath)

      if changedPath then
        if minTerminal ~= originalTerminal then
          context:getCenterToTerminals():get(originalTerminal.Center):Add(minTerminal)

          local default, locationTerminals = context:getLocationToTerminals():TryGetValue(minTerminal.Terminal)
          if not default then
            local extern = ListTerminalLocation()
            extern:Add(minTerminal)
            locationTerminals = extern
            context:getLocationToTerminals():Add(minTerminal.Terminal, locationTerminals)
          else
            locationTerminals:Add(minTerminal)
          end

          KnapcodeOilField.Helpers.EliminateOtherTerminals(context.ParentContext, minTerminal)
        end

        -- Console.WriteLine($"New best terminal: {minTerminal} -> {minPath.Last()}");

        context:UpdateIntersectionsAndGoals()

        --[[
                var clone = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, context.Pipes);
                Visualizer.Show(clone, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
                ]]

        return true
      else
        context.Goals:Add(originalTerminal.Terminal)
        return false
      end
    end
    UseShortestPath = function (context, exploredPaths, start, originalGoal)
      local originalPath = ListLocation()
      local connectionPoints = context.ParentContext:GetLocationSet6(context.Pipes:getCount(), true)
      exploredPaths:AddPath(originalGoal, originalPath)

      for i = 1, #originalPath - 1 do
        -- Does the path contain an intersection as an intermediate point? This can happen if a previous call
        -- of this method with the same exploration changed the intersections.
        if i < #originalPath - 1 and context.Intersections:Contains(originalPath:get(i)) then
          -- Add the path back.
          for j = i - 1, 1, -1 do
            context.Pipes:Add(originalPath:get(j))
          end

          return false
        end

        context.Pipes:Remove(originalPath:get(i))
      end

      --[[
            var clone = new PipeGrid(context.Grid);
            AddPipeEntities.Execute(clone, new(), context.CenterToTerminals, context.Pipes);
            Visualizer.Show(clone, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
            ]]

      ExplorePipes(context, originalGoal, connectionPoints)


      local result = KnapcodeOilField.AStar.GetShortestPath(context.ParentContext, context:getGrid(), start, connectionPoints, true, 1, 1)
      if #result:getPath() > #originalPath or (#result:getPath() == #originalPath and KnapcodeOilField.Helpers.CountTurns(result:getPath()) >= KnapcodeOilField.Helpers.CountTurns(originalPath)) then
        context.Pipes:UnionWith(originalPath)

        return false
      end

      context.Pipes:UnionWith(result:getPath())
      context:UpdateIntersectionsAndGoals()

      --[[
                var clone2 = new PipeGrid(context.Grid);
                AddPipeEntities.Execute(clone2, new(), context.CenterToTerminals, context.Pipes);
                Visualizer.Show(clone2, originalPath.Select(l => (IPoint)new Point(l.X, l.Y)), Array.Empty<IEdge>());
                ]]

      -- Console.WriteLine($"Shortened path: {result.Path[0]} -> {result.Path.Last()}");

      return true
    end
    ExplorePipes = function (context, start, pipes)
      local toExplore = QueueLocation()
      toExplore:Enqueue(start)
      pipes:Add(start)

      local neighbors = SpanLocation.ctorArray(ArrayLocation(4))


      while #toExplore > 0 do
        local current = toExplore:Dequeue()

        context.ExistingPipeGrid:GetNeighbors(neighbors, current)
        for i = 0, neighbors:getLength() - 1 do
          if neighbors:get(i).IsValid and pipes:Add(neighbors:get(i)) then
            toExplore:Enqueue(neighbors:get(i))
          end
        end
      end
    end
    ExplorePaths = function (context, start)
      local toExplore = QueueLocation()
      toExplore:Enqueue(start)
      local cameFrom = context.ParentContext:GetLocationDictionary(KnapcodeOilField.Location)
      cameFrom:set(start, start)

      local neighbors = SpanLocation.ctorArray(ArrayLocation(4))


      local reachedGoals = ListLocation()

      while #toExplore > 0 do
        local continue
        repeat
          local current = toExplore:Dequeue()

          if KnapcodeOilField.Location.op_Inequality(current, start) and context.Goals:Contains(current) then
            reachedGoals:Add(current)
            continue = true
            break
          end

          context.ExistingPipeGrid:GetNeighbors(neighbors, current)
          for i = 0, neighbors:getLength() - 1 do
            local continue
            repeat
              if not neighbors:get(i).IsValid or cameFrom:ContainsKey(neighbors:get(i)) then
                continue = true
                break
              end

              cameFrom:Add(neighbors:get(i), current)
              toExplore:Enqueue(neighbors:get(i))
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

      return class.ExploredPaths(start, cameFrom, reachedGoals)
    end
    class = {
      Execute = Execute
    }
    return class
  end)
end)
