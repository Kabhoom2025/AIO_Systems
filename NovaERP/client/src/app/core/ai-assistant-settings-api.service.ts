import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export type AiProvider = 'Anthropic' | 'Groq';

export interface AiAssistantSettingsDto {
  organizationId: number;
  provider: AiProvider;
  anthropicModel?: string | null;
  groqModel?: string | null;
  isConfigured: boolean;
}

export interface UpdateAiAssistantSettingsDto {
  provider: AiProvider;
  anthropicApiKey?: string | null;
  anthropicModel?: string | null;
  groqApiKey?: string | null;
  groqModel?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AiAssistantSettingsApiService {
  private settingsUrl = `${environment.apiUrl}/ai-assistant-settings`;

  constructor(private http: HttpClient) {}

  get(): Observable<AiAssistantSettingsDto> {
    return this.http.get<AiAssistantSettingsDto>(this.settingsUrl);
  }

  update(dto: UpdateAiAssistantSettingsDto): Observable<AiAssistantSettingsDto> {
    return this.http.put<AiAssistantSettingsDto>(this.settingsUrl, dto);
  }
}
