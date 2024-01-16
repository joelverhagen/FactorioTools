print("Loading modules")
require("CoreSystem")()
require("DelaunatorSharp.manifest")("DelaunatorSharp")
require("FluteSharp.manifest")("FluteSharp")
require("FactorioTools.manifest")("FactorioTools")

print("Running sample")
local result = Knapcode.FactorioTools.OilField.Planner.ExecuteSample()

print("Displaying results")
local context, sample = result:Deconstruct()
print(context.Grid:ToString())