<template>
  <button type="button" v-bind="$attrs" @click="copy">
    <slot></slot> {{ recentlyCopied ? '☑️' : '' }}
  </button>
</template>
    
<script lang="ts">
import clipboard from 'clipboardy';

const recentlyCopiedMs = 3000;

export default {
  props: {
    value: {
      type: String,
      required: true
    },
  },
  data() {
    return {
      copiedAt: new Date(0),
      recentlyCopied: false,
    }
  },
  methods: {
    async copy() {
      await clipboard.write(this.value);
      this.recentlyCopied = true
      this.copiedAt = new Date()
      setTimeout(() => {
        this.recentlyCopied = (new Date().getTime() - this.copiedAt.getTime()) < recentlyCopiedMs
      }, recentlyCopiedMs)
    }
  }
}
</script>