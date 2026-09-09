import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ShipmentLineDto {
  id: number;
  productId: number;
  productName: string;
  productSku: string;
  quantity: number;
  salesOrderLineId: number | null;
  displayOrder: number;
}

export interface CreateShipmentLineDto {
  productId: number;
  quantity: number;
  salesOrderLineId: number | null;
  displayOrder: number;
}

export interface ShipmentPackageItemDto {
  productId: number;
  productName: string;
  productSku: string;
  quantity: number;
}

export interface CreateShipmentPackageItemDto {
  productId: number;
  quantity: number;
}

export interface ShipmentPackageDto {
  id: number;
  packageNumber: number;
  weightKg: number | null;
  lengthCm: number | null;
  widthCm: number | null;
  heightCm: number | null;
  trackingNumber: string | null;
  items: ShipmentPackageItemDto[];
}

export interface CreateShipmentPackageDto {
  packageNumber: number;
  weightKg: number | null;
  lengthCm: number | null;
  widthCm: number | null;
  heightCm: number | null;
  trackingNumber: string | null;
  items: CreateShipmentPackageItemDto[];
}

export interface ShipmentDto {
  id: number;
  shipmentNumber: string;
  warehouseId: number;
  warehouseName: string;
  sourceType: string;
  salesOrderId: number | null;
  salesOrderNumber: string | null;
  destinationWarehouseId: number | null;
  destinationWarehouseName: string | null;

  shipToName: string | null;
  shipToContactName: string | null;
  shipToEmail: string | null;
  shipToAddressLine1: string | null;
  shipToAddressLine2: string | null;
  shipToCity: string | null;
  shipToState: string | null;
  shipToPostalCode: string | null;
  shipToCountry: string | null;
  shipToPhone: string | null;
  shipToTaxType: string | null;
  shipToTaxCountry: string | null;
  shipToTaxId: string | null;

  shipFromName: string | null;
  shipFromContactName: string | null;
  shipFromEmail: string | null;
  shipFromAddressLine1: string | null;
  shipFromAddressLine2: string | null;
  shipFromCity: string | null;
  shipFromState: string | null;
  shipFromPostalCode: string | null;
  shipFromCountry: string | null;
  shipFromPhone: string | null;

  shipDate: string;
  carrier: string | null;
  trackingNumber: string | null;
  isBlindShipment: boolean;
  status: string;
  ownerId: number;
  ownerName: string;

  lines: ShipmentLineDto[];
  packages: ShipmentPackageDto[];
}

export interface CreateShipmentDto {
  warehouseId: number;
  sourceType: string;
  salesOrderId: number | null;
  destinationWarehouseId: number | null;

  shipToName: string | null;
  shipToContactName: string | null;
  shipToEmail: string | null;
  shipToAddressLine1: string | null;
  shipToAddressLine2: string | null;
  shipToCity: string | null;
  shipToState: string | null;
  shipToPostalCode: string | null;
  shipToCountry: string | null;
  shipToPhone: string | null;
  shipToTaxType: string | null;
  shipToTaxCountry: string | null;
  shipToTaxId: string | null;

  shipFromName: string | null;
  shipFromContactName: string | null;
  shipFromEmail: string | null;
  shipFromAddressLine1: string | null;
  shipFromAddressLine2: string | null;
  shipFromCity: string | null;
  shipFromState: string | null;
  shipFromPostalCode: string | null;
  shipFromCountry: string | null;
  shipFromPhone: string | null;

  shipDate: string;
  carrier: string | null;
  trackingNumber: string | null;
  isBlindShipment: boolean;
  ownerId: number;

  lines: CreateShipmentLineDto[];
  packages: CreateShipmentPackageDto[];
}

export interface UpdateShipmentDto {
  warehouseId: number;
  shipToName: string | null;
  shipToContactName: string | null;
  shipToEmail: string | null;
  shipToAddressLine1: string | null;
  shipToAddressLine2: string | null;
  shipToCity: string | null;
  shipToState: string | null;
  shipToPostalCode: string | null;
  shipToCountry: string | null;
  shipToPhone: string | null;
  shipToTaxType: string | null;
  shipToTaxCountry: string | null;
  shipToTaxId: string | null;

  shipFromName: string | null;
  shipFromContactName: string | null;
  shipFromEmail: string | null;
  shipFromAddressLine1: string | null;
  shipFromAddressLine2: string | null;
  shipFromCity: string | null;
  shipFromState: string | null;
  shipFromPostalCode: string | null;
  shipFromCountry: string | null;
  shipFromPhone: string | null;

  shipDate: string;
  carrier: string | null;
  trackingNumber: string | null;
  isBlindShipment: boolean;
  ownerId: number;

  lines: CreateShipmentLineDto[];
  packages: CreateShipmentPackageDto[];
}

@Injectable({ providedIn: 'root' })
export class ShipmentApiService {
  private shipmentsUrl = `${environment.apiUrl}/shipments`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ShipmentDto[]> {
    return this.http.get<ShipmentDto[]>(this.shipmentsUrl);
  }

  getById(id: number): Observable<ShipmentDto> {
    return this.http.get<ShipmentDto>(`${this.shipmentsUrl}/${id}`);
  }

  create(dto: CreateShipmentDto): Observable<ShipmentDto> {
    return this.http.post<ShipmentDto>(this.shipmentsUrl, dto);
  }

  update(id: number, dto: UpdateShipmentDto): Observable<ShipmentDto> {
    return this.http.put<ShipmentDto>(`${this.shipmentsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.shipmentsUrl}/${id}`);
  }

  pick(id: number): Observable<ShipmentDto> {
    return this.http.post<ShipmentDto>(`${this.shipmentsUrl}/${id}/pick`, {});
  }

  ship(id: number): Observable<ShipmentDto> {
    return this.http.post<ShipmentDto>(`${this.shipmentsUrl}/${id}/ship`, {});
  }

  deliver(id: number): Observable<ShipmentDto> {
    return this.http.post<ShipmentDto>(`${this.shipmentsUrl}/${id}/deliver`, {});
  }

  cancel(id: number): Observable<ShipmentDto> {
    return this.http.post<ShipmentDto>(`${this.shipmentsUrl}/${id}/cancel`, {});
  }
}
