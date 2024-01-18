import { BeaconStrategy, PipeStrategy } from "./FactorioToolsApi"

export interface Step {
  readonly text: string,
  readonly class: string,
  readonly shortDescription: string,
  readonly longDescription: string,
  readonly type?: string,
}

function newStep(input: Step): Step {
  return input
}

function newPipeStep(input: Exclude<Step, "prefix">): Step {
  return newStep({
    text: input.text,
    class: input.class,
    shortDescription: input.shortDescription,
    longDescription: input.longDescription,
    type: "pipe strategy"
  })
}

function newBeaconStep(input: Exclude<Step, "prefix">): Step {
  return newStep({
    text: input.text,
    class: input.class,
    shortDescription: input.shortDescription,
    longDescription: input.longDescription,
    type: "beacon strategy"
  })
}

export const Steps = {
  PipeSteps: {
    FbeOriginal: newPipeStep({
      text: "FBE pipe",
      class: "text-bg-fbe",
      shortDescription: "Teoxoy's FBE pipe planner with minimal modifications",
      longDescription: `Teoxoy's pipe planner algorithm used in FBE (<a href="https://fbe.teoxoy.com/">Factorio Blueprint Editor</a>) with minimal modifications for a C# port (<a
      href="https://github.com/teoxoy/factorio-blueprint-editor/blob/0bec144b8989422f86bce8cea58ef49258c1a88d/packages/editor/src/core/generators/pipe.ts">original source</a>)`,
    }),
    Fbe: newPipeStep({
      text: "FBE pipe*",
      class: "text-bg-fbe",
      shortDescription: "Teoxoy's FBE pipe planner with some modifications",
      longDescription: `Teoxoy's pipe planner algorithm used in FBE (<a href="https://fbe.teoxoy.com/">Factorio Blueprint Editor</a>) with modifications (e.g. A*, more turns allowed, runtime improvements)`,
    }),
    ConnectedCentersDelaunay: newPipeStep({
      text: "CC-DT",
      class: "text-bg-primary",
      shortDescription: "Joel's Connected Centers pipe planner, via Delaunay triangulation",
      longDescription: "Joel's Connected Centers pipe planner algorithm using Delaunay triangulation to find adjacent pumpjacks",
    }),
    ConnectedCentersDelaunayMst: newPipeStep({
      text: "CC-DT-MST",
      class: "text-bg-primary",
      shortDescription: "Joel's Connected Centers pipe planner, via Delaunay triangulation and MST",
      longDescription: "Joel's Connected Centers pipe planner algorithm using Delaunay triangulation and Prim's minimum spanning tree to find adjacent pumpjacks",
    }),
    ConnectedCentersFlute: newPipeStep({
      text: "CC-FLUTE",
      class: "text-bg-primary",
      shortDescription: "Joel's Connected Centers pipe planner, via FLUTE",
      longDescription: `Joel's Connected Centers pipe planner algorithm using <a href="https://home.engineering.iastate.edu/~cnchu/flute.html">FLUTE</a> to find adjacent pumpjacks`,
    }),
  },

  OptimizeStep: newStep({
    text: "optimize",
    class: "text-bg-secondary",
    shortDescription: "Joel's pipe plan optimizer to improve a given pipe plan",
    longDescription: "Joel's pipe plan optimizer, which rotates pumpjacks and straightens paths to further improve a given pipe plan",
  }),

  UndergroundPipesStep: newStep({
    text: "bury",
    class: "text-bg-danger",
    shortDescription: "Joel's underground pipe planner",
    longDescription: "Joel's underground pipe planner algorithm, which converts stretches of pipes to underground pipes after planning the normal (overground) pipes",
  }),

  BeaconSteps: {
    FbeOriginal: newBeaconStep({
      text: "FBE beacon",
      class: "text-bg-fbe",
      shortDescription: "Teoxoy's FBE beacon planner with minimal modifications",
      longDescription: `Teoxoy's beacon planner algorithm used in FBE (<a href="https://fbe.teoxoy.com/">Factorio Blueprint Editor</a>) with minimal modifications for a C# port (<a
        href="https://github.com/teoxoy/factorio-blueprint-editor/blob/0bec144b8989422f86bce8cea58ef49258c1a88d/packages/editor/src/core/generators/beacon.ts">original source</a>)`,
    }),
    Fbe: newBeaconStep({
      text: "FBE beacon*",
      class: "text-bg-fbe",
      shortDescription: "Teoxoy's FBE beacon planner with modifications",
      longDescription: `Teoxoy's beacon planner algorithm used in FBE (<a href="https://fbe.teoxoy.com/">Factorio Blueprint Editor</a>) with modifications (e.g. different candidate sorting)`,
    }),
    Snug: newBeaconStep({
      text: "snug",
      class: "text-bg-primary",
      shortDescription: "Joel's beacon planner",
      longDescription: "Joel's beacon planner algorithm, which prefers beacon positions closer to pumpjacks",
    }),
  },

  PolesStep: newStep({
    text: "poles",
    class: "text-bg-warning",
    shortDescription: "Joel's electric pole planner",
    longDescription: "Joel's electric pole planning algorithm, which adds electric poles around all powered entities and uses Bresenham's line to connect groups",
  }),
}

const allSteps: Step[] = []
allSteps.push(...Object.values(PipeStrategy).map(getPipeStep))
allSteps.push(Steps.OptimizeStep)
allSteps.push(Steps.UndergroundPipesStep)
allSteps.push(...Object.values(BeaconStrategy).map(getBeaconStep))
allSteps.push(Steps.PolesStep)

export const AllSteps: ReadonlyArray<Step> = allSteps

export function getPipeStep(pipeStrategy: PipeStrategy): Step {
  return Steps.PipeSteps[pipeStrategy]
}

export function getBeaconStep(beaconStrategy: BeaconStrategy): Step {
  return Steps.BeaconSteps[beaconStrategy]
}
