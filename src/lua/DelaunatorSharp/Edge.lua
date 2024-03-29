-- Generated by CSharp.lua Compiler
local System = System
System.namespace("DelaunatorSharp", function (namespace)
  namespace.struct("Edge", function (namespace)
    local getP, setP, getQ, setQ, getIndex, setIndex, __ctor__
    __ctor__ = function (this, e, p, q)
      if e == nil then
        return
      end
      this.Index = e
      this.P = p
      this.Q = q
    end
    getP, setP = System.property("P")
    getQ, setQ = System.property("Q")
    getIndex, setIndex = System.property("Index")
    return {
      base = function (out)
        return {
          out.DelaunatorSharp.IEdge
        }
      end,
      getP = getP,
      setP = setP,
      getQ = getQ,
      setQ = setQ,
      Index = 0,
      getIndex = getIndex,
      setIndex = setIndex,
      __ctor__ = __ctor__
    }
  end)
end)
