import type { ApiEndpoint, HttpMethod } from '../types'
import { apiClient } from './api'

export interface ServiceDefinition {
  id: string
  applicationId: string
  name: string
  tableId: string | null
  description: string | null
  createdAt: string
}

export async function listServices(applicationId: string): Promise<ServiceDefinition[]> {
  const { data } = await apiClient.get<ServiceDefinition[]>(`/applications/${applicationId}/services`)
  return data
}

export async function createService(
  applicationId: string,
  input: { name: string; tableId: string | null; description: string | null },
): Promise<ServiceDefinition> {
  const { data } = await apiClient.post<ServiceDefinition>(`/applications/${applicationId}/services`, input)
  return data
}

export async function deleteService(applicationId: string, serviceId: string): Promise<void> {
  await apiClient.delete(`/applications/${applicationId}/services/${serviceId}`)
}

interface ApiEndpointWire {
  id: string
  applicationId: string
  method: HttpMethod
  path: string
  requestSchemaJson: string | null
  responseSchemaJson: string | null
  serviceId: string | null
  tableId: string | null
  createdAt: string
}

function fromWire(a: ApiEndpointWire): ApiEndpoint {
  return {
    id: a.id,
    applicationId: a.applicationId,
    method: a.method,
    path: a.path,
    requestSchema: a.requestSchemaJson ? JSON.parse(a.requestSchemaJson) : null,
    responseSchema: a.responseSchemaJson ? JSON.parse(a.responseSchemaJson) : null,
    serviceId: a.serviceId,
    tableId: a.tableId,
  }
}

export async function listApis(applicationId: string): Promise<ApiEndpoint[]> {
  const { data } = await apiClient.get<ApiEndpointWire[]>(`/applications/${applicationId}/apis`)
  return data.map(fromWire)
}

export interface ApiEndpointInput {
  method: HttpMethod
  path: string
  requestSchemaJson: string | null
  responseSchemaJson: string | null
  serviceId: string | null
  tableId: string | null
}

export async function createApi(applicationId: string, input: ApiEndpointInput): Promise<ApiEndpoint> {
  const { data } = await apiClient.post<ApiEndpointWire>(`/applications/${applicationId}/apis`, input)
  return fromWire(data)
}

export async function deleteApi(applicationId: string, apiId: string): Promise<void> {
  await apiClient.delete(`/applications/${applicationId}/apis/${apiId}`)
}
