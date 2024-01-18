-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeOilField
local SpanLocation
local ArrayLocation
local PriorityQueueLocationDouble
System.import(function (out)
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  SpanLocation = System.Span(KnapcodeOilField.Location)
  ArrayLocation = System.Array(KnapcodeOilField.Location)
  PriorityQueueLocationDouble = System.PriorityQueue(KnapcodeOilField.Location, System.Double)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("Dijkstras", function (namespace)
    local GetShortestPaths
    GetShortestPaths = function (context, grid, start, goals, stopOnFirstGoal, allowGoalEnumerate)
      local cameFrom = context:GetLocationDictionary(KnapcodeOilField.ILocationSet)
      cameFrom:set(start, context:GetLocationSet1())
      local remainingGoals = context:GetLocationSet(goals)
      local reachedGoals = context:GetLocationSet2(allowGoalEnumerate)


      local priorityQueue = PriorityQueueLocationDouble()
      local costSoFar = context:GetLocationDictionary(System.Double)
      local inQueue = context:GetLocationSet1()
      costSoFar:set(start, 0)

      priorityQueue:Enqueue(start, 0)
      inQueue:Add(start)

      local neighbors = SpanLocation.ctorArray(ArrayLocation(4))


      while #priorityQueue > 0 do
        local continue
        repeat
          local current = priorityQueue:Dequeue()
          inQueue:Remove(current)
          local currentCost = costSoFar:get(current)

          if remainingGoals:Remove(current) then
            reachedGoals:Add(current)

            if stopOnFirstGoal or remainingGoals:getCount() == 0 then
              break
            end
          end

          grid:GetNeighbors(neighbors, current)
          for i = 0, neighbors:getLength() - 1 do
            local continue
            repeat
              local neighbor = neighbors:get(i)
              if not neighbor.IsValid then
                continue = true
                break
              end

              local alternateCost = currentCost + 1 --[[SquareGrid.NeighborCost]]
              local previousExists
              local default, neighborCost = costSoFar:TryGetValue(neighbor)
              previousExists = default
              if not previousExists or alternateCost <= neighborCost then
                if not previousExists or alternateCost < neighborCost then
                  costSoFar:set(neighbor, alternateCost)
                  cameFrom:set(neighbor, context:GetLocationSet3(current))
                else
                  cameFrom:get(neighbor):Add(current)
                end

                if not inQueue:Contains(neighbor) then
                  priorityQueue:Enqueue(neighbor, alternateCost)
                  inQueue:Add(neighbor)
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

      return KnapcodeOilField.DijkstrasResult(cameFrom, reachedGoals)
    end
    return {
      GetShortestPaths = GetShortestPaths
    }
  end)
end)
