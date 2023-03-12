<template>
  <CustomizeSelect :defaultIsCustom="isCustom" @update:isCustom="newVal => $emit('update:isCustom', newVal)"
    customLabel="Module" :label="label" :showAdvancedOptions="showAdvancedOptions" :defaultValue="defaultValue"
    @update:modelValue="newVal => $emit('update:modelValue', newVal)">
    <option value="">None</option>
    <option value="effectivity-module">Efficiency module</option>
    <option value="effectivity-module-2">Efficiency module 2</option>
    <option value="effectivity-module-3">Efficiency module 3</option>
    <option v-if="showProductivityModules" value="productivity-module">Productivity module</option>
    <option v-if="showProductivityModules" value="productivity-module-2">Productivity module 2</option>
    <option v-if="showProductivityModules" value="productivity-module-3">Productivity module 3</option>
    <option value="speed-module">Speed module</option>
    <option value="speed-module-2">Speed module 2</option>
    <option value="speed-module-3">Speed module 3</option>
  </CustomizeSelect>
</template>

<script lang="ts">
import CustomizeSelect from './CustomizeSelect.vue';

export default {
  props: {
    showAdvancedOptions: {
      type: Boolean,
      required: true
    },
    showProductivityModules: {
      type: Boolean
    },
    label: {
      type: String,
      required: true
    },
    defaultValue: {
      type: String
    },
    isCustom: {
      type: Boolean,
      required: true
    }
  },
  components: { CustomizeSelect }
}
</script>