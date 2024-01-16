import { OilFieldStoreState, useOilFieldStore } from "../stores/OilFieldStore";
import { Api, BeaconStrategy, HttpResponse, OilFieldNormalizeRequest, OilFieldNormalizeResponse, OilFieldPlanRequest, OilFieldPlanResponse, PipeStrategy } from "./FactorioToolsApi";
import { getEntries } from "./helpers";

type RequestPropertyGetters = {
  [Property in keyof OilFieldPlanRequest]-?: (state: OilFieldStoreState) => Exclude<OilFieldPlanRequest[Property], undefined>
};

const requestPropertyGetters: RequestPropertyGetters = {
  addBeacons: (state) => state.addBeacons,
  addElectricPoles: (_) => true,
  addFbeOffset: (_) => false,
  beaconEntityName: (state) => state.beaconEntityName.trim(),
  beaconHeight: (state) => state.beaconHeight,
  beaconModules: (state) => {
    const output: Record<string, number> = {};
    const module = state.beaconModule.trim()
    if (module) {
      output[module] = state.beaconModuleSlots;
    }
    return output;
  },
  beaconStrategies: (state) => [
    state.beaconStrategyFbeOriginal ? BeaconStrategy.FbeOriginal : undefined,
    state.beaconStrategyFbe ? BeaconStrategy.Fbe : undefined,
    state.beaconStrategySnug ? BeaconStrategy.Snug : undefined,
  ].filter((b): b is BeaconStrategy => !!b),
  beaconSupplyHeight: (state) => state.beaconSupplyHeight,
  beaconSupplyWidth: (state) => state.beaconSupplyWidth,
  beaconWidth: (state) => state.beaconWidth,
  blueprint: (state) => state.inputBlueprint.trim(),
  electricPoleEntityName: (state) => state.electricPoleEntityName.trim(),
  electricPoleHeight: (state) => state.electricPoleHeight,
  electricPoleSupplyHeight: (state) => state.electricPoleSupplyHeight,
  electricPoleSupplyWidth: (state) => state.electricPoleSupplyWidth,
  electricPoleWidth: (state) => state.electricPoleWidth,
  electricPoleWireReach: (state) => state.electricPoleWireReach,
  optimizePipes: (state) => state.optimizePipes,
  overlapBeacons: (state) => state.overlapBeacons,
  pipeStrategies: (state) => [
    state.pipeStrategyFbeOriginal ? PipeStrategy.FbeOriginal : undefined,
    state.pipeStrategyFbe ? PipeStrategy.Fbe : undefined,
    state.pipeStrategyConnectedCentersDelaunay ? PipeStrategy.ConnectedCentersDelaunay : undefined,
    state.pipeStrategyConnectedCentersDelaunayMst ? PipeStrategy.ConnectedCentersDelaunayMst : undefined,
    state.pipeStrategyConnectedCentersFlute ? PipeStrategy.ConnectedCentersFlute : undefined,
  ].filter((b): b is PipeStrategy => !!b),
  pumpjackModules: (state) => {
    const output: Record<string, number> = {};
    const module = state.pumpjackModule.trim()
    if (module) {
      output[module] = 2;
    }
    return output;
  },
  useUndergroundPipes: (state) => state.useUndergroundPipes,
  validateSolution: (state) => state.validateSolution,
} as const;

export type ApiError =
  {
    isError: true,
    title: string,
    errors?: Record<string, string[]>,
    errorDetails?: string[],
    response?: HttpResponse<any, any>
  }

export interface ApiResult<Data> {
  isError: false,
  data: Data
}

async function getApiResultOrError<Request, Data>(invokeApi: (factorio: Api<unknown>) => Promise<HttpResponse<Data, any>>): Promise<ApiResult<Data> | ApiError> {

  const store = useOilFieldStore()
  const baseUrl = store.useStagingApi ? 'https://factoriotools-staging.azurewebsites.net' : 'https://factoriotools.azurewebsites.net'
  const factorio = new Api({ baseUrl })

  try {
    const response = await invokeApi(factorio);
    return {
      isError: false,
      data: response.data
    }
  } catch (e) {
    if (e instanceof Response) {
      const response = e as HttpResponse<Request, any>;
      const errors: Record<string, string[]> = {}
      const title = response.error.title ?? `HTTP ${response.status} (${response.statusText})`

      if (typeof response.error?.errors == 'object') {
        for (const [key, values] of Object.entries(response.error.errors)) {
          if (!Array.isArray(values)) {
            continue
          }

          const currentErrors = []
          for (const value of values) {
            if (typeof value === 'string') {
              currentErrors.push(value)
            }
          }

          if (currentErrors.length > 0) {
            errors[key] = currentErrors;
          }
        }

        return { isError: true, title, errors, response }
      } else {
        return { isError: true, title, response }
      }
    } else if (typeof e === 'object' && e instanceof Error) {
      return {
        isError: true,
        title: 'An unexpected error occurred.',
        errorDetails: [e.stack ?? e.toString()]
      }
    } else {
      return {
        isError: true,
        title: 'An unhandled error occurred.',
        errorDetails: [JSON.stringify(e)]
      }
    }
  }
}

export async function normalize(): Promise<ApiResult<OilFieldNormalizeResponse> | ApiError> {
  const store = useOilFieldStore()
  const request: OilFieldNormalizeRequest = { blueprint: store.$state.inputBlueprint }

  return await getApiResultOrError(factorio => factorio.api.v1OilFieldNormalizeCreate(request));
}

export async function getPlan(): Promise<ApiResult<OilFieldPlanResponse> | ApiError> {
  const store = useOilFieldStore()
  const request: OilFieldPlanRequest = { blueprint: "" }
  for (const [requestKey, getter] of getEntries(requestPropertyGetters)) {
    (request as any)[requestKey] = getter(store.$state)
  }

  return await getApiResultOrError(factorio => factorio.api.v1OilFieldPlanCreate(request));
}
