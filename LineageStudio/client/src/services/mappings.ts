import type { FieldMapping, TransformationType } from '../types'
import { apiClient } from './api'

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
  createdAt: string
}

function fromWire(m: MappingWire): FieldMapping {
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

export async function listMappings(applicationId: string): Promise<FieldMapping[]> {
  const { data } = await apiClient.get<MappingWire[]>(`/applications/${applicationId}/mappings`)
  return data.map(fromWire)
}

export interface CreateMappingInput {
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

export async function createMapping(applicationId: string, input: CreateMappingInput): Promise<FieldMapping> {
  const { data } = await apiClient.post<MappingWire>(`/applications/${applicationId}/mappings`, input)
  return fromWire(data)
}

export async function deleteMapping(applicationId: string, mappingId: string): Promise<void> {
  await apiClient.delete(`/applications/${applicationId}/mappings/${mappingId}`)
}
