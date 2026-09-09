import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DashboardWidgetDto {
  id: number;
  widgetType: string;
  title: string;
  sizeOption: string;
  displayOrder: number;
}

export interface DashboardDto {
  id: number;
  name: string;
  isDefault: boolean;
  widgets: DashboardWidgetDto[];
}

export interface CreateDashboardDto {
  name: string;
  isDefault: boolean;
}

export interface UpdateDashboardDto {
  name: string;
  isDefault: boolean;
}

export interface AddWidgetDto {
  widgetType: string;
  title: string;
  sizeOption: string;
}

export interface WidgetLayoutEntryDto {
  widgetId: number;
  displayOrder: number;
  sizeOption: string;
}

export interface ReorderWidgetsDto {
  widgets: WidgetLayoutEntryDto[];
}

export interface WidgetDataDto {
  labels: string[];
  values: number[];
  total: number | null;
}

@Injectable({ providedIn: 'root' })
export class DashboardBuilderApiService {
  private dashboardsUrl = `${environment.apiUrl}/my-dashboards`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<DashboardDto[]> {
    return this.http.get<DashboardDto[]>(this.dashboardsUrl);
  }

  getById(id: number): Observable<DashboardDto> {
    return this.http.get<DashboardDto>(`${this.dashboardsUrl}/${id}`);
  }

  create(dto: CreateDashboardDto): Observable<DashboardDto> {
    return this.http.post<DashboardDto>(this.dashboardsUrl, dto);
  }

  update(id: number, dto: UpdateDashboardDto): Observable<DashboardDto> {
    return this.http.put<DashboardDto>(`${this.dashboardsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.dashboardsUrl}/${id}`);
  }

  addWidget(dashboardId: number, dto: AddWidgetDto): Observable<DashboardDto> {
    return this.http.post<DashboardDto>(`${this.dashboardsUrl}/${dashboardId}/widgets`, dto);
  }

  removeWidget(dashboardId: number, widgetId: number): Observable<DashboardDto> {
    return this.http.delete<DashboardDto>(`${this.dashboardsUrl}/${dashboardId}/widgets/${widgetId}`);
  }

  reorderWidgets(dashboardId: number, dto: ReorderWidgetsDto): Observable<DashboardDto> {
    return this.http.put<DashboardDto>(`${this.dashboardsUrl}/${dashboardId}/widgets/layout`, dto);
  }

  getWidgetData(widgetType: string): Observable<WidgetDataDto> {
    return this.http.get<WidgetDataDto>(`${this.dashboardsUrl}/widget-data/${widgetType}`);
  }
}
