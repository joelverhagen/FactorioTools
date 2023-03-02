import { def } from '@vue/shared';
import { defineStore, Store } from 'pinia'
import { StorageLike } from 'pinia-plugin-persistedstate';
import { LocationQuery } from 'vue-router';

export function getDefaults() {
  return {
    usingQueryString: false,
    inputBlueprint: '',
    useAdvancedOptions: false,
    pumpjackModule: 'productivity-module-3',
    pumpjackModuleIsCustom: false,
    addBeacons: true,
    beaconModule: 'speed-module-3',
    beaconModuleIsCustom: false,
    beaconModuleSlots: 2,
    beaconEntityName: 'beacon',
    beaconSupplyWidth: 9,
    beaconSupplyHeight: 9,
    beaconWidth: 3,
    beaconHeight: 3,
    electricPoleEntityName: 'medium-electric-pole',
    electricPoleIsCustom: false,
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
  }
}

const queryToStore = {
  'source': 'inputBlueprint',
  'adv': 'useAdvancedOptions',
  'pumpMod': 'pumpjackModule',
  'pumpModCust': 'pumpjackModuleIsCustom',
  'beacons': 'addBeacons',
  'beaconMod': 'beaconModule',
  'beaconModCust': 'beaconModuleIsCustom',
  'beaconModSlots': 'beaconModuleSlots',
  'beacon': 'beaconEntityName',
  'beaconSupW': 'beaconSupplyWidth',
  'beaconSupH': 'beaconSupplyHeight',
  'beaconW': 'beaconWidth',
  'beaconH': 'beaconHeight',
  'pole': 'electricPoleEntityName',
  'poleCust': 'electricPoleIsCustom',
  'poleW': 'electricPoleWidth',
  'poleH': 'electricPoleHeight',
  'poleSupW': 'electricPoleSupplyWidth',
  'poleSupH': 'electricPoleSupplyHeight',
  'poleReach': 'electricPoleWireReach',
  'underground': 'useUndergroundPipes',
  'optimize': 'optimizePipes',
  'val': 'validateSolution',
  'pipesFbe': 'pipeStrategyFbe',
  'pipesCcDt': 'pipeStrategyConnectedCentersDelaunay',
  'pipesCcDtMst': 'pipeStrategyConnectedCentersDelaunayMst',
  'pipesCcFlute': 'pipeStrategyConnectedCentersFlute',
  'beaconsFbe': 'beaconStrategyFbe',
  'beaconsSnug': 'beaconStrategySnug',
} as { [k: string]: string }

const storeToQuery = Object.fromEntries(Object.entries(queryToStore).map(([key, value]) => [value, key]))
const defaults = getDefaults();

if (Object.keys(storeToQuery).length != Object.keys(queryToStore).length) {
  throw new Error(`inconsistent query parameter mapping`)
}

const expectedStoreKeys = Object.keys(getDefaults())
for (const value of Object.values(queryToStore)) {
  const index = expectedStoreKeys.indexOf(value);
  if (index < 0) {
    console.log('expected store keys:', expectedStoreKeys)
    throw new Error(`missing query parameter mapping for store key ${value}`)
  }
  expectedStoreKeys.splice(index, 1)
}

type OilFieldStoreState = typeof defaults
type OilFieldStore = Store<"OilFieldStore", OilFieldStoreState, {}, {}>

class ToggleStorage implements StorageLike {
  private readOnly: boolean = false;

  getItem(key: string): string | null {
    return localStorage.getItem(key)
  }
  setItem(key: string, value: string): void {
    if (!this.readOnly) {
      localStorage.setItem(key, value)
    }
  }
  setReadOnly(readOnly: boolean) {
    this.readOnly = readOnly
  }
}

const toggleStorage = new ToggleStorage()

function getStore(): OilFieldStore {
  return defineStore('OilFieldStore', {
    state: () => getDefaults(),
    persist: {
      storage: toggleStorage
    }
  })();
}

function hasMatchingQueryString(query: LocationQuery) {
  let matching = 0;
  for (const queryKey of Object.keys(queryToStore)) {
    if (queryKey in query) {
      matching++
    }
  }

  return matching > 0
}

function populateStoreFromQuery(query: LocationQuery) {
  const store = useOilFieldStore()
  for (const [storeKey, storeValue] of Object.entries(store.$state)) {
    if (storeKey in defaults) {
      const queryKey = storeToQuery[storeKey]
      let newValue = queryKey && queryKey in query ? query[queryKey] : (defaults as any)[storeKey] as any
      switch (typeof storeValue) {
        case 'boolean':
          newValue = newValue == 'true' || newValue == '1'
          break
        case 'number':
          newValue = parseFloat(newValue)
          break
      }
      (store as any)[storeKey] = newValue;
    }
  }

  return store
}

export function initializeOilFieldStore(query: LocationQuery) {
  if (hasMatchingQueryString(query)) {
    toggleStorage.setReadOnly(true)
    const store = populateStoreFromQuery(query)
    store.usingQueryString = true;
  } else {
    getStore().usingQueryString = false;
  }
}

export function generateQueryString() {
  const store = useOilFieldStore()
  const pieces = []
  for (const [storeKey, storeValue] of Object.entries(store.$state)) {
    const queryKey = storeToQuery[storeKey]
    if (queryKey) {
      let queryValue = storeValue;
      if (typeof storeValue == 'boolean') {
        queryValue = storeValue ? '1' : '0'
      }
      pieces.push(`${queryKey}=${encodeURIComponent(queryValue)}`)
    }
  }

  return pieces.join('&')
}

export function persistStore() {
  const store = getStore()
  if (!store.usingQueryString) {
    throw new Error('cannot persist from a store that is not using a query string')
  }

  toggleStorage.setReadOnly(false)
  store.usingQueryString = false
  store.$persist()
  toggleStorage.setReadOnly(true)
  store.usingQueryString = true
}

export function useOilFieldStore() {
  return getStore()
}