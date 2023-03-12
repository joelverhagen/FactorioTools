import { defineStore, Store } from 'pinia'
import { StorageLike } from 'pinia-plugin-persistedstate';
import { LocationQuery } from 'vue-router';
import { getEntries } from '../lib/helpers';

const defaults = {
  usingQueryString: false,
  useStagingApi: false,
  inputBlueprint: '',
  useAdvancedOptions: false,
  pumpjackModule: 'productivity-module-3',
  pumpjackModuleIsCustom: false,
  addBeacons: true,
  overlapBeacons: true,
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
  beaconStrategyFbeOriginal: false,
  beaconStrategyFbe: true,
  beaconStrategySnug: true,
};
export type OilFieldStoreState = typeof defaults

type StoreToQuery = {
  [Property in keyof OilFieldStoreState as Exclude<Property, "usingQueryString" | "useStagingApi">]: string
};

const storeToQuery: StoreToQuery = {
  inputBlueprint: 'source',
  useAdvancedOptions: 'adv',
  pumpjackModule: 'pumpMod',
  pumpjackModuleIsCustom: 'pumpModCust',
  addBeacons: 'beacons',
  overlapBeacons: 'overlapBeacons',
  beaconModule: 'beaconMod',
  beaconModuleIsCustom: 'beaconModCust',
  beaconModuleSlots: 'beaconModSlots',
  beaconEntityName: 'beacon',
  beaconSupplyWidth: 'beaconSupW',
  beaconSupplyHeight: 'beaconSupH',
  beaconWidth: 'beaconW',
  beaconHeight: 'beaconH',
  electricPoleEntityName: 'pole',
  electricPoleIsCustom: 'poleCust',
  electricPoleWidth: 'poleW',
  electricPoleHeight: 'poleH',
  electricPoleSupplyWidth: 'poleSupW',
  electricPoleSupplyHeight: 'poleSupH',
  electricPoleWireReach: 'poleReach',
  useUndergroundPipes: 'underground',
  optimizePipes: 'optimize',
  validateSolution: 'val',
  pipeStrategyFbe: 'pipesFbe',
  pipeStrategyConnectedCentersDelaunay: 'pipesCcDt',
  pipeStrategyConnectedCentersDelaunayMst: 'pipesCcDtMst',
  pipeStrategyConnectedCentersFlute: 'pipesCcFlute',
  beaconStrategyFbeOriginal: 'beaconsFbeO',
  beaconStrategyFbe: 'beaconsFbe',
  beaconStrategySnug: 'beaconsSnug',
} as const;

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
    state: () => Object.assign({}, defaults),
    persist: {
      storage: toggleStorage
    }
  })();
}

export function hasMatchingQueryString(query: LocationQuery | URLSearchParams, writeLog: boolean = true) {
  const keys = query instanceof URLSearchParams ? Array.from(query.keys()) : Object.keys(query)
  let matching = 0;
  if (keys.length > 0) {
    for (const [_, queryKey] of getEntries(storeToQuery)) {
      if (keys.includes(queryKey)) {
        matching++
      }
    }

    if (writeLog) {
      console.log(`matched ${matching} query params, ignored ${keys.length - matching}`)
    }
  }
  return matching > 0
}

function populateStoreFromQuery(query: LocationQuery) {
  const store = useOilFieldStore()
  for (const [storeKey, storeValue] of getEntries(store.$state)) {
    if (storeKey == 'usingQueryString' || storeKey == 'useStagingApi') {
      continue
    }

    const queryKey = storeToQuery[storeKey]
    let queryValue = query[queryKey];
    if (Array.isArray(queryValue)) {
      queryValue = queryValue.length > 0 ? queryValue[0] : null;
    }

    let newValue = queryValue ?? defaults[storeKey];
    switch (typeof storeValue) {
      case 'boolean':
        newValue = newValue == 'true' || newValue == '1'
        break
      case 'number':
        newValue = parseFloat(newValue?.toString())
        break
    }

    (store as any)[storeKey] = newValue;
  }

  return store
}

export function getDefaults(): Readonly<OilFieldStoreState> {
  return defaults;
}

export function initializeOilFieldStore(query: LocationQuery) {
  if (hasMatchingQueryString(query)) {
    console.log('initializing read-only store from query')
    toggleStorage.setReadOnly(true)
    const store = populateStoreFromQuery(query)
    store.usingQueryString = true;
  } else {
    console.log('initializing store from local storage')
    getStore().usingQueryString = false;
  }
}

export function generateQueryString() {
  const store = useOilFieldStore()
  const pieces = []
  for (const [storeKey, storeValue] of getEntries(store.$state)) {
    if (storeKey == 'usingQueryString' || storeKey == 'useStagingApi') {
      continue
    }

    const queryKey = storeToQuery[storeKey]
    if (queryKey) {
      let queryValue = storeValue;
      if (typeof queryValue == 'boolean') {
        queryValue = queryValue ? '1' : '0'
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