import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { PasswordModule } from 'primeng/password';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { TabViewModule } from 'primeng/tabview';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { NotificationService } from '../../core/notification.service';
import {
  ShippingConnectorApiService, SaveShippingConnectorDto, UpsertFieldMappingDto, ConnectorTestResultDto
} from '../../core/shipping-connector-api.service';
import { DetectedField, tryFlattenJsonPaths, tryFlattenXmlPaths, tryFlattenFormPaths } from '../../core/json-path-utils';

// Grouped so the dropdown makes the Ship From (the Warehouse the shipment originates from) /
// Ship To (the Shipment's own ShipTo* address) / Package split obvious at a glance — the
// underlying NovaField values are unchanged, only the dropdown's grouping/labels are new.
const OUTBOUND_NOVA_FIELD_GROUPS = [
  {
    label: 'Ship From (Warehouse)',
    items: [
      { label: 'Ship From — Location Code', value: 'Warehouse.Code' },
      { label: 'Ship From — Company', value: 'Warehouse.Name' },
      { label: 'Ship From — Contact', value: 'Warehouse.ContactName' },
      { label: 'Ship From — Email', value: 'Warehouse.Email' },
      { label: 'Ship From — Phone', value: 'Warehouse.Phone' },
      { label: 'Ship From — Address Line 1', value: 'Warehouse.Address' },
      { label: 'Ship From — Address Line 2', value: 'Warehouse.AddressLine2' },
      { label: 'Ship From — City', value: 'Warehouse.City' },
      { label: 'Ship From — State', value: 'Warehouse.State' },
      { label: 'Ship From — Postal Code', value: 'Warehouse.PostalCode' },
      { label: 'Ship From — Country', value: 'Warehouse.Country' },
      { label: 'Ship From — Tax Type', value: 'Warehouse.TaxType' },
      { label: 'Ship From — Tax Country', value: 'Warehouse.TaxCountry' },
      { label: 'Ship From — Tax ID', value: 'Warehouse.TaxId' }
    ]
  },
  {
    label: 'Ship To (Shipment)',
    items: [
      { label: 'Ship To — Company', value: 'Shipment.ShipToName' },
      { label: 'Ship To — Contact', value: 'Shipment.ShipToContactName' },
      { label: 'Ship To — Email', value: 'Shipment.ShipToEmail' },
      { label: 'Ship To — Address Line 1', value: 'Shipment.ShipToAddressLine1' },
      { label: 'Ship To — Address Line 2', value: 'Shipment.ShipToAddressLine2' },
      { label: 'Ship To — City', value: 'Shipment.ShipToCity' },
      { label: 'Ship To — State', value: 'Shipment.ShipToState' },
      { label: 'Ship To — Postal Code', value: 'Shipment.ShipToPostalCode' },
      { label: 'Ship To — Country', value: 'Shipment.ShipToCountry' },
      { label: 'Ship To — Phone', value: 'Shipment.ShipToPhone' },
      { label: 'Ship To — Tax Type', value: 'Shipment.ShipToTaxType' },
      { label: 'Ship To — Tax Country', value: 'Shipment.ShipToTaxCountry' },
      { label: 'Ship To — Tax ID', value: 'Shipment.ShipToTaxId' }
    ]
  },
  {
    label: 'Package',
    items: [
      { label: 'Package — Weight (kg)', value: 'Package.WeightKg' },
      { label: 'Package — Length (cm)', value: 'Package.LengthCm' },
      { label: 'Package — Width (cm)', value: 'Package.WidthCm' },
      { label: 'Package — Height (cm)', value: 'Package.HeightCm' }
    ]
  },
  {
    label: 'Shipment (General)',
    items: [
      { label: 'Ship Date', value: 'Shipment.ShipDate' }
    ]
  },
  {
    // Not shipment data at all — a literal value you type in (e.g. a TMS-specific location/
    // account code), for required fields their API expects that NovaERP has no concept of.
    label: 'Constant / Fixed Value',
    items: [
      { label: 'Enter a fixed value…', value: 'Constant' }
    ]
  }
];

const OUTBOUND_NOVA_FIELDS = OUTBOUND_NOVA_FIELD_GROUPS.flatMap(g => g.items.map(i => i.value));

const INBOUND_NOVA_FIELDS = [
  'Quote.CarrierName', 'Quote.ServiceLevel', 'Quote.PublishedCost', 'Quote.Price', 'Quote.Currency',
  'Quote.EstimatedDays', 'Quote.Etd', 'Quote.RateId',
  // Map both against the same-shaped sub-array under each quote (e.g. "quotes[].surcharges[].name"
  // / "...amount") — never drives row count itself, one Surcharge row is built per matching pair.
  'Quote.SurchargeName', 'Quote.SurchargeAmount'
];

const TRANSFORMS = ['None', 'KgToLb', 'LbToKg', 'CmToIn', 'InToCm', 'Uppercase', 'Lowercase', 'DateAppend0900', 'DateFormatMDY'];

@Component({
  selector: 'app-shipping-connector-edit',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ButtonModule, InputTextModule, TextareaModule,
    PasswordModule, DropdownModule, CheckboxModule, TabViewModule, TooltipModule, ToastModule
  ],
  templateUrl: './shipping-connector-edit.component.html',
  styleUrl: './shipping-connector-edit.component.scss'
})
export class ShippingConnectorEditComponent implements OnInit {
  connectorId: number | null = null;
  loading = false;
  saving = false;

  outboundNovaFields = OUTBOUND_NOVA_FIELDS;
  outboundNovaFieldGroups = OUTBOUND_NOVA_FIELD_GROUPS;
  inboundNovaFields = INBOUND_NOVA_FIELDS;
  transforms = TRANSFORMS;
  httpMethods = ['POST', 'GET'];
  authTypes = ['None', 'ApiKeyHeader', 'BearerToken', 'BasicAuth', 'TokenLogin'];
  requestContentTypes = ['Json', 'FormUrlEncoded', 'Xml', 'Text'];
  responseFormats = ['Json', 'Xml'];

  hasAuthApiKey = false;
  hasAuthPassword = false;

  requestFieldOptions: DetectedField[] = [];
  responseFieldOptions: DetectedField[] = [];

  form: SaveShippingConnectorDto = this.emptyForm();

  // Postman-style live "Send" tab state — independent of the Save form so trying requests
  // never risks losing unsaved mapping work.
  testRequestBody = '';
  sending = false;
  testResult: ConnectorTestResultDto | null = null;

  constructor(
    private api: ShippingConnectorApiService,
    private notify: NotificationService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam && idParam !== 'new') {
      this.connectorId = Number(idParam);
      this.load();
    }
  }

  private emptyForm(): SaveShippingConnectorDto {
    return {
      name: '', tmsType: '', baseUrl: '', httpMethod: 'POST',
      requestContentType: 'Json', responseFormat: 'Json', authType: 'None',
      authHeaderName: null, authApiKey: null, authUsername: null, authPassword: null,
      authTokenUrl: null, authTokenResponsePath: null,
      isActive: true, sampleRequestPayload: null, sampleResponsePayload: null,
      fieldMappings: []
    };
  }

  private load() {
    if (!this.connectorId) return;
    this.loading = true;
    this.api.getById(this.connectorId).subscribe({
      next: c => {
        this.form = {
          name: c.name, tmsType: c.tmsType, baseUrl: c.baseUrl, httpMethod: c.httpMethod,
          requestContentType: c.requestContentType, responseFormat: c.responseFormat,
          authType: c.authType, authHeaderName: c.authHeaderName, authApiKey: '',
          authUsername: c.authUsername, authPassword: '',
          authTokenUrl: c.authTokenUrl, authTokenResponsePath: c.authTokenResponsePath,
          isActive: c.isActive, sampleRequestPayload: c.sampleRequestPayload,
          sampleResponsePayload: c.sampleResponsePayload,
          fieldMappings: c.fieldMappings.map(m => ({
            id: m.id, direction: m.direction, novaField: m.novaField,
            externalPath: m.externalPath, transform: m.transform, constantValue: m.constantValue
          }))
        };
        this.hasAuthApiKey = c.hasAuthApiKey;
        this.hasAuthPassword = c.hasAuthPassword;
        this.testRequestBody = c.sampleRequestPayload ?? '';
        this.detectRequestFields();
        this.detectResponseFields();
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load connector.');
      }
    });
  }

  get outboundMappings(): UpsertFieldMappingDto[] {
    return this.form.fieldMappings.filter(m => m.direction === 'Outbound');
  }

  get inboundMappings(): UpsertFieldMappingDto[] {
    return this.form.fieldMappings.filter(m => m.direction === 'Inbound');
  }

  /// Outbound Mapping is only actually applied when building the real request for
  /// RequestContentType Json/FormUrlEncoded — Xml/Text send the Sample Request Payload
  /// verbatim, so field detection (and mapping) doesn't apply to those two.
  get outboundMappingSupported(): boolean {
    return this.form.requestContentType === 'Json' || this.form.requestContentType === 'FormUrlEncoded';
  }

  detectRequestFields() {
    this.requestFieldOptions = this.form.requestContentType === 'FormUrlEncoded'
      ? tryFlattenFormPaths(this.form.sampleRequestPayload ?? '')
      : tryFlattenJsonPaths(this.form.sampleRequestPayload ?? '');
    if (this.requestFieldOptions.length) {
      this.notify.success(`Detected ${this.requestFieldOptions.length} fields — see the list below, or pick one from the External Path dropdown on a mapping row.`);
    } else {
      this.notify.warn(this.form.requestContentType === 'FormUrlEncoded'
        ? 'Could not detect any fields — paste a "key=value&key2=value2" sample.'
        : 'Could not detect any fields — check the payload is valid JSON.');
    }
  }

  detectResponseFields() {
    this.responseFieldOptions = this.form.responseFormat === 'Xml'
      ? tryFlattenXmlPaths(this.form.sampleResponsePayload ?? '')
      : tryFlattenJsonPaths(this.form.sampleResponsePayload ?? '');
    if (this.responseFieldOptions.length) {
      this.notify.success(`Detected ${this.responseFieldOptions.length} fields — see the list below, or pick one from the External Path dropdown on a mapping row.`);
    } else {
      this.notify.warn(this.form.responseFormat === 'Xml'
        ? 'Could not detect any fields — check the payload is valid XML.'
        : 'Could not detect any fields — check the payload is valid JSON.');
    }
  }

  addMapping(direction: 'Outbound' | 'Inbound') {
    this.form.fieldMappings.push({
      direction,
      novaField: direction === 'Outbound' ? this.outboundNovaFields[0] : this.inboundNovaFields[0],
      externalPath: '',
      transform: 'None'
    });
  }

  /// Clicking a detected field chip is a shortcut for "Add Mapping" pre-filled with that path,
  /// so you don't have to add a blank row first just to see what was detected.
  quickAddFromDetectedField(direction: 'Outbound' | 'Inbound', field: DetectedField) {
    this.form.fieldMappings.push({
      direction,
      novaField: direction === 'Outbound' ? this.outboundNovaFields[0] : this.inboundNovaFields[0],
      externalPath: field.path,
      transform: 'None'
    });
  }

  /// Shortcut for the common "this carrier's response has no field for X, just hardcode a
  /// value" case (e.g. Quote.CarrierName when the API never returns a carrier name) — adds a
  /// row with External Path already set to the "Constant" sentinel so the Constant Value input
  /// appears immediately instead of the user having to type "Constant" by hand.
  addConstantInboundMapping() {
    this.form.fieldMappings.push({
      direction: 'Inbound',
      novaField: this.inboundNovaFields[0],
      externalPath: 'Constant',
      transform: 'None',
      constantValue: ''
    });
  }

  removeMapping(mapping: UpsertFieldMappingDto) {
    this.form.fieldMappings = this.form.fieldMappings.filter(m => m !== mapping);
  }

  bodyPlaceholder(): string {
    switch (this.form.requestContentType) {
      case 'FormUrlEncoded': return 'key1=value1&key2=value2… (sent verbatim as application/x-www-form-urlencoded)';
      case 'Xml': return '<request>…</request> (sent verbatim as application/xml)';
      case 'Text': return 'Raw text body to send…';
      default: return 'JSON request body to send…';
    }
  }

  /// Copies the Outbound Mapping tab's Sample Request Payload into the Send tab's editable body
  /// — a shortcut, since that's usually the starting point for a real test call.
  useSampleAsRequestBody() {
    this.testRequestBody = this.form.sampleRequestPayload ?? '';
  }

  /// Actually calls the connector's configured endpoint (through the backend, since a browser
  /// can't make an arbitrary cross-origin call to a third-party carrier API) and shows the raw
  /// response — the Postman-style "Send" button. Works before Save using whatever is currently
  /// typed in the form.
  sendTest() {
    if (!this.form.baseUrl) {
      this.notify.warn('Enter a Base URL first.');
      return;
    }
    this.sending = true;
    this.testResult = null;
    this.api.test(this.connectorId, { connector: this.form, requestBody: this.testRequestBody || null }).subscribe({
      next: result => {
        this.sending = false;
        this.testResult = result;
      },
      error: err => {
        this.sending = false;
        this.notify.error(err.error?.message ?? 'Failed to send the test request.');
      }
    });
  }

  /// Once a real response comes back, one click promotes it to the Inbound Mapping tab's
  /// Sample Response Payload so field detection can run against the actual carrier response
  /// instead of a hand-typed guess.
  useResponseAsSample() {
    if (!this.testResult?.responseBody) return;
    this.form.sampleResponsePayload = this.testResult.responseBody;
    this.detectResponseFields();
    this.notify.success('Response copied into Sample Response Payload.');
  }

  save() {
    if (!this.form.name || !this.form.baseUrl) {
      this.notify.warn('Name and Base URL are required.');
      return;
    }
    this.saving = true;
    const req$ = this.connectorId
      ? this.api.update(this.connectorId, this.form)
      : this.api.create(this.form);

    req$.subscribe({
      next: () => {
        this.saving = false;
        this.notify.success(`Connector ${this.connectorId ? 'updated' : 'created'}.`);
        this.router.navigate(['/shipping-connectors']);
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save connector.');
      }
    });
  }

  cancel() {
    this.router.navigate(['/shipping-connectors']);
  }
}
