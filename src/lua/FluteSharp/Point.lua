-- Generated by CSharp.lua Compiler
local System = System
System.namespace("Knapcode.FluteSharp", function (namespace)
  namespace.struct("Point", function (namespace)
    local EqualsObj, Equals, GetHashCode, op_Equality, op_Inequality, class, __ctor__
    __ctor__ = function (this, x, y)
      if x == nil then
        return
      end
      this.X = x
      this.Y = y
    end
    EqualsObj = function (this, obj)
      local point = obj
      return System.is(point, class) and Equals(this, point:__clone__())
    end
    Equals = function (this, other)
      return this.X == other.X and this.Y == other.Y
    end
    GetHashCode = function (this)
      return System.HashCode.Combine(this.X, this.Y, System.Int32, System.Int32)
    end
    op_Equality = function (left, right)
      return Equals(left, right:__clone__())
    end
    op_Inequality = function (left, right)
      return not (op_Equality(left, right))
    end
    class = {
      base = function (out)
        return {
          System.IEquatable_1(out.Knapcode.FluteSharp.Point)
        }
      end,
      X = 0,
      Y = 0,
      EqualsObj = EqualsObj,
      Equals = Equals,
      GetHashCode = GetHashCode,
      op_Equality = op_Equality,
      op_Inequality = op_Inequality,
      __ctor__ = __ctor__
    }
    return class
  end)
end)