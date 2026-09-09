import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface WorkflowStepDefinitionDto {
  id: number;
  stepOrder: number;
  name: string;
  approverRoleId?: number | null;
  approverRoleName?: string | null;
  minAmount?: number | null;
}

export interface CreateWorkflowStepDefinitionDto {
  stepOrder: number;
  name: string;
  approverRoleId?: number | null;
  minAmount?: number | null;
}

export interface WorkflowDefinitionDto {
  id: number;
  name: string;
  entityType: string;
  isActive: boolean;
  steps: WorkflowStepDefinitionDto[];
}

export interface CreateWorkflowDefinitionDto {
  name: string;
  entityType: string;
  isActive: boolean;
  steps: CreateWorkflowStepDefinitionDto[];
}

export interface UpdateWorkflowDefinitionDto {
  name: string;
  entityType: string;
  isActive: boolean;
  steps: CreateWorkflowStepDefinitionDto[];
}

@Injectable({ providedIn: 'root' })
export class WorkflowDefinitionApiService {
  private definitionsUrl = `${environment.apiUrl}/workflow-definitions`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<WorkflowDefinitionDto[]> {
    return this.http.get<WorkflowDefinitionDto[]>(this.definitionsUrl);
  }

  getById(id: number): Observable<WorkflowDefinitionDto> {
    return this.http.get<WorkflowDefinitionDto>(`${this.definitionsUrl}/${id}`);
  }

  create(dto: CreateWorkflowDefinitionDto): Observable<WorkflowDefinitionDto> {
    return this.http.post<WorkflowDefinitionDto>(this.definitionsUrl, dto);
  }

  update(id: number, dto: UpdateWorkflowDefinitionDto): Observable<WorkflowDefinitionDto> {
    return this.http.put<WorkflowDefinitionDto>(`${this.definitionsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.definitionsUrl}/${id}`);
  }
}
