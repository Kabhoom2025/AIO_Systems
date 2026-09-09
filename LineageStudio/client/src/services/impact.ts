import { apiClient } from './api'

export interface ScreenRef {
  id: string
  name: string
  route: string
}

export interface ApiRef {
  id: string
  method: string
  path: string
}

export interface ServiceRef {
  id: string
  name: string
}

export interface TableRef {
  id: string
  name: string
}

export interface ColumnRef {
  id: string
  name: string
  tableId: string
  tableName: string
}

export interface ComponentRef {
  id: string
  name: string
  screenId: string
  screenName: string
}

export interface ApiFieldRef {
  apiId: string
  method: string
  path: string
  apiField: string
}

export interface ServiceFieldRef {
  serviceId: string
  serviceName: string
  serviceField: string
}

export interface TableImpact {
  tableId: string
  tableName: string
  screens: ScreenRef[]
  apis: ApiRef[]
  services: ServiceRef[]
  referencingTables: TableRef[]
  referencedTables: TableRef[]
}

export interface ColumnImpact {
  columnId: string
  columnName: string
  tableId: string
  tableName: string
  components: ComponentRef[]
  apiFields: ApiFieldRef[]
  serviceFields: ServiceFieldRef[]
  dependentColumns: ColumnRef[]
}

export async function getTableImpact(tableId: string): Promise<TableImpact> {
  const { data } = await apiClient.get<TableImpact>(`/impact/table/${tableId}`)
  return data
}

export async function getColumnImpact(columnId: string): Promise<ColumnImpact> {
  const { data } = await apiClient.get<ColumnImpact>(`/impact/column/${columnId}`)
  return data
}
