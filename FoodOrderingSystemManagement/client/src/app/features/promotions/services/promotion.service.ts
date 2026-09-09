import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  PromotionDto, CreatePromotionDto,
  ApplyPromoCodeDto, ApplyPromotionResultDto,
} from '../models/promotion.model';

@Injectable({ providedIn: 'root' })
export class PromotionService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/promotion`;

  private extract<T>(obs: Observable<{ data: T }>): Observable<T> {
    return obs.pipe(map(r => r.data));
  }

  getAll(): Observable<PromotionDto[]> {
    return this.extract(this.http.get<any>(this.base));
  }
  getById(id: number): Observable<PromotionDto> {
    return this.extract(this.http.get<any>(`${this.base}/${id}`));
  }
  getActive(): Observable<PromotionDto[]> {
    return this.extract(this.http.get<any>(`${this.base}/active`));
  }
  getPublicActive(): Observable<PromotionDto[]> {
    return this.extract(this.http.get<any>(`${this.base}/public/active`));
  }
  create(dto: CreatePromotionDto): Observable<PromotionDto> {
    return this.extract(this.http.post<any>(this.base, dto));
  }
  update(id: number, dto: CreatePromotionDto): Observable<PromotionDto> {
    return this.extract(this.http.put<any>(`${this.base}/${id}`, dto));
  }
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
  toggle(id: number): Observable<{ isActive: boolean }> {
    return this.extract(this.http.patch<any>(`${this.base}/${id}/toggle`, {}));
  }
  applyCode(dto: ApplyPromoCodeDto): Observable<ApplyPromotionResultDto> {
    return this.extract(this.http.post<any>(`${this.base}/apply`, dto));
  }
}
