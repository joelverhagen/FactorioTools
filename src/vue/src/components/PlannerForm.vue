<template>
  <fieldset class="border p-3 mb-3">
    <legend>Planner options</legend>
    <div class="form-check">
      <input type="checkbox" class="form-check-input" id="use-underground-pipes" v-model="useUndergroundPipes">
      <label class="form-check-label" for="use-underground-pipes">Use underground pipes</label>
    </div>
    <div class="form-check">
      <input type="checkbox" class="form-check-input" id="optimize-pipes" v-model="optimizePipes">
      <label class="form-check-label" for="optimize-pipes">Optimize pipes</label>
    </div>
    <div class="form-check">
      <input type="checkbox" class="form-check-input" id="validate-solution" v-model="validateSolution">
      <label class="form-check-label" for="validate-solution">Validate solution 🐛</label> (slower but checks for
      problems in the resulting blueprint)
    </div>
    <div class="form-check">
      <input type="checkbox" class="form-check-input" id="use-staging-api" v-model="useStagingApi">
      <label class="form-check-label" for="use-staging-api">Use staging API 🤠</label>
    </div>
    <div class="form-check">
      <input type="checkbox" class="form-check-input" id="auto-plan" v-model="autoPlan">
      <label class="form-check-label" for="auto-plan">Start planning on page load</label> (useful for share URLs)
    </div>
    <fieldset class="border p-3 mt-3">
      <legend>Pipe strategies</legend>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="pipes-fbe-original" v-model="pipeStrategyFbeOriginal">
        <label class="form-check-label" for="pipes-fbe-original">Teoxoy's FBE</label> (<a
          href="https://github.com/teoxoy/factorio-blueprint-editor/blob/0bec144b8989422f86bce8cea58ef49258c1a88d/packages/editor/src/core/generators/pipe.ts">original
          source</a>) without modifications
      </div>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="pipes-fbe" v-model="pipeStrategyFbe">
        <label class="form-check-label" for="pipes-fbe">Teoxoy's FBE</label> with modifications
      </div>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="pipes-connected-centers-delaunay"
          v-model="pipeStrategyConnectedCentersDelaunay">
        <label class="form-check-label" for="pipes-connected-centers-delaunay">Connected centers via Delaunay
          triangulation</label>
      </div>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="pipes-connected-centers-delaunay-mst"
          v-model="pipeStrategyConnectedCentersDelaunayMst">
        <label class="form-check-label" for="pipes-connected-centers-delaunay-mst">Connected centers via Delaunay
          triangulation and Prim's MST</label>
      </div>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="pipes-connected-centers-flute"
          v-model="pipeStrategyConnectedCentersFlute">
        <label class="form-check-label" for="pipes-connected-centers-flute">Connected centers via <a
            href="https://home.engineering.iastate.edu/~cnchu/flute.html">FLUTE</a></label>
      </div>
    </fieldset>
    <fieldset class="border p-3 mt-3" :disabled="!addBeacons">
      <legend>
        Beacon strategies
        <span v-if="!addBeacons" class="badge bg-secondary">disabled</span>
      </legend>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="beacons-fbe-original" v-model="beaconStrategyFbeOriginal">
        <label class="form-check-label" for="beacons-fbe-original">Teoxoy's FBE</label> (<a
          href="https://github.com/teoxoy/factorio-blueprint-editor/blob/0bec144b8989422f86bce8cea58ef49258c1a88d/packages/editor/src/core/generators/beacon.ts">original
          source</a>) without modifications
      </div>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="beacons-fbe" v-model="beaconStrategyFbe">
        <label class="form-check-label" for="beacons-fbe">Teoxoy's FBE</label> with modifications
      </div>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="beacons-snug" v-model="beaconStrategySnug">
        <label class="form-check-label" for="beacons-snug">Snug</label>
      </div>
    </fieldset>
  </fieldset>
</template>


<script lang="ts">
import { storeToRefs } from 'pinia';
import { pick } from '../lib/helpers';
import { useAutoPlanStore } from '../stores/AutoPlanStore';
import { getDefaults, useOilFieldStore } from '../stores/OilFieldStore';

export default {
  props: {
    showAdvancedOptions: {
      type: Boolean,
      required: true
    }
  },
  data() {
    return Object.assign(
      storeToRefs(useAutoPlanStore()),
      pick(storeToRefs(useOilFieldStore()),
        'addBeacons',
        'useUndergroundPipes',
        'useStagingApi',
        'optimizePipes',
        'validateSolution',
        'pipeStrategyFbeOriginal',
        'pipeStrategyFbe',
        'pipeStrategyConnectedCentersDelaunay',
        'pipeStrategyConnectedCentersDelaunayMst',
        'pipeStrategyConnectedCentersFlute',
        'beaconStrategyFbeOriginal',
        'beaconStrategyFbe',
        'beaconStrategySnug'));
  },
  watch: {
    showAdvancedOptions: function (newVal: boolean) {
      if (!newVal) {
        this.reset()
      }
    }
  },
  methods: {
    reset() {
      const defaults = getDefaults()
      this.useUndergroundPipes = defaults.useUndergroundPipes
      this.useStagingApi = defaults.useStagingApi
      this.optimizePipes = defaults.optimizePipes
      this.validateSolution = defaults.validateSolution
      this.pipeStrategyFbeOriginal = defaults.pipeStrategyFbeOriginal
      this.pipeStrategyFbe = defaults.pipeStrategyFbe
      this.pipeStrategyConnectedCentersDelaunay = defaults.pipeStrategyConnectedCentersDelaunay
      this.pipeStrategyConnectedCentersDelaunayMst = defaults.pipeStrategyConnectedCentersDelaunayMst
      this.pipeStrategyConnectedCentersFlute = defaults.pipeStrategyConnectedCentersFlute
      this.beaconStrategyFbeOriginal = defaults.beaconStrategyFbeOriginal
      this.beaconStrategyFbe = defaults.beaconStrategyFbe
      this.beaconStrategySnug = defaults.beaconStrategySnug
    }
  }
}
</script>