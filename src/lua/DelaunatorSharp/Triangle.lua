-- Generated by CSharp.lua Compiler
local System = System
System.namespace("DelaunatorSharp", function (namespace)
  namespace.struct("Triangle", function (namespace)
    local getIndex, setIndex, getPoints, setPoints, __ctor__
    __ctor__ = function (this, t, points)
      if t == nil then
        return
      end
      this.Points = points
      this.Index = t
    end
    getIndex, setIndex = System.property("Index")
    getPoints, setPoints = System.property("Points")
    return {
      base = function (out)
        return {
          out.DelaunatorSharp.ITriangle
        }
      end,
      Index = 0,
      getIndex = getIndex,
      setIndex = setIndex,
      getPoints = getPoints,
      setPoints = setPoints,
      __ctor__ = __ctor__
    }
  end)
end)