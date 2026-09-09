import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface FieldMappingDto {
  id: number;
  direction: 'Outbound' | 'Inbound';
  novaField: string;
  externalPath: string;
  transform: string;
  constantValue?: string | null;
}

export interface UpsertFieldMappingDto {
  id?: number | null;
  direction: 'Outbound' | 'Inbound';
  novaField: string;
  externalPath: string;
  transform: string;
  constantValue?: string | null;
}

export interface ShippingConnectorDto {
  id: number;
  name: string;
  tmsType: string;
  baseUrl: string;
  httpMethod: string;
  requestContentType: string;
  responseFormat: string;
  authType: string;
  authHeaderName: string | null;
  authUsername: string | null;
  authTokenUrl: string | null;
  authTokenResponsePath: string | null;
  hasAuthApiKey: boolean;
  hasAuthPassword: boolean;
  isActive: boolean;
  sampleRequestPayload: string | null;
  sampleResponsePayload: string | null;
  fieldMappings: FieldMappingDto[];
}

export interface SaveShippingConnectorDto {
  name: string;
  tmsType: string;
  baseUrl: string;
  httpMethod: string;
  requestContentType: string;
  responseFormat: string;
  authType: string;
  authHeaderName: string | null;
  authApiKey: string | null;
  authUsername: string | null;
  authPassword: string | null;
  authTokenUrl: string | null;
  authTokenResponsePath: string | null;
  isActive: boolean;
  sampleRequestPayload: string | null;
  sampleResponsePayload: string | null;
  fieldMappings: UpsertFieldMappingDto[];
}

export interface RateSurchargeDto {
  name: string | null;
  amount: number | null;
}

export interface RateQuoteDto {
  connectorId: number;
  connectorName: string;
  carrierName: string | null;
  serviceLevel: string | null;
  publishedCost: number | null;
  price: number | null;
  currency: string | null;
  estimatedDays: number | null;
  etd: string | null;
  rateId: string | null;
  surcharges: RateSurchargeDto[];
}

export interface RateQuoteResultDto {
  quotes: RateQuoteDto[];
  errors: string[];
}

export interface TestShippingConnectorDto {
  connector: SaveShippingConnectorDto;
  requestBody: string | null;
}

export interface ConnectorTestResultDto {
  success: boolean;
  statusCode: number | null;
  responseBody: string | null;
  error: string | null;
  elapsedMs: number;
}

@Injectable({ providedIn: 'root' })
export class ShippingConnectorApiService {
  private connectorsUrl = `${environment.apiUrl}/shipping-connectors`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ShippingConnectorDto[]> {
    return this.http.get<ShippingConnectorDto[]>(this.connectorsUrl);
  }

  getById(id: number): Observable<ShippingConnectorDto> {
    return this.http.get<ShippingConnectorDto>(`${this.connectorsUrl}/${id}`);
  }

  create(dto: SaveShippingConnectorDto): Observable<ShippingConnectorDto> {
    return this.http.post<ShippingConnectorDto>(this.connectorsUrl, dto);
  }

  update(id: number, dto: SaveShippingConnectorDto): Observable<ShippingConnectorDto> {
    return this.http.put<ShippingConnectorDto>(`${this.connectorsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.connectorsUrl}/${id}`);
  }

  getRatesForShipment(shipmentId: number): Observable<RateQuoteResultDto> {
    return this.http.get<RateQuoteResultDto>(`${environment.apiUrl}/shipments/${shipmentId}/rates`);
  }

  /// Postman-style "Send" — tries the draft form live without requiring Save first. Passing
  /// connectorId lets an existing connector's blank (unchanged) secret fields still be used.
  test(connectorId: number | null, dto: TestShippingConnectorDto): Observable<ConnectorTestResultDto> {
    const url = connectorId ? `${this.connectorsUrl}/${connectorId}/test` : `${this.connectorsUrl}/test`;
    return this.http.post<ConnectorTestResultDto>(url, dto);
  }
}
