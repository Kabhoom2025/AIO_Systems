import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DeliveryLoadShipmentDto {
  shipmentId: number;
  shipmentNumber: string;
  referenceLabel: string | null;
  shipToCity: string | null;
  totalWeightKg: number;
  lineCount: number;
}

export interface DeliveryLoadDto {
  id: number;
  loadNumber: string;
  vehicleId: number;
  vehicleCode: string;
  vehicleName: string;
  vehicleCapacityKg: number;
  warehouseId: number;
  warehouseName: string;
  loadDate: string;
  dispatchedDate: string | null;
  totalWeightKg: number;
  shipments: DeliveryLoadShipmentDto[];
}

export interface CreateDeliveryLoadDto {
  vehicleId: number;
  warehouseId: number;
}

@Injectable({ providedIn: 'root' })
export class DeliveryLoadApiService {
  private loadsUrl = `${environment.apiUrl}/delivery-loads`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<DeliveryLoadDto[]> {
    return this.http.get<DeliveryLoadDto[]>(this.loadsUrl);
  }

  getById(id: number): Observable<DeliveryLoadDto> {
    return this.http.get<DeliveryLoadDto>(`${this.loadsUrl}/${id}`);
  }

  getAvailableShipments(warehouseId: number): Observable<DeliveryLoadShipmentDto[]> {
    return this.http.get<DeliveryLoadShipmentDto[]>(`${this.loadsUrl}/available-shipments`, {
      params: { warehouseId }
    });
  }

  create(dto: CreateDeliveryLoadDto): Observable<DeliveryLoadDto> {
    return this.http.post<DeliveryLoadDto>(this.loadsUrl, dto);
  }

  assignShipment(loadId: number, shipmentId: number): Observable<DeliveryLoadDto> {
    return this.http.post<DeliveryLoadDto>(`${this.loadsUrl}/${loadId}/assign-shipment`, { shipmentId });
  }

  unassignShipment(loadId: number, shipmentId: number): Observable<DeliveryLoadDto> {
    return this.http.post<DeliveryLoadDto>(`${this.loadsUrl}/${loadId}/unassign-shipment/${shipmentId}`, {});
  }

  dispatch(loadId: number): Observable<DeliveryLoadDto> {
    return this.http.post<DeliveryLoadDto>(`${this.loadsUrl}/${loadId}/dispatch`, {});
  }
}
