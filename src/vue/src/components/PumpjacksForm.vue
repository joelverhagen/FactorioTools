<template>
  <fieldset class="border p-3 mb-3 rounded">
    <legend>Pumpjacks</legend>
    <ModuleSelect label="Module" :showAdvancedOptions="showAdvancedOptions" showProductivityModules
      :defaultValue="pumpjackModule" :defaultIsCustom="pumpjackModuleIsCustom" v-model="pumpjackModule"
      v-model:isCustom="pumpjackModuleIsCustom" />
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
    return pick(storeToRefs(useOilFieldStore()), 'pumpjackModule', 'pumpjackModuleIsCustom');
  },
  components: { ModuleSelect }
}
</script>