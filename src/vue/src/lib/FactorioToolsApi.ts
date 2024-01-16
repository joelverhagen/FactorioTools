/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

/** The strategy to use when planning beacon placement around the pumpjacks. */
export enum BeaconStrategy {
  FbeOriginal = "FbeOriginal",
  Fbe = "Fbe",
  Snug = "Snug",
}

/** The properties needed to normalize a oil field blueprint. */
export interface OilFieldNormalizeRequest {
  /**
   * The input blueprint containing at least one pumpjack.
   * @minLength 1
   */
  blueprint: string;
}

/** The properties needed to normalize a oil field blueprint. */
export interface OilFieldNormalizeRequestResponse {
  /**
   * The input blueprint containing at least one pumpjack.
   * @minLength 1
   */
  blueprint: string;
}

/** The normalized oil field blueprint. */
export interface OilFieldNormalizeResponse {
  /** The original request provided, included expanded defaults. */
  request: OilFieldNormalizeRequestResponse;
  /** The output normalized blueprint. */
  blueprint: string;
}

/** A particular attempt oil field plan. */
export interface OilFieldPlan {
  /** The pipe strategy used to generate the plan. */
  pipeStrategy: PipeStrategy;
  /** Whether or not the pipe optimized was used. */
  optimizePipes: boolean;
  /** Which beacon strategy, if any, was used. */
  beaconStrategy?: BeaconStrategy | null;
  /**
   * The number of effects the beacons provided to pumpjacks. Higher is better.
   * @format int32
   */
  beaconEffectCount: number;
  /**
   * The number of beacons in the plan. For the same number of beacon effects, lower is better.
   * @format int32
   */
  beaconCount: number;
  /**
   * The number of pipes in the plan. For the same number of beacon effects and beacons, lower is better. If underground pipes are used, this only counts the upwards and downwards connections for the underground stretches of pipes.
   * @format int32
   */
  pipeCount: number;
  /**
   * The number of pipes before beacons or underground pipes are placed.
   * @format int32
   */
  pipeCountWithoutUnderground: number;
}

/** The properties needed to generate an oil field plan. */
export interface OilFieldPlanRequest {
  /**
   * Whether or not underground pipes (pipe-to-ground) should be used.
   * @default true
   */
  useUndergroundPipes?: boolean;
  /**
   * Whether or not to add beacons around the pumpjacks.
   * @default true
   */
  addBeacons?: boolean;
  /**
   * Whether or not to use the pipe optimizer after each pipe strategy is executed. If set to true, the best solution
   * found will still be used, meaning if the unoptimized pipe plan performs better, it will be preferred over the
   * corresponding optimized pipe plan.
   * @default true
   */
  optimizePipes?: boolean;
  /**
   * Whether or to allow beacon effects to overlap. For Factorio mods like Space Exploration, beacon effects cannot
   * overlap otherwise pumpjacks will break down with a beacon overload. For vanilla Factorio, this should be true.
   * @default true
   */
  overlapBeacons?: boolean;
  /**
   * Whether or not to add electric poles around the pumpjacks and (optionally) beacons.
   * @default true
   */
  addElectricPoles?: boolean;
  /**
   * The pipe planning strategies to attempt.
   * @default ["Fbe","ConnectedCentersDelaunay","ConnectedCentersDelaunayMst","ConnectedCentersFlute"]
   */
  pipeStrategies?: PipeStrategy[];
  /**
   * The beacon planning strategies to attempt. This will have no affect if Knapcode.FactorioTools.OilField.OilFieldOptions.AddBeacons is false.
   * @default ["Fbe","Snug"]
   */
  beaconStrategies?: BeaconStrategy[];
  /**
   * The internal entity name for the electric pole to use.
   * @default "medium-electric-pole"
   */
  electricPoleEntityName?: string;
  /**
   * The supply width (horizontal) for the electric pole. This is the width of the area that the electric pole will
   * provide power to.
   * @format int32
   * @default 7
   */
  electricPoleSupplyWidth?: number;
  /**
   * The supply height (vertical) for the electric pole. This is the height of the area that the electric pole will
   * provide power to.
   * @format int32
   * @default 7
   */
  electricPoleSupplyHeight?: number;
  /**
   * The wire reach for the electric pole. This is how far apart electric poles can be but still be connected.
   * @format double
   * @default 9
   */
  electricPoleWireReach?: number;
  /**
   * The width of the electric pole entity.
   * @format int32
   * @default 1
   */
  electricPoleWidth?: number;
  /**
   * The height of the electric pole entity.
   * @format int32
   * @default 1
   */
  electricPoleHeight?: number;
  /**
   * The internal entity name for the beacon to use.
   * @default "beacon"
   */
  beaconEntityName?: string;
  /**
   * The supply width (horizontal) for the beacon. This is the width of the area that the beacon will provide
   * module effects to.
   * @format int32
   * @default 9
   */
  beaconSupplyWidth?: number;
  /**
   * The supply height (vertical) for the beacon. This is the height of the area that the beacon will provide
   * module effects to.
   * @format int32
   * @default 9
   */
  beaconSupplyHeight?: number;
  /**
   * The width of the beacon entity.
   * @format int32
   * @default 3
   */
  beaconWidth?: number;
  /**
   * The height of the beacon entity.
   * @format int32
   * @default 3
   */
  beaconHeight?: number;
  /**
   * Whether or not additional validations should be perform on the blueprint correctness. In most cases this should
   * be false. If you see an invalid blueprint returned, try setting this to true and reporting a bug.
   * @default false
   */
  validateSolution?: boolean;
  /**
   * The modules to add to the pumpjacks. The string key is the internal item name for the module. The value is the
   * count that kind of module to add to each pumpjack. There can be multiple module types provided.
   * @default {"productivity-module-3":2}
   */
  pumpjackModules?: Record<string, number>;
  /**
   * The modules to add to the beacons. The string key is the internal item name for the module. The value is the
   * count that kind of module to add to each beacon. There can be multiple module types provided.
   * @default {"speed-module-3":2}
   */
  beaconModules?: Record<string, number>;
  /**
   * The input blueprint containing at least one pumpjack.
   * @minLength 1
   * @example "0eJyMj70OwjAMhN/lZg8NbHkVhFB/rMrQuFGSIqoq707aMiCVgcWSz+fP5wXNMLEPogl2gbSjRtjLgii91sOqae0YFn5y/l63DxDS7FdFEjtkgmjHL1iTrwTWJEl4Z2zNfNPJNRyKgX6w/BjLwqjrpQI5E+ZSC7WTwO0+qTIdYKc/YKbaaOaAK0G38Pbre8KTQ/wY8hsAAP//AwAEfF3F"
   */
  blueprint: string;
  /**
   * Whether or not to add a placeholder entity to the output grid so that the planning grid entity coordinates match
   * the entity coordinate when the output blueprint is pasted into Factorio Blueprint Editor (FBE). This helps with
   * debugging the planner.
   * @default false
   */
  addFbeOffset?: boolean;
}

/** The properties needed to generate an oil field plan. */
export interface OilFieldPlanRequestResponse {
  /** Whether or not underground pipes (pipe-to-ground) should be used. */
  useUndergroundPipes: boolean;
  /** Whether or not to add beacons around the pumpjacks. */
  addBeacons: boolean;
  /**
   * Whether or not to use the pipe optimizer after each pipe strategy is executed. If set to true, the best solution
   * found will still be used, meaning if the unoptimized pipe plan performs better, it will be preferred over the
   * corresponding optimized pipe plan.
   */
  optimizePipes: boolean;
  /**
   * Whether or to allow beacon effects to overlap. For Factorio mods like Space Exploration, beacon effects cannot
   * overlap otherwise pumpjacks will break down with a beacon overload. For vanilla Factorio, this should be true.
   */
  overlapBeacons: boolean;
  /** Whether or not to add electric poles around the pumpjacks and (optionally) beacons. */
  addElectricPoles: boolean;
  /** The pipe planning strategies to attempt. */
  pipeStrategies: PipeStrategy[];
  /** The beacon planning strategies to attempt. This will have no affect if Knapcode.FactorioTools.OilField.OilFieldOptions.AddBeacons is false. */
  beaconStrategies: BeaconStrategy[];
  /** The internal entity name for the electric pole to use. */
  electricPoleEntityName: string;
  /**
   * The supply width (horizontal) for the electric pole. This is the width of the area that the electric pole will
   * provide power to.
   * @format int32
   */
  electricPoleSupplyWidth: number;
  /**
   * The supply height (vertical) for the electric pole. This is the height of the area that the electric pole will
   * provide power to.
   * @format int32
   */
  electricPoleSupplyHeight: number;
  /**
   * The wire reach for the electric pole. This is how far apart electric poles can be but still be connected.
   * @format double
   */
  electricPoleWireReach: number;
  /**
   * The width of the electric pole entity.
   * @format int32
   */
  electricPoleWidth: number;
  /**
   * The height of the electric pole entity.
   * @format int32
   */
  electricPoleHeight: number;
  /** The internal entity name for the beacon to use. */
  beaconEntityName: string;
  /**
   * The supply width (horizontal) for the beacon. This is the width of the area that the beacon will provide
   * module effects to.
   * @format int32
   */
  beaconSupplyWidth: number;
  /**
   * The supply height (vertical) for the beacon. This is the height of the area that the beacon will provide
   * module effects to.
   * @format int32
   */
  beaconSupplyHeight: number;
  /**
   * The width of the beacon entity.
   * @format int32
   */
  beaconWidth: number;
  /**
   * The height of the beacon entity.
   * @format int32
   */
  beaconHeight: number;
  /**
   * Whether or not additional validations should be perform on the blueprint correctness. In most cases this should
   * be false. If you see an invalid blueprint returned, try setting this to true and reporting a bug.
   */
  validateSolution: boolean;
  /**
   * The modules to add to the pumpjacks. The string key is the internal item name for the module. The value is the
   * count that kind of module to add to each pumpjack. There can be multiple module types provided.
   */
  pumpjackModules: Record<string, number>;
  /**
   * The modules to add to the beacons. The string key is the internal item name for the module. The value is the
   * count that kind of module to add to each beacon. There can be multiple module types provided.
   */
  beaconModules: Record<string, number>;
  /**
   * The input blueprint containing at least one pumpjack.
   * @minLength 1
   */
  blueprint: string;
  /**
   * Whether or not to add a placeholder entity to the output grid so that the planning grid entity coordinates match
   * the entity coordinate when the output blueprint is pasted into Factorio Blueprint Editor (FBE). This helps with
   * debugging the planner.
   */
  addFbeOffset: boolean;
}

/** The resulting oil field plan. */
export interface OilFieldPlanResponse {
  /** The original request provided, included expanded defaults. */
  request: OilFieldPlanRequestResponse;
  /** The output blueprint, containing the planned oil field. */
  blueprint: string;
  /** A summary of different oil field plans attempted and their performance. */
  summary: OilFieldPlanSummary;
}

/** A summary of the various oil field plans attempted. */
export interface OilFieldPlanSummary {
  /**
   * The number of pumpjacks removed to allow for electric poles. This must be zero.
   * @format int32
   */
  missingPumpjacks: number;
  /**
   * The number of pumpjacks that were rotated from their original position.
   * @format int32
   */
  rotatedPumpjacks: number;
  /** The set of plans which exactly the same and determined to be the best. */
  selectedPlans: OilFieldPlan[];
  /** The set of plans which are equivalent to the selected plans by ranking but not exactly the same. */
  alternatePlans: OilFieldPlan[];
  /** The set of plans that were not the best and were discarded. */
  unusedPlans: OilFieldPlan[];
}

/** The strategy to use while planning pipes between pumpjacks. */
export enum PipeStrategy {
  FbeOriginal = "FbeOriginal",
  Fbe = "Fbe",
  ConnectedCentersDelaunay = "ConnectedCentersDelaunay",
  ConnectedCentersDelaunayMst = "ConnectedCentersDelaunayMst",
  ConnectedCentersFlute = "ConnectedCentersFlute",
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<FullRequestParams, "body" | "method" | "query" | "path">;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (securityData: SecurityDataType | null) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown> extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) => fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter((key) => "undefined" !== typeof query[key]);
    return keys
      .map((key) => (Array.isArray(query[key]) ? this.addArrayQueryParam(query, key) : this.addQueryParam(query, key)))
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string") ? JSON.stringify(input) : input,
    [ContentType.Text]: (input: any) => (input !== null && typeof input !== "string" ? JSON.stringify(input) : input),
    [ContentType.FormData]: (input: any) =>
      Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
            ? JSON.stringify(property)
            : `${property}`,
        );
        return formData;
      }, new FormData()),
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(params1: RequestParams, params2?: RequestParams): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (cancelToken: CancelToken): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(`${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`, {
      ...requestParams,
      headers: {
        ...(requestParams.headers || {}),
        ...(type && type !== ContentType.FormData ? { "Content-Type": type } : {}),
      },
      signal: cancelToken ? this.createAbortSignal(cancelToken) : requestParams.signal,
      body: typeof body === "undefined" || body === null ? null : payloadFormatter(body),
    }).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const data = !responseFormat
        ? r
        : await response[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title Knapcode.FactorioTools.WebApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
 * @version 1.0
 */
export class Api<SecurityDataType extends unknown> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags OilField
     * @name V1OilFieldNormalizeCreate
     * @request POST:/api/v1/oil-field/normalize
     */
    v1OilFieldNormalizeCreate: (data: OilFieldNormalizeRequest, params: RequestParams = {}) =>
      this.request<OilFieldNormalizeResponse, any>({
        path: `/api/v1/oil-field/normalize`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags OilField
     * @name V1OilFieldPlanCreate
     * @request POST:/api/v1/oil-field/plan
     */
    v1OilFieldPlanCreate: (data: OilFieldPlanRequest, params: RequestParams = {}) =>
      this.request<OilFieldPlanResponse, any>({
        path: `/api/v1/oil-field/plan`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
}
