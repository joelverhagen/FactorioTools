﻿-- Generated by CSharp.lua Compiler
return function (path)
  return System.init({
    path = path,
    files = {
      "LocationIntDictionary",
      "AddElectricPoles",
      "AddPipeEntities",
      "AddPipes_0",
      "AStar",
      "AStarResult",
      "AvoidEntity",
      "AvoidLocation",
      "BeaconCenter",
      "BeaconSide",
      "BeaconSolution",
      "Blueprint",
      "BlueprintBook",
      "BlueprintPage",
      "BlueprintRoot",
      "BreadthFirstFinder",
      "BresenhamsLine",
      "CandidateInfo",
      "CleanBlueprint",
      "CollectionExtensions",
      "Context",
      "CustomCountedBitArray",
      "Dijkstras",
      "DijkstrasResult",
      "Direction",
      "ElectricPoleCandidateInfo",
      "ElectricPoleCenter",
      "ElectricPoleSide",
      "EmptyLocationSet",
      "Endpoints",
      "Entity",
      "EntityNames",
      "ExistingPipeGrid",
      "FactorioToolsException",
      "GridEntity",
      "Helpers",
      "ICandidateFactory",
      "Icon",
      "ILocationDictionary",
      "ILocationSet",
      "InitializeContext",
      "InitializeFLUTE",
      "ItemNames",
      "Location",
      "LocationBitSet",
      "LocationHashDictionary",
      "LocationHashSet",
      "LocationIntSet",
      "NoPathBetweenTerminalsException",
      "OilFieldOptions",
      "OilFieldPlan",
      "OilFieldPlanSummary",
      "Pipe",
      "PipeGrid",
      "PlanBeacons_0",
      "Planner",
      "PlanUndergroundPipes",
      "Position",
      "Prims",
      "ProviderRecipient",
      "PumpjackCenter",
      "PumpjackSide",
      "RotateOptimize",
      "SetHandling",
      "SharedInstances",
      "SignalID",
      "SignalTypes",
      "SingleLocationSet",
      "SortedBatches",
      "SquareGrid",
      "TemporaryEntity",
      "Terminal",
      "TerminalLocation",
      "UndergroundPipe",
      "Validate"
    },
    types = {
      "Knapcode.FactorioTools.OilField.CandidateInfo",
      "Knapcode.FactorioTools.OilField.PlanBeacons",
      "Knapcode.FactorioTools.OilField.GridEntity",
      "Knapcode.FactorioTools.Data.EntityNames",
      "Knapcode.FactorioTools.Data.ItemNames",
      "Knapcode.FactorioTools.Data.SignalTypes",
      "Knapcode.FactorioTools.FactorioToolsException",
      "Knapcode.FactorioTools.OilField.AddElectricPoles",
      "Knapcode.FactorioTools.OilField.ElectricPoleCandidateInfo",
      "Knapcode.FactorioTools.OilField.ICandidateFactory_1",
      "Knapcode.FactorioTools.OilField.AddPipes",
      "Knapcode.FactorioTools.OilField.ILocationDictionary_1",
      "Knapcode.FactorioTools.OilField.ILocationSet",
      "Knapcode.FactorioTools.OilField.SquareGrid",
      "Knapcode.FactorioTools.OilField.PlanBeacons.BeaconCandidateInfo",
      "Knapcode.FactorioTools.OilField.Planner",
      "Knapcode.FactorioTools.OilField.RotateOptimize",
      "Knapcode.FactorioTools.OilField.Pipe",
      "Knapcode.FactorioTools.CollectionExtensions",
      "Knapcode.FactorioTools.Data.Blueprint",
      "Knapcode.FactorioTools.Data.BlueprintBook",
      "Knapcode.FactorioTools.Data.BlueprintPage",
      "Knapcode.FactorioTools.Data.BlueprintRoot",
      "Knapcode.FactorioTools.Data.Direction",
      "Knapcode.FactorioTools.Data.Entity",
      "Knapcode.FactorioTools.Data.EntityNames.AaiIndustry",
      "Knapcode.FactorioTools.Data.EntityNames.Vanilla",
      "Knapcode.FactorioTools.Data.Icon",
      "Knapcode.FactorioTools.Data.ItemNames.Vanilla",
      "Knapcode.FactorioTools.Data.Position",
      "Knapcode.FactorioTools.Data.SignalID",
      "Knapcode.FactorioTools.Data.SignalTypes.Vanilla",
      "Knapcode.FactorioTools.NoPathBetweenTerminalsException",
      "Knapcode.FactorioTools.OilField.AStar",
      "Knapcode.FactorioTools.OilField.AStarResult",
      "Knapcode.FactorioTools.OilField.AddElectricPoles.CandidateComparerForSameCoveredCount",
      "Knapcode.FactorioTools.OilField.AddElectricPoles.CandidateComparerForSamePriorityPowered",
      "Knapcode.FactorioTools.OilField.AddElectricPoles.CandidateFactory",
      "Knapcode.FactorioTools.OilField.AddPipeEntities",
      "Knapcode.FactorioTools.OilField.AddPipes.BestConnection",
      "Knapcode.FactorioTools.OilField.AddPipes.ConnectedCentersComparer",
      "Knapcode.FactorioTools.OilField.AddPipes.FlutePoint",
      "Knapcode.FactorioTools.OilField.AddPipes.Group",
      "Knapcode.FactorioTools.OilField.AddPipes.GroupCandidate",
      "Knapcode.FactorioTools.OilField.AddPipes.LocationSetComparer",
      "Knapcode.FactorioTools.OilField.AddPipes.PathAndTurns",
      "Knapcode.FactorioTools.OilField.AddPipes.PlanInfo",
      "Knapcode.FactorioTools.OilField.AddPipes.PumpjackConnection",
      "Knapcode.FactorioTools.OilField.AddPipes.PumpjackGroup",
      "Knapcode.FactorioTools.OilField.AddPipes.Solution",
      "Knapcode.FactorioTools.OilField.AddPipes.SolutionsAndGroupNumber",
      "Knapcode.FactorioTools.OilField.AddPipes.TerminalPair",
      "Knapcode.FactorioTools.OilField.AddPipes.Trunk",
      "Knapcode.FactorioTools.OilField.AddPipes.TwoConnectedGroups",
      "Knapcode.FactorioTools.OilField.AvoidEntity",
      "Knapcode.FactorioTools.OilField.AvoidLocation",
      "Knapcode.FactorioTools.OilField.BeaconCenter",
      "Knapcode.FactorioTools.OilField.BeaconSide",
      "Knapcode.FactorioTools.OilField.BeaconSolution",
      "Knapcode.FactorioTools.OilField.BreadthFirstFinder",
      "Knapcode.FactorioTools.OilField.BresenhamsLine",
      "Knapcode.FactorioTools.OilField.CleanBlueprint",
      "Knapcode.FactorioTools.OilField.Context",
      "Knapcode.FactorioTools.OilField.CustomCountedBitArray",
      "Knapcode.FactorioTools.OilField.Dijkstras",
      "Knapcode.FactorioTools.OilField.DijkstrasResult",
      "Knapcode.FactorioTools.OilField.ElectricPoleCenter",
      "Knapcode.FactorioTools.OilField.ElectricPoleSide",
      "Knapcode.FactorioTools.OilField.EmptyLocationSet",
      "Knapcode.FactorioTools.OilField.Endpoints",
      "Knapcode.FactorioTools.OilField.ExistingPipeGrid",
      "Knapcode.FactorioTools.OilField.Helpers",
      "Knapcode.FactorioTools.OilField.InitializeContext",
      "Knapcode.FactorioTools.OilField.InitializeFLUTE",
      "Knapcode.FactorioTools.OilField.Location",
      "Knapcode.FactorioTools.OilField.LocationBitSet",
      "Knapcode.FactorioTools.OilField.LocationHashDictionary_1",
      "Knapcode.FactorioTools.OilField.LocationHashSet",
      "Knapcode.FactorioTools.OilField.LocationIntDictionary_1",
      "Knapcode.FactorioTools.OilField.LocationIntSet",
      "Knapcode.FactorioTools.OilField.OilFieldOptions",
      "Knapcode.FactorioTools.OilField.OilFieldPlan",
      "Knapcode.FactorioTools.OilField.OilFieldPlanSummary",
      "Knapcode.FactorioTools.OilField.PipeGrid",
      "Knapcode.FactorioTools.OilField.PlanBeacons.Area",
      "Knapcode.FactorioTools.OilField.PlanBeacons.BeaconCandidate",
      "Knapcode.FactorioTools.OilField.PlanBeacons.CandidateFactory",
      "Knapcode.FactorioTools.OilField.PlanBeacons.SnugCandidateSorter",
      "Knapcode.FactorioTools.OilField.PlanUndergroundPipes",
      "Knapcode.FactorioTools.OilField.Prims",
      "Knapcode.FactorioTools.OilField.ProviderRecipient",
      "Knapcode.FactorioTools.OilField.PumpjackCenter",
      "Knapcode.FactorioTools.OilField.PumpjackSide",
      "Knapcode.FactorioTools.OilField.RotateOptimize.ChildContext",
      "Knapcode.FactorioTools.OilField.RotateOptimize.ExploredPaths",
      "Knapcode.FactorioTools.OilField.SharedInstances",
      "Knapcode.FactorioTools.OilField.SingleLocationSet",
      "Knapcode.FactorioTools.OilField.SortedBatches_1",
      "Knapcode.FactorioTools.OilField.SquareGrid.Empty",
      "Knapcode.FactorioTools.OilField.TemporaryEntity",
      "Knapcode.FactorioTools.OilField.Terminal",
      "Knapcode.FactorioTools.OilField.TerminalLocation",
      "Knapcode.FactorioTools.OilField.UndergroundPipe",
      "Knapcode.FactorioTools.OilField.Validate",
      "Knapcode.FactorioTools.SetHandling"
    }
  })
end

