import type { Screen } from '../types'
import { apiClient } from './api'

export async function listScreens(applicationId: string): Promise<Screen[]> {
  const { data } = await apiClient.get<Screen[]>(`/applications/${applicationId}/screens`)
  return data
}

export async function createScreen(applicationId: string, input: { name: string; route: string }): Promise<Screen> {
  const { data } = await apiClient.post<Screen>(`/applications/${applicationId}/screens`, input)
  return data
}

export async function deleteScreen(applicationId: string, screenId: string): Promise<void> {
  await apiClient.delete(`/applications/${applicationId}/screens/${screenId}`)
}
