-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeFactorioTools
local KnapcodeOilField
local KnapcodeAddPipesFbe
local ListGroup
local ListTuple
local ListLocation
local SpanLocation
local ArrayLocation
local ListListLocation
local ListPathAndTurns
local ListTerminalPair
local ListTerminalLocation
local ListPumpjackConnection
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  KnapcodeAddPipesFbe = Knapcode.FactorioTools.OilField.AddPipesFbe
  ListGroup = System.List(KnapcodeAddPipesFbe.Group)
  ListTuple = System.List(System.Tuple)
  ListLocation = System.List(KnapcodeOilField.Location)
  SpanLocation = System.Span(KnapcodeOilField.Location)
  ArrayLocation = System.Array(KnapcodeOilField.Location)
  ListListLocation = System.List(ListLocation)
  ListPathAndTurns = System.List(KnapcodeAddPipesFbe.PathAndTurns)
  ListTerminalPair = System.List(KnapcodeAddPipesFbe.TerminalPair)
  ListTerminalLocation = System.List(KnapcodeOilField.TerminalLocation)
  ListPumpjackConnection = System.List(KnapcodeAddPipesFbe.PumpjackConnection)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  -- <summary>
  -- This "FBE" implementation is copied from Teoxoy's Factorio Blueprint Editor (FBE).
  -- Source:
  -- - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/pipe.ts
  -- - https://github.com/teoxoy/factorio-blueprint-editor/blob/21ab873d8316a41b9a05c719697d461d3ede095d/packages/editor/src/core/generators/util.ts
  -- 
  -- Teoxoy came up with the idea to use Delaunay triangulation for this problem. Awesome!
  -- </summary>
  namespace.class("AddPipesFbe", function (namespace)
    local Execute, DelaunayTriangulation, GetNextLine, LineContainsAnAddedPumpjack, GetPathBetweenGroups, ConnectTwoGroups, class
    namespace.class("FbeResult", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, Pipes, FinalStrategy)
        this.Pipes = Pipes
        this.FinalStrategy = FinalStrategy
      end
      __members__ = function ()
        return "FbeResult", "Pipes", "FinalStrategy"
      end
      return {
        FinalStrategy = 0,
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipesFbe.FbeResult)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("FbeResultInfo", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, Terminals, Pipes, FinalStrategy)
        this.Terminals = Terminals
        this.Pipes = Pipes
        this.FinalStrategy = FinalStrategy
      end
      __members__ = function ()
        return "FbeResultInfo", "Terminals", "Pipes", "FinalStrategy"
      end
      return {
        FinalStrategy = 0,
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipesFbe.FbeResultInfo)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("TwoConnectedGroups", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, Lines, MinDistance, FirstGroup)
        this.Lines = Lines
        this.MinDistance = MinDistance
        this.FirstGroup = FirstGroup
      end
      __members__ = function ()
        return "TwoConnectedGroups", "Lines", "MinDistance", "FirstGroup"
      end
      return {
        MinDistance = 0,
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipesFbe.TwoConnectedGroups)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("PathAndTurns", function (namespace)
      local __members__, __ctor__
      __ctor__ = function (this, Endpoints, Path, Turns, OriginalIndex)
        this.Endpoints = Endpoints
        this.Path = Path
        this.Turns = Turns
        this.OriginalIndex = OriginalIndex
      end
      __members__ = function ()
        return "PathAndTurns", "Endpoints", "Path", "Turns", "OriginalIndex"
      end
      return {
        Turns = 0,
        OriginalIndex = 0,
        base = function (out)
          return {
            System.RecordType,
            System.IEquatable_1(out.Knapcode.FactorioTools.OilField.AddPipesFbe.PathAndTurns)
          }
        end,
        __members__ = __members__,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("Group", function (namespace)
      local getEntities, HasTerminal, Add, AddRange, UpdateLocation, internal, __ctor1__, __ctor2__, 
      __ctor3__
      internal = function (this)
        this.Location = KnapcodeOilField.Location.getInvalid()
      end
      __ctor1__ = function (this, context, terminal, paths)
        __ctor3__(this, context, paths)
        Add(this, terminal)
        UpdateLocation(this)
      end
      __ctor2__ = function (this, context, pair, paths)
        __ctor3__(this, context, paths)
        Add(this, pair.TerminalA)
        Add(this, pair.TerminalB)
        UpdateLocation(this)
      end
      __ctor3__ = function (this, context, paths)
        internal(this)
        this._terminals = context:GetLocationSet1()
        this._entities = ListTerminalLocation()
        this.Paths = paths
      end
      getEntities = function (this)
        return this._entities
      end
      HasTerminal = function (this, location)
        return this._terminals:Contains(location.Terminal)
      end
      Add = function (this, entity)
        this._entities:Add(entity)
        this._terminals:Add(entity.Terminal)
        this._sumX = this._sumX + entity.Center.X
        this._sumY = this._sumY + entity.Center.Y
        UpdateLocation(this)
      end
      AddRange = function (this, group)
        this._entities:AddRange(getEntities(group))
        this._terminals:UnionWith1(group._terminals)
        for i = 0, getEntities(group):getCount() - 1 do
          local entity = getEntities(group):get(i)
          this._sumX = this._sumX + entity.Center.X
          this._sumY = this._sumY + entity.Center.Y
        end
        UpdateLocation(this)
      end
      UpdateLocation = function (this)
        this.Location = KnapcodeOilField.Location(System.ToInt32(math.Round(this._sumX / #this._entities, 0)), System.ToInt32(math.Round(this._sumY / #this._entities, 0)))
      end
      return {
        _sumX = 0,
        _sumY = 0,
        getEntities = getEntities,
        HasTerminal = HasTerminal,
        Add = Add,
        AddRange = AddRange,
        __ctor__ = {
          __ctor1__,
          __ctor2__,
          __ctor3__
        }
      }
    end)
    namespace.class("PumpjackConnection", function (namespace)
      local GetAverageDistance, __ctor__
      __ctor__ = function (this, endpoints, connections, middle)
        this.Endpoints = endpoints
        this.Connections = connections
        this.EndpointDistance = endpoints.A:GetManhattanDistance(middle) + endpoints.B:GetManhattanDistance(middle)
      end
      GetAverageDistance = function (this)
        return (#this.Connections > 0) and KnapcodeFactorioTools.CollectionExtensions.Average(this.Connections, function (x)
          return #x.Line - 1
        end, KnapcodeAddPipesFbe.TerminalPair) or 0
      end
      return {
        EndpointDistance = 0,
        GetAverageDistance = GetAverageDistance,
        __ctor__ = __ctor__
      }
    end)
    namespace.class("TerminalPair", function (namespace)
      local ToString, __ctor__
      __ctor__ = function (this, terminalA, terminalB, line, middle)
        this.TerminalA = terminalA
        this.TerminalB = terminalB
        this.Line = line
        this.CenterDistance = terminalA.Terminal:GetManhattanDistance(middle)
      end
      ToString = function (this)
        return System.toString(this.TerminalA.Terminal) .. " -> " .. System.toString(this.TerminalB.Terminal) .. " (length " .. #this.Line .. ")"
      end
      return {
        CenterDistance = 0,
        ToString = ToString,
        __ctor__ = __ctor__
      }
    end)
    Execute = function (context, strategy)
      -- HACK: it appears FBE does not adjust the grid middle by the 2 cell buffer added to the side of the grid.
      -- We'll apply this hack for now to reproduce FBE results.
      local middle = context.Grid.Middle:Translate(- 2, - 2)

      local result = DelaunayTriangulation(context, middle, strategy)
      if result.Exception ~= nil then
        return KnapcodeFactorioTools.Result.NewException(result.Exception, class.FbeResult)
      end

      local terminals, pipes, finalStrategy = result.Data:Deconstruct()

      for _, terminal in System.each(terminals) do
        KnapcodeOilField.Helpers.EliminateOtherTerminals(context, terminal)
      end

      return KnapcodeFactorioTools.Result.NewData(class.FbeResult(pipes, finalStrategy), class.FbeResult)
    end
    DelaunayTriangulation = function (context, middle, strategy)
      -- GENERATE LINES
      local lines = ListPumpjackConnection()
      local allLines = KnapcodeOilField.Helpers.PointsToLines1(context.Centers, false)
      for i = 0, #allLines - 1 do
        local continue
        repeat
          local line = allLines:get(i)
          local connections = ListTerminalPair()

          for _, tA in System.each(context.CenterToTerminals:get(line.A)) do
            local continue
            repeat
              for _, tB in System.each(context.CenterToTerminals:get(line.B)) do
                local continue
                repeat
                  if tA.Terminal.X ~= tB.Terminal.X and tA.Terminal.Y ~= tB.Terminal.Y then
                    continue = true
                    break
                  end

                  local straightLine = KnapcodeOilField.Helpers.MakeStraightLineOnEmpty(context.Grid, tA.Terminal, tB.Terminal)
                  if straightLine == nil then
                    continue = true
                    break
                  end

                  connections:Add(class.TerminalPair(tA, tB, straightLine, middle))
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

          if #connections == 0 then
            continue = true
            break
          end

          lines:Add(class.PumpjackConnection(KnapcodeOilField.Endpoints(line.A, line.B), connections, middle))
          continue = true
        until 1
        if not continue then
          break
        end
      end

      -- GENERATE GROUPS
      local groups = ListGroup()
      local addedPumpjacks = ListTerminalLocation()
      local leftoverPumpjacks = KnapcodeFactorioTools.CollectionExtensions.ToSet(context.Centers, context, true)
      while #lines > 0 do
        local line = GetNextLine(lines, addedPumpjacks)

        local addedA = KnapcodeFactorioTools.CollectionExtensions.FirstOrDefault(addedPumpjacks, function (x)
          return KnapcodeOilField.Location.op_Equality(x.Center, line.Endpoints.A)
        end, KnapcodeOilField.TerminalLocation)
        local addedB = KnapcodeFactorioTools.CollectionExtensions.FirstOrDefault(addedPumpjacks, function (x)
          return KnapcodeOilField.Location.op_Equality(x.Center, line.Endpoints.B)
        end, KnapcodeOilField.TerminalLocation)

        line.Connections:Sort(function (a, b)
          local c = System.Int32.CompareTo(a.CenterDistance, b.CenterDistance)
          if c ~= 0 then
            return c
          end

          return System.Int32.CompareTo((#a.Line), #b.Line)
        end)

        for _, connection in System.each(line.Connections) do
          if addedA == nil and addedB == nil then
            local default = ListListLocation()
            default:Add(connection.Line)
            local group = System.new(class.Group, 2, context, connection, default)
            groups:Add(group)
            addedPumpjacks:Add(connection.TerminalA)
            addedPumpjacks:Add(connection.TerminalB)
            leftoverPumpjacks:Remove(connection.TerminalA.Center)
            leftoverPumpjacks:Remove(connection.TerminalB.Center)
            break
          end

          if addedA == nil and addedB ~= nil and addedB.Direction == connection.TerminalB.Direction then
            local group = KnapcodeFactorioTools.CollectionExtensions.First1(groups, function (g)
              return g:HasTerminal(addedB)
            end, class.Group)
            group:Add(connection.TerminalA)
            group.Paths:Add(connection.Line)
            addedPumpjacks:Add(connection.TerminalA)
            leftoverPumpjacks:Remove(connection.TerminalA.Center)
            break
          end

          if addedA ~= nil and addedB == nil and addedA.Direction == connection.TerminalA.Direction then
            local group = KnapcodeFactorioTools.CollectionExtensions.First1(groups, function (g)
              return g:HasTerminal(addedA)
            end, class.Group)
            group:Add(connection.TerminalB)
            group.Paths:Add(connection.Line)
            addedPumpjacks:Add(connection.TerminalB)
            leftoverPumpjacks:Remove(connection.TerminalB.Center)
            break
          end
        end
      end

      -- if no LINES were generated, add 2 pumpjacks to a group here
      -- this will only happen when only a few pumpjacks need to be connected
      if #groups == 0 then
        local connection = nil
        for i = 0, #allLines - 1 do
          local continue
          repeat
            local line = allLines:get(i)
            local terminalsA = context.CenterToTerminals:get(line.A)
            for j = 0, #terminalsA - 1 do
              local continue
              repeat
                local tA = terminalsA:get(j)
                local terminalsB = context.CenterToTerminals:get(line.B)
                for k = 0, #terminalsB - 1 do
                  local continue
                  repeat
                    local tB = terminalsB:get(k)
                    local goals = context:GetSingleLocationSet(tB.Terminal)

                    if connection ~= nil then
                      -- don't perform a shortest path search if the Manhattan distance (minimum possible) is longer than the best.
                      local minDistance = tA.Terminal:GetManhattanDistance(tB.Terminal)
                      if minDistance >= #connection.Line then
                        continue = true
                        break
                      end
                    end

                    local result = KnapcodeOilField.AStar.GetShortestPath(context, context.Grid, tA.Terminal, goals, true, 1, 1)

                    if not result.Success then
                      System.throw(KnapcodeFactorioTools.FactorioToolsException("A goal should have been reached."))
                    end

                    if connection == nil then
                      connection = class.TerminalPair(tA, tB, result:getPath(), middle)
                      continue = true
                      break
                    end

                    local c = System.Int32.CompareTo((#result:getPath()), #connection.Line)
                    if c < 0 then
                      connection = class.TerminalPair(tA, tB, result:getPath(), middle)
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

        if connection == nil then
          System.throw(KnapcodeFactorioTools.FactorioToolsException("A connection between terminals should have been found."))
        end

        local default = ListListLocation()
        default:Add(connection.Line)
        local group = System.new(class.Group, 2, context, connection, default)
        groups:Add(group)
      end

      -- CONNECT GROUPS
      local maxTries = (strategy == 0 --[[PipeStrategy.FbeOriginal]]) and 3 or 20
      local tries = maxTries
      local aloneGroups = ListGroup()
      local finalGroup = nil

      while #groups > 0 do
        local continue
        repeat
          local group = KnapcodeFactorioTools.CollectionExtensions.MinBy(groups, function (x)
            return KnapcodeFactorioTools.CollectionExtensions.Sum(x.Paths, function (p)
              return #p
            end, ListLocation)
          end, class.Group, System.Int32)
          groups:Remove(group)

          if #groups == 0 then
            if #aloneGroups > 0 and tries > 0 then
              groups:AddRange(aloneGroups)
              groups:Add(group)
              aloneGroups:Clear()
              tries = tries - 1
              continue = true
              break
            end

            finalGroup = group
            break
          end

          local locationToGroup = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(groups, context, function (x)
            return x.Location
          end, function (x)
            return x
          end, class.Group, class.Group)
          locationToGroup:Add(group.Location, group)

          local groupLines = KnapcodeOilField.Helpers.PointsToLines(locationToGroup:getKeys())
          local par = ListGroup(#groupLines)
          for i = 0, #groupLines - 1 do
            local line = groupLines:get(i)
            if KnapcodeOilField.Location.op_Equality(line.A, group.Location) then
              par:Add(locationToGroup:get(line.B))
            elseif KnapcodeOilField.Location.op_Equality(line.B, group.Location) then
              par:Add(locationToGroup:get(line.A))
            end
          end

          local result = GetPathBetweenGroups(context, par, group, 2 + maxTries - tries, strategy)
          if result.Exception ~= nil then
            return KnapcodeFactorioTools.Result.NewException(result.Exception, class.FbeResultInfo)
          end

          local connection = result.Data

          if connection ~= nil then
            connection.FirstGroup:AddRange(group)
            connection.FirstGroup.Paths:AddRange(group.Paths)
            connection.FirstGroup.Paths:Add(connection.Lines:get(0))
          else
            aloneGroups:Add(group)
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      if finalGroup == nil then
        System.throw(KnapcodeFactorioTools.FactorioToolsException("The final group should be initialized at this point."))
      end

      if #aloneGroups > 0 then
        -- Fallback to the modified FBE algorithm if the original cannot connect all of the groups.
        -- Related to https://github.com/teoxoy/factorio-blueprint-editor/issues/254
        if strategy == 0 --[[PipeStrategy.FbeOriginal]] then
          return DelaunayTriangulation(context, middle, 1 --[[PipeStrategy.Fbe]])
        end

        System.throw(KnapcodeFactorioTools.FactorioToolsException("There should be no more alone groups at this point."))
      end

      local sortedLeftoverPumpjacks = ArrayLocation(leftoverPumpjacks:getCount())
      leftoverPumpjacks:CopyTo(SpanLocation.ctorArray(sortedLeftoverPumpjacks))
      System.Array.Sort(sortedLeftoverPumpjacks, function (a, b)
        local aC = a:GetManhattanDistance(middle)
        local bC = b:GetManhattanDistance(middle)
        return System.Int32.CompareTo(aC, bC)
      end)


      for i = 0, #sortedLeftoverPumpjacks - 1 do
        local continue
        repeat
          local center = sortedLeftoverPumpjacks:get(i)
          local centerTerminals = context.CenterToTerminals:get(center)
          local terminalGroups = ListGroup(#centerTerminals)
          for j = 0, #centerTerminals - 1 do
            local terminal = centerTerminals:get(j)
            local default = ListListLocation()
            local extern = ListLocation()
            extern:Add(terminal.Terminal)
            default:Add(extern)
            local group = class.Group(context, terminal, default)
            terminalGroups:Add(group)
          end

          local maxTurns = 2
          while true do
            local continue
            repeat
              local result = GetPathBetweenGroups(context, terminalGroups, finalGroup, maxTurns, strategy)
              if result.Exception ~= nil then
                return KnapcodeFactorioTools.Result.NewException(result.Exception, class.FbeResultInfo)
              end

              local connection = result.Data

              if connection == nil then
                -- Allow more max turns with the modified FBE algorithm.
                -- Related to https://github.com/teoxoy/factorio-blueprint-editor/issues/253
                if strategy == 0 --[[PipeStrategy.FbeOriginal]] then
                  return DelaunayTriangulation(context, middle, 1 --[[PipeStrategy.Fbe]])
                elseif strategy == 0 --[[PipeStrategy.FbeOriginal]] or maxTurns > maxTries then
                  System.throw(KnapcodeFactorioTools.FactorioToolsException("There should be at least one connection between a leftover pumpjack and the final group. Max turns: " .. maxTurns))
                end

                maxTurns = maxTurns + 1
                continue = true
                break
              end

              finalGroup:Add(KnapcodeFactorioTools.CollectionExtensions.Single(connection.FirstGroup:getEntities(), KnapcodeOilField.TerminalLocation))
              finalGroup.Paths:Add(connection.Lines:get(0))
              break
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

      local terminals = finalGroup:getEntities()
      local pipes = context:GetLocationSet2(true)
      for i = 0, #finalGroup.Paths - 1 do
        local path = finalGroup.Paths:get(i)
        for j = 0, #path - 1 do
          pipes:Add(path:get(j))
        end
      end

      return KnapcodeFactorioTools.Result.NewData(class.FbeResultInfo(terminals, pipes, strategy), class.FbeResultInfo)
    end
    GetNextLine = function (lines, addedPumpjacks)
      local next = nil
      local nextIndex = 0
      local nextContainsAddedPumpjack = nil
      local nextAverageDistance = nil

      for i = 0, #lines - 1 do
        local continue
        repeat
          local line = lines:get(i)
          if next == nil then
            next = line
            continue = true
            break
          end

          if not (nextContainsAddedPumpjack ~= nil) then
            nextContainsAddedPumpjack = LineContainsAnAddedPumpjack(addedPumpjacks, next)
          end

          local containsAddedPumpjack = LineContainsAnAddedPumpjack(addedPumpjacks, line)
          local c = System.Boolean.CompareTo(containsAddedPumpjack, System.Nullable.Value(nextContainsAddedPumpjack))
          if c > 0 then
            next = line
            nextIndex = i
            nextContainsAddedPumpjack = containsAddedPumpjack
            nextAverageDistance = nil
            continue = true
            break
          elseif c < 0 then
            continue = true
            break
          end

          c = System.Int32.CompareTo(line.EndpointDistance, next.EndpointDistance)
          if c > 0 then
            next = line
            nextIndex = i
            nextContainsAddedPumpjack = containsAddedPumpjack
            nextAverageDistance = nil
            continue = true
            break
          elseif c < 0 then
            continue = true
            break
          end

          c = System.Int32.CompareTo((#line.Connections), #next.Connections)
          if c < 0 then
            next = line
            nextIndex = i
            nextContainsAddedPumpjack = containsAddedPumpjack
            nextAverageDistance = nil
            continue = true
            break
          elseif c > 0 then
            continue = true
            break
          end

          if not (nextAverageDistance ~= nil) then
            nextAverageDistance = next:GetAverageDistance()
          end

          local averageDistance = line:GetAverageDistance()
          c = System.Double.CompareToObj(averageDistance, nextAverageDistance)
          if c < 0 then
            next = line
            nextIndex = i
            nextContainsAddedPumpjack = containsAddedPumpjack
            nextAverageDistance = averageDistance
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      lines:RemoveAt(nextIndex)

      return next
    end
    LineContainsAnAddedPumpjack = function (addedPumpjacks, ent)
      for i = 0, #addedPumpjacks - 1 do
        local continue
        repeat
          local terminal = addedPumpjacks:get(i)
          if KnapcodeOilField.Location.op_Inequality(ent.Endpoints.A, terminal.Center) and KnapcodeOilField.Location.op_Inequality(ent.Endpoints.B, terminal.Center) then
            continue = true
            break
          end

          for j = 0, #ent.Connections - 1 do
            local pair = ent.Connections:get(j)
            if pair.TerminalA.Direction == terminal.Direction or pair.TerminalB.Direction == terminal.Direction then
              return true
            end
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      return false
    end
    GetPathBetweenGroups = function (context, groups, group, maxTurns, strategy)
      local best = nil
      for i = 0, #groups - 1 do
        local continue
        repeat
          local g = groups:get(i)
          local result = ConnectTwoGroups(context, g, group, maxTurns, strategy)
          if result.Exception ~= nil then
            return result
          end

          local connection = result.Data

          if #connection.Lines == 0 then
            continue = true
            break
          end

          if best == nil then
            best = connection
            continue = true
            break
          end

          local c = System.Int32.CompareTo(connection.MinDistance, best.MinDistance)
          if c < 0 then
            best = connection
          end
          continue = true
        until 1
        if not continue then
          break
        end
      end

      return KnapcodeFactorioTools.Result.NewData(best, class.TwoConnectedGroups)
    end
    ConnectTwoGroups = function (context, a, b, maxTurns, strategy)
      local aLocations = ListLocation()
      for i = 0, #a.Paths - 1 do
        aLocations:AddRange(a.Paths:get(i))
      end

      local bLocations = ListLocation()
      for i = 0, #b.Paths - 1 do
        bLocations:AddRange(b.Paths:get(i))
      end

      local lineInfo = ListPathAndTurns()
      for i = 0, #aLocations - 1 do
        local continue
        repeat
          local al = aLocations:get(i)
          for j = 0, #bLocations - 1 do
            local continue
            repeat
              local bl = bLocations:get(j)
              if al.X ~= bl.X and al.Y ~= bl.Y then
                continue = true
                break
              end

              local line = KnapcodeOilField.Helpers.MakeStraightLineOnEmpty(context.Grid, al, bl)
              if line == nil then
                continue = true
                break
              end

              lineInfo:Add(class.PathAndTurns(KnapcodeOilField.Endpoints(line:get(0), line:get(#line - 1)), line, 0, #lineInfo))
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

      if #aLocations == 1 then
        local locationToIndex = context:GetLocationDictionary1(#bLocations, System.Int32)
        for i = 0, #bLocations - 1 do
          locationToIndex:TryAdd(bLocations:get(i), i)
        end

        bLocations:Sort(function (a, b)
          local aC = aLocations:get(0):GetManhattanDistance(a)
          local bC = aLocations:get(0):GetManhattanDistance(b)
          local c = System.Int32.CompareTo(aC, bC)
          if c ~= 0 then
            return c
          end

          return System.Int32.CompareTo(locationToIndex:get(a), locationToIndex:get(b))
        end)

        if #bLocations > 20 then
          bLocations:RemoveRange(20, #bLocations - 20)
        end
      end

      local aPlusB = context:GetLocationSet6(#aLocations + #bLocations, true)
      aPlusB:UnionWith(aLocations)
      aPlusB:UnionWith(bLocations)

      local allEndpoints = KnapcodeOilField.Helpers.PointsToLines(aPlusB:EnumerateItems())
      local matches = ListTuple(#allEndpoints)
      for i = 0, #allEndpoints - 1 do
        local continue
        repeat
          local pair = allEndpoints:get(i)
          if (aLocations:Contains(pair.A) and aLocations:Contains(pair.B)) or (bLocations:Contains(pair.A) and bLocations:Contains(pair.B)) then
            continue = true
            break
          end

          matches:Add(System.Tuple(pair, pair.A:GetManhattanDistance(pair.B), #matches))
          continue = true
        until 1
        if not continue then
          break
        end
      end

      matches:Sort(function (a, b)
        local c = System.Int32.CompareTo(a[2], b[2])
        if c ~= 0 then
          return c
        end

        return System.Int32.CompareTo(a[3], b[3])
      end)

      local takeLines = math.Min(#matches, 5)
      for i = 0, takeLines - 1 do
        local continue
        repeat
          local l = matches:get(i)[1]

          local path
          if strategy == 0 --[[PipeStrategy.FbeOriginal]] then
            -- We can't terminal early based on max turns because this leads to different results since it allows
            -- secondary path options that would have otherwise been not considered for a given start and goal state.
            path = KnapcodeOilField.BreadthFirstFinder.GetShortestPath(context, l.B, l.A)
            if path == nil then
              -- Visualizer.Show(context.Grid, new[] { l.A, l.B }.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
              return KnapcodeFactorioTools.Result.NewException(KnapcodeFactorioTools.NoPathBetweenTerminalsException(l.A, l.B), class.TwoConnectedGroups)
            end
          else
            local goals = context:GetSingleLocationSet(l.B)
            local result = KnapcodeOilField.AStar.GetShortestPath(context, context.Grid, l.A, goals, true, 1, 1)
            if not result.Success then
              -- Visualizer.Show(context.Grid, new[] { l.A, l.B }.Select(p => (IPoint)new Point(p.X, p.Y)), Array.Empty<IEdge>());
              return KnapcodeFactorioTools.Result.NewException(KnapcodeFactorioTools.NoPathBetweenTerminalsException(l.A, l.B), class.TwoConnectedGroups)
            end

            path = result:getPath()
          end

          local turns = KnapcodeOilField.Helpers.CountTurns(path)
          if turns == 0 or turns > maxTurns then
            continue = true
            break
          end

          lineInfo:Add(class.PathAndTurns(l, path, turns, #lineInfo))
          continue = true
        until 1
        if not continue then
          break
        end
      end

      lineInfo:Sort(function (a, b)
        local c = System.Int32.CompareTo((#a.Path), #b.Path)
        if c ~= 0 then
          return c
        end

        return System.Int32.CompareTo(a.OriginalIndex, b.OriginalIndex)
      end)

      local lines = ListListLocation(#lineInfo)
      local minCount = (#lineInfo == 0) and 0 or 2147483647 --[[Int32.MaxValue]]
      for i = 0, #lineInfo - 1 do
        local path = lineInfo:get(i).Path
        lines:Add(path)
        if #path < minCount then
          minCount = #path
        end
      end

      return KnapcodeFactorioTools.Result.NewData(class.TwoConnectedGroups(lines, minCount, a), class.TwoConnectedGroups)
    end
    class = {
      Execute = Execute
    }
    return class
  end)
end)
