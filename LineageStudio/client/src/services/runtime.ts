import { apiClient } from './api'

export interface RuntimeFieldResult {
  apiField: string
  columnName: string
  rawValue: string | null
  transformedValue: string | null
}

export interface RuntimeExecutionResult {
  success: boolean
  errorCode: string | null
  errorMessage: string | null
  fields: RuntimeFieldResult[]
  rows: Record<string, unknown>[]
}

export async function executeApi(
  applicationId: string,
  apiId: string,
  formData: Record<string, unknown>,
): Promise<RuntimeExecutionResult> {
  const { data } = await apiClient.post<RuntimeExecutionResult>(`/runtime/${applicationId}/execute`, {
    apiId,
    formData,
  })
  return data
}
