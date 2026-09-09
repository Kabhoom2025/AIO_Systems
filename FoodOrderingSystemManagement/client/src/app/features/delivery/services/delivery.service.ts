import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  DriverDto, CreateDriverDto, UpdateDriverDto,
  DeliveryOrderDto, CreateDeliveryOrderDto, AssignDriverDto, UpdateDeliveryStatusDto,
  DeliveryChargeSlabDto, UpsertDeliveryChargeSlabDto,
  ThirdPartyConfigDto, UpsertThirdPartyConfigDto,
  DeliveryDashboardDto,
} from '../models/delivery.model';

@Injectable({ providedIn: 'root' })
export class DeliveryService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/delivery`;

  private extract<T>(obs: Observable<{ data: T }>): Observable<T> {
    return obs.pipe(map(r => r.data));
  }

  // Dashboard
  getDashboard(): Observable<DeliveryDashboardDto> {
    return this.extract(this.http.get<any>(`${this.base}/dashboard`));
  }

  // Drivers
  getDrivers(): Observable<DriverDto[]> {
    return this.extract(this.http.get<any>(`${this.base}/drivers`));
  }
  createDriver(dto: CreateDriverDto): Observable<DriverDto> {
    return this.extract(this.http.post<any>(`${this.base}/drivers`, dto));
  }
  updateDriver(id: number, dto: UpdateDriverDto): Observable<DriverDto> {
    return this.extract(this.http.put<any>(`${this.base}/drivers/${id}`, dto));
  }
  deleteDriver(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/drivers/${id}`);
  }

  // Delivery Orders
  getOrders(date?: Date): Observable<DeliveryOrderDto[]> {
    let params = new HttpParams();
    if (date) params = params.set('date', date.toISOString().split('T')[0]);
    return this.extract(this.http.get<any>(`${this.base}/orders`, { params }));
  }
  getOrder(id: number): Observable<DeliveryOrderDto> {
    return this.extract(this.http.get<any>(`${this.base}/orders/${id}`));
  }
  createOrder(dto: CreateDeliveryOrderDto): Observable<DeliveryOrderDto> {
    return this.extract(this.http.post<any>(`${this.base}/orders`, dto));
  }
  assignDriver(id: number, dto: AssignDriverDto): Observable<DeliveryOrderDto> {
    return this.extract(this.http.patch<any>(`${this.base}/orders/${id}/assign`, dto));
  }
  updateStatus(id: number, dto: UpdateDeliveryStatusDto): Observable<DeliveryOrderDto> {
    return this.extract(this.http.patch<any>(`${this.base}/orders/${id}/status`, dto));
  }

  // Charge Slabs
  getChargeSlabs(): Observable<DeliveryChargeSlabDto[]> {
    return this.extract(this.http.get<any>(`${this.base}/charges`));
  }
  addChargeSlab(dto: UpsertDeliveryChargeSlabDto): Observable<DeliveryChargeSlabDto> {
    return this.extract(this.http.post<any>(`${this.base}/charges`, dto));
  }
  deleteChargeSlab(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/charges/${id}`);
  }

  // Dispatch to 3rd party
  dispatchToThirdParty(deliveryId: number, provider: string): Observable<DeliveryOrderDto> {
    return this.extract(this.http.post<any>(`${this.base}/orders/${deliveryId}/dispatch`, { provider }));
  }

  // Third Party
  getThirdPartyConfigs(): Observable<ThirdPartyConfigDto[]> {
    return this.extract(this.http.get<any>(`${this.base}/third-party`));
  }
  upsertThirdPartyConfig(dto: UpsertThirdPartyConfigDto): Observable<ThirdPartyConfigDto> {
    return this.extract(this.http.post<any>(`${this.base}/third-party`, dto));
  }
}
