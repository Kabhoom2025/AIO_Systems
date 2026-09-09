import axios from 'axios'
import { API_BASE_URL } from './api'
import type { RuntimeExecutionResult } from './runtime'
import type { Application, ComponentType, DataTable, FieldMapping, Screen, TransformationType, UiComponent } from '../types'

/**
 * A separate, deliberately auth-free axios instance for the end-user-facing runtime (see
 * Platform.Api PublicController, [AllowAnonymous]) - keeps it independent of the builder's
 * apiClient (which attaches a Bearer token and logs the builder out on a 401), since a visitor
 * here never has - or needs - a LineageStudio login.
 */
const publicApiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

export async function getPublicApp(applicationId: string): Promise<Application> {
  const { data } = await publicApiClient.get<Application>(`/public/apps/${applicationId}`)
  return data
}

export async function listPublicScreens(applicationId: string): Promise<Screen[]> {
  const { data } = await publicApiClient.get<Screen[]>(`/public/apps/${applicationId}/screens`)
  return data
}

interface ComponentWire {
  id: string
  screenId: string
  type: ComponentType
  name: string
  positionX: number
  positionY: number
  propertiesJson: string | null
  validationJson: string | null
  dataBinding: string | null
  eventsJson: string | null
}

function componentFromWire(c: ComponentWire): UiComponent {
  return {
    id: c.id,
    screenId: c.screenId,
    type: c.type,
    name: c.name,
    position: { x: c.positionX, y: c.positionY },
    properties: c.propertiesJson ? JSON.parse(c.propertiesJson) : {},
    validation: c.validationJson ? JSON.parse(c.validationJson) : null,
    dataBinding: c.dataBinding,
    events: c.eventsJson ? JSON.parse(c.eventsJson) : null,
  }
}

export async function listPublicComponents(applicationId: string, screenId: string): Promise<UiComponent[]> {
  const { data } = await publicApiClient.get<ComponentWire[]>(
    `/public/apps/${applicationId}/screens/${screenId}/components`,
  )
  return data.map(componentFromWire)
}

interface MappingWire {
  id: string
  applicationId: string
  sourceComponentId: string
  sourceField: string
  apiId: string
  apiField: string
  serviceId: string | null
  serviceField: string | null
  columnId: string
  transformation: TransformationType
  transformationConfigJson: string | null
}

function mappingFromWire(m: MappingWire): FieldMapping {
  return {
    id: m.id,
    applicationId: m.applicationId,
    sourceComponentId: m.sourceComponentId,
    sourceField: m.sourceField,
    apiId: m.apiId,
    apiField: m.apiField,
    serviceId: m.serviceId,
    serviceField: m.serviceField,
    columnId: m.columnId,
    transformation: m.transformation,
    transformationConfig: m.transformationConfigJson ? JSON.parse(m.transformationConfigJson) : null,
  }
}

export async function listPublicMappings(applicationId: string): Promise<FieldMapping[]> {
  const { data } = await publicApiClient.get<MappingWire[]>(`/public/apps/${applicationId}/mappings`)
  return data.map(mappingFromWire)
}

export async function listPublicTables(applicationId: string): Promise<DataTable[]> {
  const { data } = await publicApiClient.get<DataTable[]>(`/public/apps/${applicationId}/tables`)
  return data
}

/** Existing rows of one of the app's own tables - used to populate a foreign-key picker with real
 *  rows instead of asking an end user to type a raw UUID. */
export async function listPublicTableRows(
  applicationId: string,
  tableId: string,
): Promise<Record<string, unknown>[]> {
  const { data } = await publicApiClient.get<Record<string, unknown>[]>(
    `/public/apps/${applicationId}/tables/${tableId}/rows`,
  )
  return data
}

export async function executePublicApi(
  applicationId: string,
  apiId: string,
  formData: Record<string, unknown>,
): Promise<RuntimeExecutionResult> {
  const { data } = await publicApiClient.post<RuntimeExecutionResult>(`/public/apps/${applicationId}/execute`, {
    apiId,
    formData,
  })
  return data
}
