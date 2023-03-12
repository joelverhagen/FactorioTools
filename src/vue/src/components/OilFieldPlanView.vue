<template>
  <div class="border p-3 mt-3">
    <div class="row row-cols-lg-auto g-2 mb-3">
      <div class="col-12">
        <CopyButton class="btn btn-info" :value="plan.data.blueprint">Copy blueprint</CopyButton>
      </div>
      <div class="col-12">
        <a class="btn btn-link" :href="fbeUrl" target="_blank" rel="noopener noreferrer">View in FBE</a>
      </div>
    </div>
    <template v-if="allPlans.length > 0">
      <table class="table">
        <thead>
          <tr>
            <th scope="col">Rank</th>
            <th scope="col">Plan</th>
            <th v-if="plan.data.request.addBeacons" scope="col">Beacon effect count ⏬</th>
            <th v-if="plan.data.request.addBeacons" scope="col">Beacon count ⏫</th>
            <th scope="col">Pipe count ⏫</th>
          </tr>
        </thead>
        <tbody class="table-group-divider">
          <tr v-for="p in allPlans" :class="p.selected ? 'table-primary' : ''">
            <th scope="row">{{ p.rank }}</th>
            <td>
              <template v-for="(s, i) of p.steps">
                <span v-if="i > 0">➡️</span>
                <span class="badge" :class="s.class">{{ s.text }} </span>
              </template>
            </td>
            <td v-if="plan.data.request.addBeacons">{{ p.beaconEffectCount }}</td>
            <td v-if="plan.data.request.addBeacons">{{ p.beaconCount }}</td>
            <td>{{ p.pipeCount }}</td>
          </tr>
        </tbody>
      </table>
    </template>
    <h6>Oil field blueprint</h6>
    <code>{{ plan.data.blueprint }}</code>
  </div>
</template>
    
<script lang="ts">
import clipboard from 'clipboardy';
import { PropType } from 'vue';
import { BeaconStrategy, OilFieldPlan, PipeStrategy } from '../lib/FactorioToolsApi';
import { OilFieldPlanResult } from '../lib/OilFieldPlanner';
import CopyButton from './CopyButton.vue';

interface SelectedOilFieldPlan extends OilFieldPlan {
  selected: boolean,
  rank: number,
  steps: Step[],
}

interface Step {
  readonly text: string,
  readonly class: string,
}

const optimizeStep: Step = { text: "optimize", class: "text-bg-secondary" }

function getPipeStep(pipeStrategy: PipeStrategy): Step {
  switch (pipeStrategy) {
    case PipeStrategy.Fbe:
      return { text: "FBE", class: "text-bg-fbe" };
    case PipeStrategy.ConnectedCentersDelaunay:
      return { text: "CC-DT", class: "text-bg-primary" };
    case PipeStrategy.ConnectedCentersDelaunayMst:
      return { text: "CC-DT-MST", class: "text-bg-primary" };
    case PipeStrategy.ConnectedCentersFlute:
      return { text: "CC-FLUTE", class: "text-bg-primary" };
    default:
      throw new Error(`unrecognized pipe strategy ${pipeStrategy}`)
  }
}

function getBeaconStep(beaconStrategy: BeaconStrategy): Step {
  switch (beaconStrategy) {
    case BeaconStrategy.FbeOriginal:
      return { text: "FBE", class: "text-bg-fbe" };
    case BeaconStrategy.Fbe:
      return { text: "FBE*", class: "text-bg-fbe" };
    case BeaconStrategy.Snug:
      return { text: "snug", class: "text-bg-primary" };
    default:
      throw new Error(`unrecognized beacon strategy ${beaconStrategy}`)
  }
}

export default {
  props: {
    plan: {
      type: Object as PropType<OilFieldPlanResult>,
      required: true
    }
  },
  computed: {
    fbeUrl() {
      return `https://fbe.teoxoy.com/?source=${this.plan.data.blueprint}`;
    },
    allPlans(): SelectedOilFieldPlan[] {
      const allPlans = []
      allPlans.push(...this.plan.data.summary.selectedPlans.map(p => ({ ...p, selected: true, rank: 1, steps: [] as Step[] })))
      allPlans.push(...this.plan.data.summary.unusedPlans.map(p => ({ ...p, selected: false, rank: 1, steps: [] as Step[] })))

      allPlans.sort((a, b) => {
        let c = b.beaconEffectCount - a.beaconEffectCount
        if (c != 0) { return c }

        c = a.beaconCount - b.beaconCount
        if (c != 0) { return c }

        c = a.pipeCount - b.pipeCount
        if (c != 0) { return c }

        c = a.pipeStrategy.localeCompare(b.pipeStrategy)
        if (c != 0) { return c }

        if (a.optimizePipes != b.optimizePipes) { return a.optimizePipes ? -1 : 1 }

        return (a.beaconStrategy ?? "").localeCompare(b.pipeStrategy ?? "")
      })

      let rank = 1;
      for (let i = 0; i < allPlans.length; i++) {
        const c = allPlans[i]
        if (i > 0) {
          const p = allPlans[i - 1]
          if (c.beaconEffectCount != p.beaconEffectCount
            || c.beaconCount != p.beaconCount
            || c.pipeCount != p.pipeCount) {
            rank++;
          }
        }
        c.rank = rank;

        c.steps.push(getPipeStep(c.pipeStrategy))

        if (c.optimizePipes) {
          c.steps.push(optimizeStep)
        }
        if (c.beaconStrategy) {
          c.steps.push(getBeaconStep(c.beaconStrategy))
        }
      }

      return allPlans;
    }
  },
  methods: {
    async copyBlueprint() {
      await clipboard.write(this.plan.data.blueprint);
    }
  },
  components: { CopyButton }
}
</script>

<style lang="css">
.text-bg-fbe {
  color: #FE7520;
  background-color: #303030
}
</style>