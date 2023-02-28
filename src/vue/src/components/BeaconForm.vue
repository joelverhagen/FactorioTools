<template>
  <fieldset class="border p-3 mb-3">
    <legend>Beacons</legend>
    <div class="form-check">
      <input type="checkbox" class="form-check-input" id="add-beacons" v-model="addBeacons">
      <label class="form-check-label" for="add-beacons">Add beacons</label>
    </div>
    <ModuleSelect label="Beacon module" :show-advanced-options="showAdvancedOptions" :default-value="beaconModule"
      v-if="addBeacons" class="mt-3" v-model="beaconModule" />
    <div class="row" v-show="showAdvancedOptions && addBeacons">
      <div class="col-lg-4 mt-3">
        <label for="beacon-entity-name" class="form-label">Entity internal name (<a href="#"
            @click.prevent="reset">reset</a>)</label>
        <input type="text" class="form-control" id="beacon-entity-name" v-model="beaconEntityName">
      </div>
      <div class="col-lg-4 mt-3">
        <p class="form-label">Entity size (<label for="beacon-width">width</label> x <label
            for="beacon-height">height</label>)</p>
        <div class="input-group">
          <input type="number" min="0" max="9" class="form-control" id="beacon-width" v-model="beaconWidth" required>
          <span class="input-group-text">x</span>
          <input type="number" min="1" max="9" class="form-control" id="beacon-height" v-model="beaconHeight" required>
        </div>
      </div>
      <div class="col-lg-4 mt-3">
        <p class="form-label">Supply area (<label for="beacon-supply-width">width</label> x <label
            for="beacon-supply-height">height</label>)</p>
        <div class="input-group">
          <input type="number" min="1" max="99" class="form-control" id="beacon-supply-width" v-model="beaconSupplyWidth"
            required>
          <span class="input-group-text">x</span>
          <input type="number" min="1" max="99" class="form-control" id="beacon-supply-height"
            v-model="beaconSupplyHeight" required>
        </div>
      </div>
    </div>
  </fieldset>
</template>

<script lang="ts">
import { storeToRefs } from 'pinia';
import { pick } from '../lib/helpers';
import { useOilFieldStore } from '../stores/OilFieldStore';
import ModuleSelect from './ModuleSelect.vue';

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
      'addBeacons',
      'beaconModule',
      'beaconEntityName',
      'beaconSupplyWidth',
      'beaconSupplyHeight',
      'beaconWidth',
      'beaconHeight');
  },
  mounted() {
    this.reset()
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
      this.beaconEntityName = 'beacon'
      this.beaconWidth = 3;
      this.beaconHeight = 3;
      this.beaconSupplyWidth = 9;
      this.beaconSupplyHeight = 9;
    }
  },
  components: { ModuleSelect }
}
</script>