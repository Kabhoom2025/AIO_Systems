import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface WorkflowDefinitionDto {
  id: number;
  name: string;
  description?: string | null;
  triggerType: string;
  triggerLabel: string;
  isActive: boolean;
  publishedVersionId?: number | null;
  publishedVersionNumber?: number | null;
  draftVersionId: number;
  draftVersionNumber: number;
  publishedAt?: string | null;
  updatedDate: string;
  webhookToken: string;
}

export interface CreateWorkflowDefinitionDto {
  name: string;
  description?: string | null;
  triggerType: string;
}

export interface UpdateWorkflowDefinitionDto {
  name: string;
  description?: string | null;
  isActive: boolean;
}

export interface WorkflowVersionDto {
  id: number;
  versionNumber: number;
  graphJson: string;
  status: string; // "Draft" | "Published" | "Archived"
  publishedAt?: string | null;
  publishedByName?: string | null;
  createdDate: string;
}

export interface SaveDraftGraphDto {
  graphJson: string;
}

export interface WorkflowFieldDto {
  key: string;
  label: string;
  type: 'string' | 'number' | 'boolean';
  options?: string[] | null;
}

export interface TriggerTypeDto {
  key: string;
  label: string;
  fields: WorkflowFieldDto[];
}

export interface ActionTypeDto {
  key: string;
  label: string;
  configFields: WorkflowFieldDto[];
}

export interface WorkflowExecutionDto {
  id: number;
  triggerEntityType: string;
  triggerEntityId: number;
  status: string;
  errorMessage?: string | null;
  pathJson: string;
  createdDate: string;
}

export interface RunWorkflowDto {
  context: Record<string, unknown>;
}

/// One step emitted while a run is streaming (matches the backend's WorkflowStepEvent) — type "done"
/// is a synthetic final event carrying the completed WorkflowExecutionDto, not a graph step.
export interface WorkflowStepEvent {
  node?: string;
  type?: string; // trigger | condition | action | done
  actionType?: string;
  result?: string;
  matchedBranch?: string;
  matchedBranchName?: string;
}

// ---------------- Graph JSON shape (client-side, stored as a JSON string in graphJson) ----------------

export interface WorkflowGraph {
  nodes: WorkflowNode[];
  edges: WorkflowEdge[];
}

export type WorkflowNodeType = 'trigger' | 'condition' | 'action' | 'end';

export interface WorkflowNode {
  id: string;
  type: WorkflowNodeType;
  x: number;
  y: number;
  data: TriggerData | ConditionData | ActionData | Record<string, unknown>;
}

export interface TriggerData {
  triggerType: string;
  label: string;
  description?: string;
}

export interface ConditionRule {
  field: string;
  operator: string; // Equals | NotEquals | GreaterThan | GreaterThanOrEqual | LessThan | LessThanOrEqual | Contains
  value: string;
}

export interface DecisionBranch {
  id: string;
  name: string;
  conditions: ConditionRule[];
}

export interface ConditionData {
  label: string;
  branches: DecisionBranch[];
  description?: string;
}

export interface ActionData {
  actionType: string;
  label: string;
  config: Record<string, string>;
  description?: string;
}

export interface WorkflowEdge {
  id: string;
  source: string;
  target: string;
  branch?: string; // 'true' | 'false' | 'group-N' — only set on edges leaving a 'condition' node
  label?: string;
}

@Injectable({ providedIn: 'root' })
export class WorkflowApiService {
  private baseUrl = `${environment.apiUrl}/workflows`;

  constructor(private http: HttpClient) {}

  getWorkflows(): Observable<WorkflowDefinitionDto[]> {
    return this.http.get<WorkflowDefinitionDto[]>(this.baseUrl);
  }

  getWorkflow(id: number): Observable<WorkflowDefinitionDto> {
    return this.http.get<WorkflowDefinitionDto>(`${this.baseUrl}/${id}`);
  }

  createWorkflow(dto: CreateWorkflowDefinitionDto): Observable<WorkflowDefinitionDto> {
    return this.http.post<WorkflowDefinitionDto>(this.baseUrl, dto);
  }

  updateWorkflow(id: number, dto: UpdateWorkflowDefinitionDto): Observable<WorkflowDefinitionDto> {
    return this.http.put<WorkflowDefinitionDto>(`${this.baseUrl}/${id}`, dto);
  }

  deleteWorkflow(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getVersions(id: number): Observable<WorkflowVersionDto[]> {
    return this.http.get<WorkflowVersionDto[]>(`${this.baseUrl}/${id}/versions`);
  }

  getDraft(id: number): Observable<WorkflowVersionDto> {
    return this.http.get<WorkflowVersionDto>(`${this.baseUrl}/${id}/draft`);
  }

  saveDraft(id: number, dto: SaveDraftGraphDto): Observable<WorkflowVersionDto> {
    return this.http.put<WorkflowVersionDto>(`${this.baseUrl}/${id}/draft`, dto);
  }

  publish(id: number): Observable<WorkflowDefinitionDto> {
    return this.http.post<WorkflowDefinitionDto>(`${this.baseUrl}/${id}/publish`, {});
  }

  getTriggerTypes(): Observable<TriggerTypeDto[]> {
    return this.http.get<TriggerTypeDto[]>(`${this.baseUrl}/meta/triggers`);
  }

  getActionTypes(): Observable<ActionTypeDto[]> {
    return this.http.get<ActionTypeDto[]>(`${this.baseUrl}/meta/actions`);
  }

  getExecutions(id: number): Observable<WorkflowExecutionDto[]> {
    return this.http.get<WorkflowExecutionDto[]>(`${this.baseUrl}/${id}/executions`);
  }

  run(id: number, dto: RunWorkflowDto): Observable<WorkflowExecutionDto> {
    return this.http.post<WorkflowExecutionDto>(`${this.baseUrl}/${id}/run`, dto);
  }

  webhookUrl(token: string): string {
    return `${this.baseUrl}/webhook/${token}`;
  }

  /// Not consumed via HttpClient — SSE responses need a raw fetch() + stream reader, so callers
  /// use this URL directly with fetch().
  runStreamUrl(id: number): string {
    return `${this.baseUrl}/${id}/run-stream`;
  }
}
