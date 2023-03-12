<template>
  <h1 class="row gx-2">
    <div class="col-md-auto">
      Oil field planner
      <span v-if="useAdvancedOptions">🤓</span>
      <span v-else>🐣</span>
    </div>
    <div class="col-md-auto">
      <small v-if="usingQueryString"
        class="px-2 text-secondary bg-secondary bg-opacity-10 border border-secondary border-opacity-10 rounded-2">
        from share URL
      </small>
    </div>
  </h1>
  <p>This tool finds a near optimal oil field layout given a blueprint containing pumpjacks.</p>

  <form>
    <div class="row row-cols-md-auto g-2 mb-3">
      <div class="col-12">
        <button type="button" class="btn btn-info" @click="toggleAdvancedOptions">
          Use {{ useAdvancedOptions ? "simple" : "advanced" }} options
        </button>
      </div>
      <div class="col-12">
        <CopyButton class="btn btn-info" :value="shareUrl">Copy share URL</CopyButton>
      </div>
      <div class="col-12">
        <button type="button" class="btn btn-warning" @click="reset">Reset</button>
      </div>
      <div class="col-12">
        <button type="button" class="btn btn-warning" v-if="usingQueryString" @click="overrideLocalSettings">
          Override local settings
        </button>
      </div>
    </div>
    <fieldset class="border p-3 mb-3">
      <label for="input-blueprint" class="form-label fs-4">Input blueprint</label>
      <a class="btn btn-link btn-sm ms-1s mb-2" :href="fbeUrl" target="_blank" rel="noopener noreferrer">View in FBE</a>
      <button type="button" class="btn btn-warning btn-sm mb-2" @click="addSampleBlueprint">Add sample</button>
      <textarea class="form-control font-monospace" id="input-blueprint" aria-describedby="input-blueprint-help"
        placeholder="paste blueprint string here" rows="3" v-model="inputBlueprint" spellcheck="false"
        autocomplete="off"></textarea>
    </fieldset>
    <PumpjacksForm :show-advanced-options="useAdvancedOptions" />
    <BeaconForm :show-advanced-options="useAdvancedOptions" />
    <ElectricPoleSelect :show-advanced-options="useAdvancedOptions" />
    <PlannerForm :show-advanced-options="useAdvancedOptions" v-show="useAdvancedOptions" />
    <div class="d-grid gap-2">
      <button type="submit" class="btn btn-primary btn-lg" @click.prevent="submit" :disabled="cannotSubmit">
        <span v-if="cannotSubmit" class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
        Plan oil field
      </button>
    </div>
    <OilFieldPlanView v-if="plan" :plan="plan" />
    <ResponseErrorView v-if="error" :error="error" />
  </form>
</template>

<script lang="ts">
import { getPlan, OilFieldPlanResult, ResponseError } from '../lib/OilFieldPlanner'
import BeaconForm from '../components/BeaconForm.vue';
import ElectricPoleSelect from '../components/ElectricPoleForm.vue';
import PlannerForm from '../components/PlannerForm.vue';
import PumpjacksForm from '../components/PumpjacksForm.vue';
import { pick } from '../lib/helpers';
import { storeToRefs } from 'pinia';
import { generateQueryString, initializeOilFieldStore, persistStore, useOilFieldStore } from '../stores/OilFieldStore';
import ResponseErrorView from '../components/ResponseErrorView.vue';
import OilFieldPlanView from '../components/OilFieldPlanView.vue';
import CopyButton from '../components/CopyButton.vue';

const sampleBlueprints = __SAMPLE_BLUEPRINTS__;
sampleBlueprints.sort((a, b) => b.length - a.length)

export default {
  data() {
    return Object.assign({
      copiedAt: new Date(0),
      recentlyCopied: false,
      submitting: false,
      plan: null as null | OilFieldPlanResult,
      error: null as null | ResponseError
    }, pick(
      storeToRefs(useOilFieldStore()),
      'usingQueryString',
      'useAdvancedOptions',
      'inputBlueprint'));
  },
  computed: {
    cannotSubmit() { return !this.inputBlueprint.trim() || this.submitting },
    shareUrl() {
      const queryString = generateQueryString();
      return `${location.protocol}//${location.host}${location.pathname}?${queryString}`
    },
    fbeUrl() {
      return `https://fbe.teoxoy.com/?source=${this.inputBlueprint}`
    }
  },
  methods: {
    toggleAdvancedOptions() {
      this.useAdvancedOptions = !this.useAdvancedOptions;
    },
    addSampleBlueprint() {
      if (!this.inputBlueprint) {
        this.inputBlueprint = sampleBlueprints[0]
      } else {
        const index = Math.floor(Math.random() * sampleBlueprints.length);
        let sampleBlueprint = sampleBlueprints[index]
        if (sampleBlueprint == this.inputBlueprint) {
          sampleBlueprint = sampleBlueprint[(index + 1) % sampleBlueprints.length]
        }
        this.inputBlueprint = sampleBlueprint
      }
    },
    reset() {
      const store = useOilFieldStore()
      const usingQueryString = store.usingQueryString
      store.$reset()
      store.usingQueryString = usingQueryString
    },
    async submit() {
      if (this.cannotSubmit) {
        return
      }

      this.submitting = true;
      const planOrError = await getPlan()
      if (planOrError.isError) {
        this.plan = null
        this.error = planOrError
      } else {
        this.plan = planOrError
        this.error = null
      }

      this.submitting = false
    },
    async overrideLocalSettings() {
      persistStore()
      await this.$router.replace({ query: {} });
      initializeOilFieldStore(this.$route.query)
    }
  },
  components: { ElectricPoleSelect, BeaconForm, PumpjacksForm, PlannerForm, ResponseErrorView, OilFieldPlanView, CopyButton }
}
</script>