-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioTools
local KnapcodeOilField
local ListLocation
local DictInt32Int32
local ListListLocation
local DictInt32Location
local KeyValuePairInt32Location
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  ListLocation = System.List(KnapcodeOilField.Location)
  DictInt32Int32 = System.Dictionary(System.Int32, System.Int32)
  ListListLocation = System.List(ListLocation)
  DictInt32Location = System.Dictionary(System.Int32, KnapcodeOilField.Location)
  KeyValuePairInt32Location = System.KeyValuePair(System.Int32, KnapcodeOilField.Location)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("DijkstrasResult", function (namespace)
    local GetStraightPaths, GetDirection, __ctor__
    __ctor__ = function (this, locationToPrevious, reachedGoals)
      this.LocationToPrevious = locationToPrevious
      this.ReachedGoals = reachedGoals
    end
    GetStraightPaths = function (this, goal)
      local paths = ListListLocation()

      local default, previousLocations = this.LocationToPrevious:TryGetValue(goal)
      if default then
        if previousLocations:getCount() == 0 then
          -- This is a special case when the goal matches the starting point.
          local extern = ListLocation()
          extern:Add(goal)
          paths:Add(extern)
        end

        for _, beforeGoal in System.each(previousLocations:EnumerateItems()) do
          -- This is the final direction used in the path. We'll start with preferring this direction as we reconstruct
          -- the full path.
          local preferredDirection = GetDirection(beforeGoal, goal)

          -- Track the number of times each direction was used so when we have to switch directions, we can prefer a
          -- direction that's been used the most.
          local extern = DictInt32Int32()
          extern:AddKeyValue(0 --[[Direction.Up]], 0)
          extern:AddKeyValue(2 --[[Direction.Right]], 0)
          extern:AddKeyValue(4 --[[Direction.Down]], 0)
          extern:AddKeyValue(6 --[[Direction.Left]], 0)
          local directionHits = extern

          local current = goal
          local path = ListLocation()
          while true do
            path:Add(current)

            local allPrevious = this.LocationToPrevious:get(current)
            if allPrevious:getCount() == 0 then
              break
            end

            local directionToPrevious = DictInt32Location()
            for _, candidate in System.each(allPrevious:EnumerateItems()) do
              directionToPrevious:set(GetDirection(candidate, current), candidate)
            end

            local extern, previous = directionToPrevious:TryGetValue(preferredDirection, nil)
            if extern then
              current = previous
              local ref = directionHits
              ref:set(preferredDirection, ref:get(preferredDirection) + 1)
            else
              local nextBest = KnapcodeFactorioTools.CollectionExtensions.MaxBy(directionToPrevious, function (x)
                return directionHits:get(x[1])
              end, KeyValuePairInt32Location, System.Int32)
              local ref = directionHits
              ref:set(nextBest[1], ref:get(nextBest[1]) + 1)
              current = nextBest[2]
            end
          end

          paths:Add(path)
        end
      end

      return paths
    end
    GetDirection = function (from, to)
      local deltaX = to.X - from.X
      local deltaY = to.Y - from.Y

      if deltaX > 0 then
        return 2 --[[Direction.Right]]
      elseif deltaX < 0 then
        return 6 --[[Direction.Left]]
      elseif deltaY > 0 then
        return 4 --[[Direction.Down]]
      else
        return 0 --[[Direction.Up]]
      end
    end
    return {
      GetStraightPaths = GetStraightPaths,
      __ctor__ = __ctor__
    }
  end)
end)
