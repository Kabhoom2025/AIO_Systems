import type { ExecutionStatus, LineageEvent, LineageExecution, LineageNodeType } from '../types'
import { apiClient } from './api'

export type LineageExecutionWire = LineageExecution

export async function listExecutions(applicationId: string): Promise<LineageExecutionWire[]> {
  const { data } = await apiClient.get<LineageExecutionWire[]>('/lineage/executions', { params: { applicationId } })
  return data
}

/** All executions across every application - the Execution History view. */
export async function listAllExecutions(): Promise<LineageExecutionWire[]> {
  const { data } = await apiClient.get<LineageExecutionWire[]>('/lineage/executions')
  return data
}

export async function getExecution(executionId: string): Promise<LineageExecutionWire> {
  const { data } = await apiClient.get<LineageExecutionWire>(`/lineage/executions/${executionId}`)
  return data
}

export interface LineageEventWire {
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
  metadataJson: string | null
  errorCode: string | null
  errorMessage: string | null
}

export function fromWire(e: LineageEventWire): LineageEvent {
  return {
    id: e.id,
    executionId: e.executionId,
    applicationId: e.applicationId,
    parentEventId: e.parentEventId,
    nodeId: e.nodeId,
    nodeType: e.nodeType,
    eventType: e.eventType,
    status: e.status,
    startedAt: e.startedAt,
    completedAt: e.completedAt,
    durationMs: e.durationMs,
    metadata: e.metadataJson ? JSON.parse(e.metadataJson) : null,
    errorCode: e.errorCode,
    errorMessage: e.errorMessage,
  }
}

export async function listEvents(executionId: string): Promise<LineageEvent[]> {
  const { data } = await apiClient.get<LineageEventWire[]>(`/lineage/${executionId}/events`)
  return data.map(fromWire)
}

export const STATUS_LABEL: Record<ExecutionStatus, string> = {
  Pending: '⏳ PENDING',
  Running: '⟳ RUNNING',
  Success: '✓ SUCCESS',
  Failed: '✗ FAILED',
  Skipped: '⏹ SKIPPED',
}
