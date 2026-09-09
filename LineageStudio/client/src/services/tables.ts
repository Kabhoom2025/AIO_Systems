import type { ColumnDataType, ColumnDefinition, DataTable } from '../types'
import { apiClient } from './api'

export async function listTables(applicationId: string): Promise<DataTable[]> {
  const { data } = await apiClient.get<DataTable[]>(`/applications/${applicationId}/tables`)
  return data
}

export async function createTable(
  applicationId: string,
  input: { name: string; columns: ColumnDefinition[] },
): Promise<DataTable> {
  const { data } = await apiClient.post<DataTable>(`/applications/${applicationId}/tables`, input)
  return data
}

export async function deleteTable(applicationId: string, tableId: string): Promise<void> {
  await apiClient.delete(`/applications/${applicationId}/tables/${tableId}`)
}

export async function addColumn(
  applicationId: string,
  tableId: string,
  column: ColumnDefinition,
): Promise<DataTable> {
  await apiClient.post(`/applications/${applicationId}/tables/${tableId}/columns`, { column })
  const { data } = await apiClient.get<DataTable>(`/applications/${applicationId}/tables/${tableId}`)
  return data
}

export async function deleteColumn(applicationId: string, tableId: string, columnId: string): Promise<void> {
  await apiClient.delete(`/applications/${applicationId}/tables/${tableId}/columns/${columnId}`)
}

/** The most-recently-written real rows of this table - "what data has actually flowed into the
 *  database", most recent first. */
export async function listLatestTableRows(
  applicationId: string,
  tableId: string,
  limit = 10,
): Promise<Record<string, unknown>[]> {
  const { data } = await apiClient.get<Record<string, unknown>[]>(
    `/applications/${applicationId}/tables/${tableId}/rows`,
    { params: { limit } },
  )
  return data
}

export function emptyColumn(): ColumnDefinition {
  return {
    name: '',
    dataType: 'Varchar' as ColumnDataType,
    isNullable: true,
  }
}
