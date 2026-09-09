import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface WorkflowStepInstanceDto {
  id: number;
  stepOrder: number;
  approverRoleId?: number | null;
  approverRoleName?: string | null;
  approverUserId?: number | null;
  approverUserName?: string | null;
  status: string;
  actionedDate?: string | null;
  comments?: string | null;
}

export interface WorkflowInstanceDto {
  id: number;
  workflowDefinitionId: number;
  workflowDefinitionName: string;
  entityType: string;
  entityId: number;
  amount?: number | null;
  status: string;
  currentStepOrder: number;
  submittedByUserId: number;
  submittedDate: string;
  completedDate?: string | null;
  steps: WorkflowStepInstanceDto[];
}

export interface StartWorkflowDto {
  entityType: string;
  entityId: number;
  amount?: number | null;
}

export interface WorkflowActionDto {
  comments?: string | null;
}

@Injectable({ providedIn: 'root' })
export class WorkflowInstanceApiService {
  private instancesUrl = `${environment.apiUrl}/workflow-instances`;

  constructor(private http: HttpClient) {}

  start(dto: StartWorkflowDto): Observable<WorkflowInstanceDto> {
    return this.http.post<WorkflowInstanceDto>(this.instancesUrl, dto);
  }

  getPending(): Observable<WorkflowInstanceDto[]> {
    return this.http.get<WorkflowInstanceDto[]>(`${this.instancesUrl}/pending`);
  }

  getByEntity(entityType: string, entityId: number): Observable<WorkflowInstanceDto[]> {
    const params = new HttpParams().set('entityType', entityType).set('entityId', entityId);
    return this.http.get<WorkflowInstanceDto[]>(`${this.instancesUrl}/by-entity`, { params });
  }

  approve(id: number, dto: WorkflowActionDto): Observable<WorkflowInstanceDto> {
    return this.http.post<WorkflowInstanceDto>(`${this.instancesUrl}/${id}/approve`, dto);
  }

  reject(id: number, dto: WorkflowActionDto): Observable<WorkflowInstanceDto> {
    return this.http.post<WorkflowInstanceDto>(`${this.instancesUrl}/${id}/reject`, dto);
  }
}
