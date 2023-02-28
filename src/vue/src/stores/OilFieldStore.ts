import { defineStore } from 'pinia'

export const useOilFieldStore = defineStore('OilFieldStore', {
  state: () => ({
    showAdvancedOptions: true,
    inputBlueprint: '',
    pumpjackModule: 'productivity-module-3',
    addBeacons: true,
    beaconModule: 'speed-module-3',
    beaconEntityName: 'beacon',
    beaconSupplyWidth: 9,
    beaconSupplyHeight: 9,
    beaconWidth: 3,
    beaconHeight: 3,
    electricPoleEntityName: 'medium-electric-pole',
    electricPoleWidth: 1,
    electricPoleHeight: 1,
    electricPoleSupplyWidth: 7,
    electricPoleSupplyHeight: 7,
    electricPoleWireReach: 9,
    useUndergroundPipes: true,
    optimizePipes: true,
    validateSolution: false,
    pipeStrategyFbe: true,
    pipeStrategyConnectedCentersDelaunay: true,
    pipeStrategyConnectedCentersDelaunayMst: true,
    pipeStrategyConnectedCentersFlute: true,
    beaconStrategyFbe: true,
    beaconStrategySnug: true,
  }),
  persist: true
})
