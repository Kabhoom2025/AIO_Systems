import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { SystemHealthDto } from '../models/system-health.model';

@Injectable({ providedIn: 'root' })
export class SystemHealthService {
  private readonly url = `${environment.apiUrl}/system-health`;

  constructor(private http: HttpClient) {}

  get(): Observable<SystemHealthDto> {
    return this.http.get<ApiResponse<SystemHealthDto>>(this.url).pipe(
      map(res => res.data!)
    );
  }
}
