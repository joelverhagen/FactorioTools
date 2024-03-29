<template>
  <fieldset class="border p-3 mb-3">
    <legend>Electric poles</legend>
    <div class="form-check">
      <input type="checkbox" class="form-check-input" id="add-electric-poles" v-model="addElectricPoles">
      <label class="form-check-label" for="add-electric-poles">Add electric poles</label>
    </div>
    <CustomizeSelect v-if="addElectricPoles" custom-label="Entity" label="Entity" idPrefix="electric-pole"
      :showAdvancedOptions="showAdvancedOptions" :defaultValue="electricPoleEntityName"
      :defaultIsCustom="electricPoleIsCustom" v-model="electricPoleEntityName" v-model:isCustom="electricPoleIsCustom">
      <option value="small-electric-pole">Small electric pole</option>
      <option value="medium-electric-pole">Medium electric pole</option>
      <option value="big-electric-pole">Big electric pole</option>
      <option value="substation">Substation</option>
    </CustomizeSelect>
    <div class="row" v-show="showAdvancedOptions && addElectricPoles">
      <div class="col-lg-4 mt-3">
        <label class="form-label" for="electric-pole-wire-reach">Wire reach</label>
        <input type="text" pattern="\d+(\.\d+)" min="1" max="99" class="form-control" id="electric-pole-wire-reach"
          v-model="electricPoleWireReach" :disabled="!electricPoleIsCustom" required autocomplete="off">
      </div>
      <div class="col-lg-4 mt-3">
        <p class="form-label">Entity size (<label for="electric-pole-width">width</label> x <label
            for="electric-pole-height">height</label>)</p>
        <div class="input-group">
          <input type="number" min="0" max="9" class="form-control" id="electric-pole-width" v-model="electricPoleWidth"
            :disabled="!electricPoleIsCustom" required>
          <span class="input-group-text">x</span>
          <input type="number" min="1" max="9" class="form-control" id="electric-pole-height" v-model="electricPoleHeight"
            :disabled="!electricPoleIsCustom" required>
        </div>
      </div>
      <div class="col-lg-4 mt-3">
        <p class="form-label">Supply area (<label for="electric-pole-supply-width">width</label> x <label
            for="electric-pole-supply-height">height</label>)</p>
        <div class="input-group">
          <input type="number" min="1" max="99" class="form-control" id="electric-pole-supply-width"
            v-model="electricPoleSupplyWidth" :disabled="!electricPoleIsCustom" required>
          <span class="input-group-text">x</span>
          <input type="number" min="1" max="99" class="form-control" id="electric-pole-supply-height"
            v-model="electricPoleSupplyHeight" :disabled="!electricPoleIsCustom" required>
        </div>
      </div>
    </div>
  </fieldset>
</template>
  
<script lang="ts">
import { storeToRefs } from 'pinia';
import { pick } from '../lib/helpers';
import { getDefaults, useOilFieldStore } from '../stores/OilFieldStore';
import CustomizeSelect from './CustomizeSelect.vue';

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
      'addElectricPoles',
      'electricPoleEntityName',
      'electricPoleIsCustom',
      'electricPoleWidth',
      'electricPoleHeight',
      'electricPoleSupplyWidth',
      'electricPoleSupplyHeight',
      'electricPoleWireReach');
  },
  watch: {
    showAdvancedOptions: function (newVal: boolean) {
      if (!newVal) {
        this.reset()
      }
    },
    electricPoleEntityName: function (newVal: string) {
      this.setKnownElectricPole(newVal)
    }
  },
  methods: {
    reset() {
      if (!this.setKnownElectricPole(this.electricPoleEntityName)) {
        const defaults = getDefaults()
        this.electricPoleEntityName = defaults.electricPoleEntityName
      }
    },
    setKnownElectricPole(electricPoleEntityName: string) {
      switch (electricPoleEntityName) {
        case 'small-electric-pole':
          this.electricPoleWidth = 1
          this.electricPoleHeight = 1
          this.electricPoleSupplyWidth = 5
          this.electricPoleSupplyHeight = 5
          this.electricPoleWireReach = 7.5
          return true
        case 'medium-electric-pole':
          this.electricPoleWidth = 1
          this.electricPoleHeight = 1
          this.electricPoleSupplyWidth = 7
          this.electricPoleSupplyHeight = 7
          this.electricPoleWireReach = 9
          return true
        case 'big-electric-pole':
          this.electricPoleWidth = 2
          this.electricPoleHeight = 2
          this.electricPoleSupplyWidth = 4
          this.electricPoleSupplyHeight = 4
          this.electricPoleWireReach = 30
          return true
        case 'substation':
          this.electricPoleWidth = 2
          this.electricPoleHeight = 2
          this.electricPoleSupplyWidth = 18
          this.electricPoleSupplyHeight = 18
          this.electricPoleWireReach = 18
          return true
        default:
          return false
      }
    }
  },
  components: { CustomizeSelect }
}
</script>