import { defineStore, Store } from "pinia";

const defaults = {
  autoPlan: false
};

export type AutoPlanStoreState = typeof defaults

type AutoPlanStore = Store<"AutoPlanStore", AutoPlanStoreState, {}, {}>

export function useAutoPlanStore(): AutoPlanStore {
  return defineStore('AutoPlanStore', {
    state: () => Object.assign({}, defaults),
    persist: true
  })();
}
