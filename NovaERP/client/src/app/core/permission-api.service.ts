import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PermissionDto {
  id: number;
  key: string;
  module: string;
  description: string;
}

@Injectable({ providedIn: 'root' })
export class PermissionApiService {
  private permissionsUrl = `${environment.apiUrl}/permissions`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<PermissionDto[]> {
    return this.http.get<PermissionDto[]>(this.permissionsUrl);
  }
}
