-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeOilField
local ListLocation
local SpanLocation
local ArrayLocation
local PriorityQueueLocationDouble
System.import(function (out)
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  ListLocation = System.List(KnapcodeOilField.Location)
  SpanLocation = System.Span(KnapcodeOilField.Location)
  ArrayLocation = System.Array(KnapcodeOilField.Location)
  PriorityQueueLocationDouble = System.PriorityQueue(KnapcodeOilField.Location, System.Double)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  -- <summary>
  -- Source: https://www.redblobgames.com/pathfinding/a-star/implementation.html
  -- </summary>
  namespace.class("AStar", function (namespace)
    local GetShortestPath, IsTurn, Heuristic
    GetShortestPath = function (context, grid, start, goals, preferNoTurns, xWeight, yWeight, outputList)
      if goals:Contains(start) then
        if outputList ~= nil then
          outputList:Add(start)
        else
          local default = ListLocation()
          default:Add(start)
          outputList = default
        end

        return KnapcodeOilField.AStarResult(true, start, outputList)
      end

      local goalsArray = ArrayLocation(goals:getCount())

      goals:CopyTo(SpanLocation.ctorArray(goalsArray))


      local cameFrom = context:GetLocationDictionary(KnapcodeOilField.Location)
      local costSoFar = context:GetLocationDictionary(System.Double)
      local frontier = PriorityQueueLocationDouble()


      local default, extern = System.try(function ()
        frontier:Enqueue(start, 0)

        cameFrom:set(start, start)
        costSoFar:set(start, 0)

        local reachedGoal = KnapcodeOilField.Location.getInvalid()
        local success = false
        local neighbors = SpanLocation.ctorArray(ArrayLocation(4))


        while #frontier > 0 do
          local continue
          repeat
            local current = frontier:Dequeue()

            if goals:Contains(current) then
              reachedGoal = current
              success = true
              break
            end

            local previous = cameFrom:get(current)
            local currentCost = costSoFar:get(current)

            grid:GetNeighbors(neighbors, current)
            for i = 0, neighbors:getLength() - 1 do
              local continue
              repeat
                local next = neighbors:get(i)
                if not next.IsValid then
                  continue = true
                  break
                end

                local newCost = currentCost + 1 --[[SquareGrid.NeighborCost]]

                local default, thisCostSoFar = costSoFar:TryGetValue(next)
                if not default or newCost < thisCostSoFar then
                  costSoFar:set(next, newCost)
                  local priority

                  do
                    priority = newCost + Heuristic(next, goalsArray, goals:getCount(), xWeight, yWeight)
                  end

                  -- Prefer paths without turns.
                  if preferNoTurns and KnapcodeOilField.Location.op_Inequality(previous, current) and IsTurn(previous, current, next) then
                    priority = priority + 0.0001
                  end

                  frontier:Enqueue(next, priority)
                  cameFrom:set(next, current)
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

        if not success then
          outputList = nil
        elseif outputList ~= nil then
          KnapcodeOilField.Helpers.AddPath(cameFrom, reachedGoal, outputList)
        else
          outputList = KnapcodeOilField.Helpers.GetPath(cameFrom, start, reachedGoal)
        end

        return true, KnapcodeOilField.AStarResult(success, reachedGoal, outputList)
      end, nil, function ()
      end)
      if default then
        return extern
      end
    end
    IsTurn = function (a, b, c)
      local directionA = (a.X == b.X) and 0 or 1
      local directionB = (b.X == c.X) and 0 or 1
      return directionA ~= directionB
    end
    Heuristic = function (current, goals, goalsCount, xWeight, yWeight)
      local min = 2147483647 --[[Int32.MaxValue]]
      for i = 0, goalsCount - 1 do
        local val = xWeight * math.Abs(goals:get(i).X - current.X) + yWeight * math.Abs(goals:get(i).Y - current.Y)
        if val < min then
          min = val
        end
      end

      return min
    end
    return {
      GetShortestPath = GetShortestPath
    }
  end)
end)
