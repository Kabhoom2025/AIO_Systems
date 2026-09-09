import type { Application } from '../types'
import { apiClient } from './api'

export interface ApplicationVersion {
  id: string
  applicationId: string
  versionNumber: number
  publishedAt: string | null
  createdAt: string
}

export async function listApplications(): Promise<Application[]> {
  const { data } = await apiClient.get<Application[]>('/applications')
  return data
}

export async function createApplication(input: { name: string; description?: string }): Promise<Application> {
  const { data } = await apiClient.post<Application>('/applications', input)
  return data
}

export async function updateApplication(
  id: string,
  input: { name: string; description?: string },
): Promise<Application> {
  const { data } = await apiClient.put<Application>(`/applications/${id}`, input)
  return data
}

export async function deleteApplication(id: string): Promise<void> {
  await apiClient.delete(`/applications/${id}`)
}

export async function publishApplication(id: string): Promise<ApplicationVersion> {
  const { data } = await apiClient.post<ApplicationVersion>(`/applications/${id}/publish`)
  return data
}

export async function unpublishApplication(id: string): Promise<Application> {
  const { data } = await apiClient.post<Application>(`/applications/${id}/unpublish`)
  return data
}

export async function listApplicationVersions(id: string): Promise<ApplicationVersion[]> {
  const { data } = await apiClient.get<ApplicationVersion[]>(`/applications/${id}/versions`)
  return data
}
