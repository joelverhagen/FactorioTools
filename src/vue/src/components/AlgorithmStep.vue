<template>
  <span v-if="showAsOption">Use </span>
  <span class="badge" :title="shortDescription" :class="class">{{ text }}</span>
  <span v-if="showAsOption"> ({{ shortDescription }})</span>
</template>

<script lang="ts">
export default {
  props: {
    class: {
      type: String,
      required: true,
    },
    text: {
      type: String,
      required: true,
    },
    shortDescription: {
      type: String,
      required: true,
    },
    longDescription: {
      type: String,
      required: true,
    },
    type: {
      type: String,
    },
    showAsOption: {
      type: Boolean,
      default: false,
    },
  }
}
</script>
