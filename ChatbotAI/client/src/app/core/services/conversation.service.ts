import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Conversation, ConversationDetail } from '../models/conversation.model';

@Injectable({ providedIn: 'root' })
export class ConversationService {
  private readonly baseUrl = `${environment.apiBaseUrl}/conversations`;

  constructor(private readonly http: HttpClient) {}

  getAll(search?: string): Observable<Conversation[]> {
    const params = search ? { search } : undefined;
    return this.http.get<Conversation[]>(this.baseUrl, { params });
  }

  getDetail(id: string): Observable<ConversationDetail> {
    return this.http.get<ConversationDetail>(`${this.baseUrl}/${id}`);
  }

  create(title?: string): Observable<Conversation> {
    return this.http.post<Conversation>(this.baseUrl, { title: title ?? null });
  }

  rename(id: string, title: string): Observable<Conversation> {
    return this.http.put<Conversation>(`${this.baseUrl}/${id}`, { title });
  }

  archive(id: string, isArchived: boolean): Observable<Conversation> {
    return this.http.put<Conversation>(`${this.baseUrl}/${id}`, { isArchived });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  deleteMessage(messageId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiBaseUrl}/messages/${messageId}`);
  }
}
