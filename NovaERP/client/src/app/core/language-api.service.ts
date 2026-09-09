import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LanguageDto {
  id: number;
  code: string;
  name: string;
  nativeName: string;
  isRtl: boolean;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class LanguageApiService {
  private languagesUrl = `${environment.apiUrl}/languages`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<LanguageDto[]> {
    return this.http.get<LanguageDto[]>(this.languagesUrl);
  }
}
