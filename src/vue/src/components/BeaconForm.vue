<template>
  <fieldset class="border p-3 mb-3 rounded">
    <legend>Beacons</legend>
    <div class="form-check">
      <input type="checkbox" class="form-check-input" id="add-beacons" v-model="addBeacons">
      <label class="form-check-label" for="add-beacons">Add beacons</label>
    </div>
    <ModuleSelect v-if="addBeacons" class="mt-3" label="Module" :showAdvancedOptions="showAdvancedOptions"
      :defaultValue="beaconModule" :defaultIsCustom="beaconModuleIsCustom" v-model="beaconModule"
      v-model:isCustom="beaconModuleIsCustom" :showProductivityModules="isCustom" />
    <div class="row" v-show="showAdvancedOptions && addBeacons">
      <div class="col-lg-3 mt-3">
        <label for="beacon-entity-name" class="form-label">Entity internal name (<a href="#"
            @click.prevent="reset">reset</a>)</label>
        <input type="text" class="form-control" id="beacon-entity-name" v-model="beaconEntityName" autocomplete="off">
      </div>
      <div class="col-lg-3 mt-3">
        <label for="beacon-module-slots" class="form-label">Module slots</label>
        <input type="number" min="1" max="99" class="form-control" id="beacon-module-slots" v-model="beaconModuleSlots"
          required>
      </div>
      <div class="col-lg-3 mt-3">
        <p class="form-label">Entity size (<label for="beacon-width">width</label> x <label
            for="beacon-height">height</label>)</p>
        <div class="input-group">
          <input type="number" min="0" max="9" class="form-control" id="beacon-width" v-model="beaconWidth" required>
          <span class="input-group-text">x</span>
          <input type="number" min="1" max="9" class="form-control" id="beacon-height" v-model="beaconHeight" required>
        </div>
      </div>
      <div class="col-lg-3 mt-3">
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
import { getDefaults, useOilFieldStore } from '../stores/OilFieldStore';
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
      'beaconModuleIsCustom',
      'beaconModuleSlots',
      'beaconEntityName',
      'beaconSupplyWidth',
      'beaconSupplyHeight',
      'beaconWidth',
      'beaconHeight');
  },
  watch: {
    showAdvancedOptions: function (newVal: boolean) {
      if (!newVal) {
        this.reset()
      }
    }
  },
  computed: {
    isCustom() {
      return this.beaconEntityName != 'beacon'
    }
  },
  methods: {
    reset() {
      const defaults = getDefaults()
      this.beaconEntityName = defaults.beaconEntityName
      this.beaconModuleSlots = defaults.beaconModuleSlots
      this.beaconWidth = defaults.beaconWidth
      this.beaconHeight = defaults.beaconHeight
      this.beaconSupplyWidth = defaults.beaconSupplyWidth
      this.beaconSupplyHeight = defaults.beaconSupplyHeight
    }
  },
  components: { ModuleSelect }
}
</script>