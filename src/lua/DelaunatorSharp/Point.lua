-- Generated by CSharp.lua Compiler
local System = System
System.namespace("DelaunatorSharp", function (namespace)
  namespace.struct("Point", function (namespace)
    local getX, setX, getY, setY, ToString, __ctor__
    __ctor__ = function (this, x, y)
      if x == nil then
        return
      end
      this.X = x
      this.Y = y
    end
    getX, setX = System.property("X")
    getY, setY = System.property("Y")
    ToString = function (this)
      return this.X .. "," .. this.Y
    end
    return {
      base = function (out)
        return {
          out.DelaunatorSharp.IPoint
        }
      end,
      X = 0,
      getX = getX,
      setX = setX,
      Y = 0,
      getY = getY,
      setY = setY,
      ToString = ToString,
      __ctor__ = __ctor__
    }
  end)
end)
