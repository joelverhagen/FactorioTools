-- Generated by CSharp.lua Compiler
local System = System
local KnapcodeOilField
local KnapcodeTableArray
System.import(function (out)
  KnapcodeOilField = Knapcode.FactorioTools.OilField
  KnapcodeTableArray = Knapcode.FactorioTools.OilField.TableArray
end)
System.namespace("Knapcode.FactorioTools.OilField", function (namespace)
  namespace.class("TableArray", function (namespace)
    local New, New1, New2, Empty
    namespace.class("EmptyInstances_1", function (namespace)
      return function (T)
        local Instance, static
        static = function (this)
          Instance = New1(0, T)
          this.Instance = Instance
        end
        return {
          static = static
        }
      end
    end)
    New = function (T)
      return KnapcodeOilField.DictionaryTableArray_1(T)()
    end
    New1 = function (capacity, T)
      return System.new(KnapcodeOilField.DictionaryTableArray_1(T), 2, capacity)
    end
    New2 = function (item, T)
      local list = New(T)
      list:Add(item)
      return list
    end
    Empty = function (T)
      return KnapcodeTableArray.EmptyInstances_1(T).Instance
    end
    return {
      New = New,
      New1 = New1,
      New2 = New2,
      Empty = Empty
    }
  end)
end)
