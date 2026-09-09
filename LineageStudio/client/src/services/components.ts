import type { ComponentType, UiComponent } from '../types'
import { apiClient } from './api'

export interface CreateComponentInput {
  screenId: string
  type: ComponentType
  name: string
  positionX: number
  positionY: number
  propertiesJson?: string | null
  validationJson?: string | null
  dataBinding?: string | null
  eventsJson?: string | null
}

export interface UpdateComponentInput {
  name: string
  positionX: number
  positionY: number
  propertiesJson?: string | null
  validationJson?: string | null
  dataBinding?: string | null
  eventsJson?: string | null
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

function fromWire(c: ComponentWire): UiComponent {
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

export async function listComponents(applicationId: string, screenId?: string): Promise<UiComponent[]> {
  const { data } = await apiClient.get<ComponentWire[]>(`/applications/${applicationId}/components`, {
    params: screenId ? { screenId } : undefined,
  })
  return data.map(fromWire)
}

export async function createComponent(applicationId: string, input: CreateComponentInput): Promise<UiComponent> {
  const { data } = await apiClient.post<ComponentWire>(`/applications/${applicationId}/components`, input)
  return fromWire(data)
}

export async function updateComponent(
  applicationId: string,
  componentId: string,
  input: UpdateComponentInput,
): Promise<UiComponent> {
  const { data } = await apiClient.put<ComponentWire>(`/applications/${applicationId}/components/${componentId}`, input)
  return fromWire(data)
}

export async function deleteComponent(applicationId: string, componentId: string): Promise<void> {
  await apiClient.delete(`/applications/${applicationId}/components/${componentId}`)
}
