<template>
  <div class="row">
    <div :class="[showAdvancedOptions ? 'col-lg-6' : '']">
      <label :for="idPrefix + '-select'" class="form-label">{{ label }}</label>
      <select class="form-select" :id="idPrefix + '-select'" v-model="selectValue" ref="selectEl"
        @change="onSelectChange" required>
        <slot></slot>
        <option value="custom" v-show="showAdvancedOptions">Custom</option>
      </select>
    </div>
    <div class="col-lg-6 mt-3 mt-lg-0" v-show="showAdvancedOptions">
      <label :for="idPrefix + '-custom'" class="form-label">{{ customLabel }} internal name (<a href="#" role="button"
          @click.prevent="customize">customize</a>)</label>
      <input type="text" class="form-control" :id="idPrefix + '-custom'" ref="customEl"
        :disabled="selectValue != 'custom'" v-model="customValue" required>
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
  watch: {
    showAdvancedOptions: function (newVal: boolean) {
      if (!newVal && this.selectValue == 'custom') {
        this.selectValue = this.defaultValue
        this.customValue = this.defaultValue
      }
    },
    customValue: function (newVal: string) {
      this.$emit('update:modelValue', newVal)
      this.$emit('update:isCustom', this.selectValue == 'custom')
    }
  },
  data() {
    console.log("defaultValue: " + this.defaultValue)
    return {
      mapping: new Map<string, string>(),
      selectValue: this.defaultValue,
      customValue: ''
    }
  },
  mounted() {
    this.mapping.clear()
    const options = this.getSelectEl().options;
    for (let i = 0; i < options.length; i++) {
      this.mapping.set(options[i].value, options[i].textContent ?? "")
    }
    // this.customValue = this.selectValue
  },
  methods: {
    customize() {
      if (this.selectValue != 'custom') {
        this.selectValue = 'custom'
        this.customValue = '';
      }
      this.focusCustom()
    },
    getCustomEl(): HTMLInputElement {
      return this.$refs.customEl as HTMLInputElement
    },
    getSelectEl(): HTMLSelectElement {
      return this.$refs.selectEl as HTMLSelectElement
    },
    onSelectChange(event: Event) {
      const target = event.target as HTMLSelectElement;
      if (target.value != 'custom') {
        this.customValue = target.value;
      } else {
        this.customValue = '';
      }
    },
    focusCustom() {
      this.$nextTick(() => this.getCustomEl().focus())
    }
  }
}
</script>
