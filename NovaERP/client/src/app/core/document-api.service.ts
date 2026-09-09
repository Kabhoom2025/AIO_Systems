import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DocumentDto {
  id: number;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  entityType?: string | null;
  entityId?: number | null;
  uploadedByUserId: number;
  uploadedDate: string;
}

@Injectable({ providedIn: 'root' })
export class DocumentApiService {
  private documentsUrl = `${environment.apiUrl}/documents`;

  constructor(private http: HttpClient) {}

  getAll(entityType?: string, entityId?: number): Observable<DocumentDto[]> {
    let params: Record<string, string> = {};
    if (entityType) params['entityType'] = entityType;
    if (entityId != null) params['entityId'] = String(entityId);
    return this.http.get<DocumentDto[]>(this.documentsUrl, { params });
  }

  upload(file: File, entityType?: string, entityId?: number): Observable<DocumentDto> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    if (entityType) formData.append('entityType', entityType);
    if (entityId != null) formData.append('entityId', String(entityId));
    return this.http.post<DocumentDto>(this.documentsUrl, formData);
  }

  download(id: number): Observable<Blob> {
    return this.http.get(`${this.documentsUrl}/${id}/download`, { responseType: 'blob' });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.documentsUrl}/${id}`);
  }
}
