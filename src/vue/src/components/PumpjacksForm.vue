<template>
  <fieldset class="border p-3 mb-3">
    <legend>Pumpjacks</legend>
    <ModuleSelect label="Pumpjack module" :show-advanced-options="showAdvancedOptions" show-productivity-modules
      :default-value="pumpjackModule" v-model="pumpjackModule" />
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
    return pick(storeToRefs(useOilFieldStore()), 'pumpjackModule');
  },
  components: { ModuleSelect }
}
</script>