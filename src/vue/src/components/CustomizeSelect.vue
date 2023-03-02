<template>
  <div class="row">
    <div :class="showAdvancedOptions ? 'col-lg-6' : ''">
      <label :for="idPrefix + '-select'" class="form-label">{{ label }}</label>
      <select class="form-select" :id="idPrefix + '-select'" v-model="selectValue" ref="selectEl" required>
        <slot></slot>
        <option value="custom" v-show="showAdvancedOptions">Custom</option>
      </select>
    </div>
    <div class="col-lg-6 mt-3 mt-lg-0" v-show="showAdvancedOptions">
      <label :for="idPrefix + '-custom'" class="form-label">{{ customLabel }} internal name (<a href="#" role="button"
          @click.prevent="customize">customize</a>)</label>
      <input type="text" class="form-control" :id="idPrefix + '-custom'" ref="customEl"
        :disabled="selectValue != 'custom'" v-model="customValue" required autocomplete="off">
    </div>
  </div>
</template>

<script lang="ts">
export default {
  props: {
    showAdvancedOptions: {
      type: Boolean,
      required: true
    },
    label: {
      type: String,
      required: true
    },
    defaultValue: {
      type: String,
      default: ''
    },
    defaultIsCustom: {
      type: Boolean,
      required: true
    },
    customLabel: {
      type: String,
      required: true
    },
    idPrefix: {
      type: String,
      default(rawProps: { label: String }) {
        var idPrefix = rawProps.label.replaceAll(' ', '-').toLowerCase();
        return idPrefix;
      }
    }
  },
  mounted() {
    const options = this.getSelectEl().options;
    for (let i = 0; i < options.length - 1; i++) {
      this.allowedValues.push(options[i].value)
    }
  },
  watch: {
    showAdvancedOptions: function (newVal: boolean) {
      if (!newVal && this.selectValue == 'custom') {
        this.selectValue = this.allowedValues.includes(this.defaultValue) ? this.defaultValue : this.allowedValues[0]
      }
    },
    defaultValue: function (newVal: string) {
      if (newVal != this.customValue) {
        this.selectValue = newVal
      }
    },
    selectValue: function (newVal: string) {
      const isCustom = this.selectValue == 'custom';
      this.$emit('update:isCustom', isCustom)
      this.customValue = isCustom ? '' : newVal
    },
    customValue: function (newVal: string) {
      this.$emit('update:modelValue', newVal)
    }
  },
  data() {
    return {
      allowedValues: new Array<string>(),
      selectValue: this.defaultIsCustom ? 'custom' : this.defaultValue,
      customValue: this.defaultValue,
    }
  },
  methods: {
    customize() {
      if (this.selectValue != 'custom') {
        this.selectValue = 'custom'
        this.customValue = '';
      }
      this.focusCustom()
    },
    getSelectEl(): HTMLSelectElement {
      return this.$refs.selectEl as HTMLSelectElement
    },
    getCustomEl(): HTMLInputElement {
      return this.$refs.customEl as HTMLInputElement
    },
    focusCustom() {
      this.$nextTick(() => this.getCustomEl().focus())
    }
  }
}
</script>
