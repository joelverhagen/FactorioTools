-- Generated by CSharp.lua Compiler
local System = System
local DelaunatorSharp = DelaunatorSharp
local ArrayIPoint = System.Array(DelaunatorSharp.IPoint)
local KnapcodeFactorioTools
local KnapcodeOilField
local ILocationDictionary_1Int32
System.import(function (out)
  KnapcodeFactorioTools = Knapcode.FactorioTools
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  ILocationDictionary_1Int32 = KnapcodeOilField.ILocationDictionary_1(System.Int32)
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("AddPipesConnectedCentersDT", function (namespace)
    local ExecuteWithDelaunay, ExecuteWithDelaunayMst, GetDelauntator
    ExecuteWithDelaunay = function (context, centers)
      local delaunator = GetDelauntator(centers)
      local dlGraph = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(centers, context, function (c)
        return c
      end, function (c)
        return context:GetLocationSet2(true)
      end, KnapcodeOilField.Location, KnapcodeOilField.ILocationSet)

      for e = 0, #delaunator.Triangles - 1 do
        if e > delaunator.Halfedges:get(e) then
          local p = centers:get(delaunator.Triangles:get(e))
          local q = centers:get(delaunator.Triangles:get((System.mod(e, 3) == 2) and (e - 2) or (e + 1)))

          dlGraph:get(p):Add(q)
          dlGraph:get(q):Add(p)
        end
      end

      return dlGraph
    end
    ExecuteWithDelaunayMst = function (context, centers)
      local delaunator = GetDelauntator(centers)
      local dlGraph = KnapcodeFactorioTools.CollectionExtensions.ToDictionary(centers, context, function (c)
        return c
      end, function (c)
        return context:GetLocationDictionary(System.Int32)
      end, KnapcodeOilField.Location, ILocationDictionary_1Int32)

      for e = 0, #delaunator.Triangles - 1 do
        if e > delaunator.Halfedges:get(e) then
          local p = centers:get(delaunator.Triangles:get(e))
          local q = centers:get(delaunator.Triangles:get((System.mod(e, 3) == 2) and (e - 2) or (e + 1)))

          local cost = p:GetEuclideanDistanceSquared(q)
          dlGraph:get(p):set(q, cost)
          dlGraph:get(q):set(p, cost)
        end
      end

      local closestToMiddle = KnapcodeFactorioTools.CollectionExtensions.MinBy(centers, System.fn(context.Grid.Middle, context.Grid.Middle.GetEuclideanDistanceSquared), KnapcodeOilField.Location, System.Int32)
      local mst = KnapcodeOilField.Prims.GetMinimumSpanningTree(context, dlGraph, closestToMiddle, false)

      return mst
    end
    GetDelauntator = function (centers)
      local points = ArrayIPoint(#centers)
      for i = 0, #centers - 1 do
        local center = centers:get(i)
        points:set(i, DelaunatorSharp.Point(center.X, center.Y))
      end
      local delaunator = DelaunatorSharp.Delaunator(points)
      return delaunator
    end
    return {
      ExecuteWithDelaunay = ExecuteWithDelaunay,
      ExecuteWithDelaunayMst = ExecuteWithDelaunayMst
    }
  end)
end)