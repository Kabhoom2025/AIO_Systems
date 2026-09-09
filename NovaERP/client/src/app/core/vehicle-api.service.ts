import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface VehicleDto {
  id: number;
  code: string;
  name: string;
  model: string | null;
  capacityKg: number;
  status: string;
  currentWarehouseId: number | null;
  currentWarehouseName: string | null;
  isActive: boolean;
}

export interface CreateVehicleDto {
  code: string;
  name: string;
  model: string | null;
  capacityKg: number;
  currentWarehouseId: number | null;
  isActive: boolean;
}

export interface UpdateVehicleDto {
  name: string;
  model: string | null;
  capacityKg: number;
  currentWarehouseId: number | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class VehicleApiService {
  private vehiclesUrl = `${environment.apiUrl}/vehicles`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<VehicleDto[]> {
    return this.http.get<VehicleDto[]>(this.vehiclesUrl);
  }

  getById(id: number): Observable<VehicleDto> {
    return this.http.get<VehicleDto>(`${this.vehiclesUrl}/${id}`);
  }

  create(dto: CreateVehicleDto): Observable<VehicleDto> {
    return this.http.post<VehicleDto>(this.vehiclesUrl, dto);
  }

  update(id: number, dto: UpdateVehicleDto): Observable<VehicleDto> {
    return this.http.put<VehicleDto>(`${this.vehiclesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.vehiclesUrl}/${id}`);
  }
}
