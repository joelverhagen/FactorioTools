<template>
  <h1>Oil field generator</h1>
  <p>This tool finds a near optimal oil field layout given a blueprint containing pumpjacks.</p>

  <form>
    <div class="mb-3">
      <button type="button" class="btn btn-secondary btn-sm" @click="toggleAdvancedOptions">
        Show {{ showAdvancedOptions ? "simple" : "advanced" }} options
      </button> 
      <button type="button" class="btn btn-warning btn-sm ms-1" @click="reset">
        Reset
      </button>
    </div>
    <div class="mb-3">
      <label for="input-blueprint" class="form-label">Input blueprint</label>
      <textarea class="form-control font-monospace" id="input-blueprint" aria-describedby="input-blueprint-help"
        placeholder="paste blueprint string here" rows="3" v-model="inputBlueprint" spellcheck="false"></textarea>
    </div>
    <PumpjacksForm :show-advanced-options="showAdvancedOptions" />
    <BeaconForm :show-advanced-options="showAdvancedOptions" />
    <ElectricPoleSelect :show-advanced-options="showAdvancedOptions" />
    <PlannerForm :show-advanced-options="showAdvancedOptions" v-show="showAdvancedOptions" />
    <div>
      <button type="submit" class="btn btn-primary btn-sm" @click.prevent="submit">Submit</button>
    </div>
  </form>
</template>

<script lang="ts">
import BeaconForm from '../components/BeaconForm.vue';
import CustomizeSelect from '../components/CustomizeSelect.vue';
import ElectricPoleSelect from '../components/ElectricPoleForm.vue';
import ModuleSelect from '../components/ModuleSelect.vue';
import PlannerForm from '../components/PlannerForm.vue';
import PumpjacksForm from '../components/PumpjacksForm.vue';
import { pick } from '../lib/helpers';
import { storeToRefs } from 'pinia';
import { useOilFieldStore } from '../stores/OilFieldStore';

export default {
  data() {
    return pick(
      storeToRefs(useOilFieldStore()),
      'showAdvancedOptions',
      'inputBlueprint');
  },
  methods: {
    toggleAdvancedOptions() {
      this.showAdvancedOptions = !this.showAdvancedOptions;
    },
    reset() {
      useOilFieldStore().$reset()
    },
    submit() {
    }
  },
  components: { ModuleSelect, CustomizeSelect, ElectricPoleSelect, BeaconForm, PumpjacksForm, PlannerForm }
}
</script>