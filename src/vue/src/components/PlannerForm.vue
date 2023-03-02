<template>
  <fieldset class="border p-3 mb-3 rounded">
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
      <label class="form-check-label" for="validate-solution">Validate solution</label>
    </div>
    <fieldset class="border p-3 mt-3 rounded">
      <legend>Pipe strategies</legend>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="pipes-fbe" v-model="pipeStrategyFbe">
        <label class="form-check-label" for="pipes-fbe">Teoxoy's FBE</label> (<a
          href="https://github.com/teoxoy/factorio-blueprint-editor/blob/0bec144b8989422f86bce8cea58ef49258c1a88d/packages/editor/src/core/generators/pipe.ts">original
          source</a>)
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
    <fieldset class="border p-3 mt-3 rounded">
      <legend>Beacon strategies</legend>
      <div class="form-check">
        <input type="checkbox" class="form-check-input" id="beacons-fbe" v-model="beaconStrategyFbe">
        <label class="form-check-label" for="beacons-fbe">Teoxoy's FBE</label> (<a
          href="https://github.com/teoxoy/factorio-blueprint-editor/blob/0bec144b8989422f86bce8cea58ef49258c1a88d/packages/editor/src/core/generators/beacon.ts">original
          source</a>) with modifications
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
import { getDefaults, useOilFieldStore } from '../stores/OilFieldStore';

export default {
  props: {
    showAdvancedOptions: {
      type: Boolean,
      required: true
    }
  },
  data() {
    return pick(
      storeToRefs(useOilFieldStore()),
      'useUndergroundPipes',
      'optimizePipes',
      'validateSolution',
      'pipeStrategyFbe',
      'pipeStrategyConnectedCentersDelaunay',
      'pipeStrategyConnectedCentersDelaunayMst',
      'pipeStrategyConnectedCentersFlute',
      'beaconStrategyFbe',
      'beaconStrategySnug');
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
      this.optimizePipes = defaults.optimizePipes
      this.validateSolution = defaults.validateSolution
      this.pipeStrategyFbe = this.pipeStrategyFbe
      this.pipeStrategyConnectedCentersDelaunay = this.pipeStrategyConnectedCentersDelaunay
      this.pipeStrategyConnectedCentersDelaunayMst = this.pipeStrategyConnectedCentersDelaunayMst
      this.pipeStrategyConnectedCentersFlute = this.pipeStrategyConnectedCentersFlute
      this.beaconStrategyFbe = this.beaconStrategyFbe
      this.beaconStrategySnug = this.beaconStrategySnug
    }
  }
}
</script>