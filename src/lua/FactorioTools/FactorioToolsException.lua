-- Generated by CSharp.lua Compiler
local System = System
System.namespace("Knapcode.FactorioTools", function (namespace)
  namespace.class("FactorioToolsException", function (namespace)
    local __ctor1__, __ctor2__, __ctor3__
    __ctor1__ = function (this, message)
      __ctor2__(this, message, false)
      this.BadInput = false
    end
    __ctor2__ = function (this, message, badInput)
      System.Exception.__ctor__(this, message)
      this.BadInput = badInput
    end
    __ctor3__ = function (this, message, innerException, badInput)
      System.Exception.__ctor__(this, message, innerException)
      this.BadInput = badInput
    end
    return {
      base = function (out, this)
        local base = System.Exception
        this.__tostring = base.__tostring
        return {
          base
        }
      end,
      BadInput = false,
      __ctor__ = {
        __ctor1__,
        __ctor2__,
        __ctor3__
      }
    }
  end)
end)