import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AutomationRuleDto {
  id: number;
  name: string;
  triggerEvent: string;
  actionType: string;
  notifyRoleId?: number | null;
  notifyRoleName?: string | null;
  notifyMessageTemplate: string;
  isEnabled: boolean;
}

export interface CreateAutomationRuleDto {
  name: string;
  triggerEvent: string;
  actionType: string;
  notifyRoleId?: number | null;
  notifyMessageTemplate: string;
  isEnabled: boolean;
}

export interface UpdateAutomationRuleDto {
  name: string;
  triggerEvent: string;
  actionType: string;
  notifyRoleId?: number | null;
  notifyMessageTemplate: string;
  isEnabled: boolean;
}

/** Fixed vocabulary from NovaERP.Application.Common.AutomationEvents — keep in sync manually,
 *  there is no API endpoint exposing this list. */
export const AUTOMATION_TRIGGER_EVENTS: string[] = [
  'WorkflowInstance.Approved',
  'WorkflowInstance.Rejected',
  'Document.Uploaded'
];

@Injectable({ providedIn: 'root' })
export class AutomationRuleApiService {
  private rulesUrl = `${environment.apiUrl}/automation-rules`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AutomationRuleDto[]> {
    return this.http.get<AutomationRuleDto[]>(this.rulesUrl);
  }

  getById(id: number): Observable<AutomationRuleDto> {
    return this.http.get<AutomationRuleDto>(`${this.rulesUrl}/${id}`);
  }

  create(dto: CreateAutomationRuleDto): Observable<AutomationRuleDto> {
    return this.http.post<AutomationRuleDto>(this.rulesUrl, dto);
  }

  update(id: number, dto: UpdateAutomationRuleDto): Observable<AutomationRuleDto> {
    return this.http.put<AutomationRuleDto>(`${this.rulesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.rulesUrl}/${id}`);
  }
}
