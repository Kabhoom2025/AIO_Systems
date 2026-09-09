export type ExecutionStatus = 'Pending' | 'Running' | 'Success' | 'Failed' | 'Skipped'

export interface Application {
  id: string
  name: string
  description: string | null
  isPublished: boolean
  currentVersionId: string | null
  createdAt: string
  updatedAt: string
}

export interface Screen {
  id: string
  applicationId: string
  name: string
  route: string
  createdAt: string
}

export type ComponentType =
  | 'Text'
  | 'Input'
  | 'Number'
  | 'Email'
  | 'Select'
  | 'Checkbox'
  | 'Radio'
  | 'Date'
  | 'Button'
  | 'Table'
  | 'Card'
  | 'Form'
  | 'Label'
  | 'Container'

export interface UiComponent {
  id: string
  screenId: string
  type: ComponentType
  name: string
  position: { x: number; y: number }
  properties: Record<string, unknown>
  validation: Record<string, unknown> | null
  dataBinding: string | null
  events: Record<string, unknown> | null
}

export type ColumnDataType =
  | 'Uuid'
  | 'Varchar'
  | 'Text'
  | 'Integer'
  | 'Bigint'
  | 'Decimal'
  | 'Boolean'
  | 'Date'
  | 'Timestamp'
  | 'Jsonb'

export const COLUMN_DATA_TYPES: ColumnDataType[] = [
  'Uuid',
  'Varchar',
  'Text',
  'Integer',
  'Bigint',
  'Decimal',
  'Boolean',
  'Date',
  'Timestamp',
  'Jsonb',
]

export interface DataColumn {
  id: string
  tableId: string
  name: string
  dataType: ColumnDataType
  length: number | null
  isPrimaryKey: boolean
  isForeignKey: boolean
  referencesTableId: string | null
  referencesColumnId: string | null
  isUnique: boolean
  isIndexed: boolean
  isNullable: boolean
  defaultValue: string | null
  ordinalPosition: number
}

export interface ColumnDefinition {
  name: string
  dataType: ColumnDataType
  length?: number | null
  isPrimaryKey?: boolean
  isForeignKey?: boolean
  referencesTableId?: string | null
  referencesColumnId?: string | null
  isUnique?: boolean
  isIndexed?: boolean
  isNullable?: boolean
  defaultValue?: string | null
}

export interface DataTable {
  id: string
  applicationId: string
  name: string
  schemaName: string
  columns: DataColumn[]
  createdAt: string
}

export type HttpMethod = 'Get' | 'Post' | 'Put' | 'Patch' | 'Delete'

export interface ApiEndpoint {
  id: string
  applicationId: string
  method: HttpMethod
  path: string
  requestSchema: Record<string, unknown> | null
  responseSchema: Record<string, unknown> | null
  serviceId: string | null
  tableId: string | null
}

export type TransformationType =
  | 'None'
  | 'Trim'
  | 'Uppercase'
  | 'Lowercase'
  | 'Default'
  | 'Concatenate'
  | 'Split'
  | 'DateConversion'
  | 'NumberConversion'

export interface FieldMapping {
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
  transformationConfig: Record<string, unknown> | null
}

export interface LineageExecution {
  id: string
  correlationId: string
  applicationId: string
  applicationName: string | null
  versionId: string
  startedAt: string
  completedAt: string | null
  status: ExecutionStatus
  durationMs: number | null
}

export type LineageNodeType =
  | 'Screen'
  | 'Component'
  | 'Api'
  | 'Controller'
  | 'Service'
  | 'Repository'
  | 'Database'
  | 'Table'
  | 'Column'
  | 'Transformation'

export interface LineageEvent {
  id: string
  executionId: string
  applicationId: string
  parentEventId: string | null
  nodeId: string
  nodeType: LineageNodeType
  eventType: string
  status: ExecutionStatus
  startedAt: string
  completedAt: string | null
  durationMs: number | null
  metadata: Record<string, unknown> | null
  errorCode: string | null
  errorMessage: string | null
}
