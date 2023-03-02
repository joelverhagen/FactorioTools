<template>
  <h1>
    Oil field planner
    <small v-if="usingQueryString"
      class="px-2 text-secondary bg-secondary bg-opacity-10 border border-secondary border-opacity-10 rounded-2">
      from share URL
    </small>
  </h1>
  <p>This tool finds a near optimal oil field layout given a blueprint containing pumpjacks.</p>

  <form>
    <div class="row row-cols-lg-auto g-2 mb-3">
      <div class="col-12">
        <button type="button" class="btn btn-secondary" @click="toggleAdvancedOptions">
          Use {{ useAdvancedOptions ? "simple" : "advanced" }} options
        </button>
      </div>
      <div class="col-12">
        <button type="button" class="btn btn-warning" @click="reset">Reset</button>
      </div>
      <div class="col-12">
        <button type="button" class="btn btn-info" @click="copyShareUrl">
          Copy share URL {{ recentlyCopied ? '☑️' : '' }}
        </button>
      </div>
      <div class="col-12">
        <button type="button" class="btn btn-warning" v-if="usingQueryString" @click="overrideLocalSettings">
          Override local settings
        </button>
      </div>
    </div>
    <div class="mb-3">
      <label for="input-blueprint" class="form-label">Input blueprint</label>
      <textarea class="form-control font-monospace" id="input-blueprint" aria-describedby="input-blueprint-help"
        placeholder="paste blueprint string here" rows="3" v-model="inputBlueprint" spellcheck="false" autocomplete="off"></textarea>
    </div>
    <PumpjacksForm :show-advanced-options="useAdvancedOptions" />
    <BeaconForm :show-advanced-options="useAdvancedOptions" />
    <ElectricPoleSelect :show-advanced-options="useAdvancedOptions" />
    <PlannerForm :show-advanced-options="useAdvancedOptions" v-show="useAdvancedOptions" />
    <div class="row row-cols-lg-auto g-2">
      <div class="col-12">
        <button type="submit" class="btn btn-primary" @click.prevent="submit">Submit</button>
      </div>
    </div>
  </form>
</template>

<script lang="ts">
import BeaconForm from '../components/BeaconForm.vue';
import clipboard from 'clipboardy';
import ElectricPoleSelect from '../components/ElectricPoleForm.vue';
import PlannerForm from '../components/PlannerForm.vue';
import PumpjacksForm from '../components/PumpjacksForm.vue';
import { pick } from '../lib/helpers';
import { storeToRefs } from 'pinia';
import { generateQueryString, initializeOilFieldStore, persistStore, useOilFieldStore } from '../stores/OilFieldStore';

const recentlyCopiedMs = 3000;

export default {
  data() {
    return Object.assign({ copiedAt: new Date(0), recentlyCopied: false }, pick(
      storeToRefs(useOilFieldStore()),
      'usingQueryString',
      'useAdvancedOptions',
      'inputBlueprint'));
  },
  methods: {
    toggleAdvancedOptions() {
      this.useAdvancedOptions = !this.useAdvancedOptions;
    },
    reset() {
      const store = useOilFieldStore()
      const usingQueryString = store.usingQueryString
      store.$reset()
      store.usingQueryString = usingQueryString
    },
    submit() {
    },
    async overrideLocalSettings() {
      persistStore()
      await this.$router.replace({ query: {} });
      initializeOilFieldStore(this.$route.query)
    },
    async copyShareUrl() {
      const queryString = generateQueryString();
      const shareUrl = `${location.protocol}//${location.host}${location.pathname}?${queryString}`
      await clipboard.write(shareUrl);
      this.recentlyCopied = true
      this.copiedAt = new Date()
      setTimeout(() => {
        this.recentlyCopied = (new Date().getTime() - this.copiedAt.getTime()) < recentlyCopiedMs
      }, recentlyCopiedMs)
    }
  },
  components: { ElectricPoleSelect, BeaconForm, PumpjacksForm, PlannerForm }
}
</script>