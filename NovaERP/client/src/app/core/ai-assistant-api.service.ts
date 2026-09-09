import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MessageDto {
  id: number;
  role: string;
  content: string;
  createdDate: string;
  pendingActionStatus?: string | null;
}

export interface ConversationSummaryDto {
  id: number;
  title: string;
  createdDate: string;
}

export interface ConversationDto {
  id: number;
  title: string;
  messages: MessageDto[];
}

export interface CreateConversationDto {
  title: string;
}

export interface SendMessageDto {
  content: string;
}

@Injectable({ providedIn: 'root' })
export class AiAssistantApiService {
  private assistantUrl = `${environment.apiUrl}/my-assistant`;

  constructor(private http: HttpClient) {}

  getConversations(): Observable<ConversationSummaryDto[]> {
    return this.http.get<ConversationSummaryDto[]>(this.assistantUrl);
  }

  getConversation(id: number): Observable<ConversationDto> {
    return this.http.get<ConversationDto>(`${this.assistantUrl}/${id}`);
  }

  createConversation(dto: CreateConversationDto): Observable<ConversationDto> {
    return this.http.post<ConversationDto>(this.assistantUrl, dto);
  }

  deleteConversation(id: number): Observable<void> {
    return this.http.delete<void>(`${this.assistantUrl}/${id}`);
  }

  sendMessage(id: number, dto: SendMessageDto): Observable<ConversationDto> {
    return this.http.post<ConversationDto>(`${this.assistantUrl}/${id}/messages`, dto);
  }

  confirmAction(conversationId: number, messageId: number): Observable<ConversationDto> {
    return this.http.post<ConversationDto>(`${this.assistantUrl}/${conversationId}/messages/${messageId}/confirm-action`, {});
  }

  cancelAction(conversationId: number, messageId: number): Observable<ConversationDto> {
    return this.http.post<ConversationDto>(`${this.assistantUrl}/${conversationId}/messages/${messageId}/cancel-action`, {});
  }
}
