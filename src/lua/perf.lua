print("Loading modules")
require("CoreSystem")()
require("DelaunatorSharp.manifest")("DelaunatorSharp")
require("FluteSharp.manifest")("FluteSharp")
require("FactorioTools.manifest")("FactorioTools")

print("Running perf")
local timingSum = 0
local timingCount = 0
for i = 1, 11, 1 do
    local start = os.clock()
    local context, summary = Knapcode.FactorioTools.OilField.Planner.ExecuteSample():Deconstruct()
    local stop = os.clock()

    if i == 1 then
        print(context.Grid:ToString())
        print("Warmup: " .. (stop - start))
    else
        print((i - 2) .. ": " .. (stop - start))
        timingSum = timingSum + (stop - start)
        timingCount = timingCount + 1
    end
end

print("Average: " .. (timingSum / timingCount))
