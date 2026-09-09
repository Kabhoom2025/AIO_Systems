import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Table, TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { SidebarModule } from 'primeng/sidebar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  ShipmentApiService, ShipmentDto, CreateShipmentDto, UpdateShipmentDto,
  CreateShipmentLineDto, CreateShipmentPackageDto, CreateShipmentPackageItemDto
} from '../../../core/shipment-api.service';
import { WarehouseApiService, WarehouseDto } from '../../../core/warehouse-api.service';
import { SalesOrderApiService, SalesOrderDto } from '../../../core/sales-order-api.service';
import { ProductApiService, ProductDto } from '../../../core/product-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';
import { StockMovementApiService } from '../../../core/stock-movement-api.service';
import { ShippingConnectorApiService, RateQuoteDto } from '../../../core/shipping-connector-api.service';

interface LineForm {
  productId: number | null;
  quantity: number | null;
  salesOrderLineId: number | null;
  displayOrder: number;
  availableQty: number | null;
  loadingAvailable: boolean;
}

interface PackageItemForm {
  productId: number | null;
  quantity: number | null;
}

interface PackageForm {
  packageNumber: number;
  weightKg: number | null;
  lengthCm: number | null;
  widthCm: number | null;
  heightCm: number | null;
  trackingNumber: string | null;
  items: PackageItemForm[];
}

@Component({
  selector: 'app-shipments',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, SidebarModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, CheckboxModule,
    TagModule, TooltipModule, ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './shipments.component.html',
  styleUrl: './shipments.component.scss'
})
export class ShipmentsComponent implements OnInit {
  @ViewChild('dt') ratesTable?: Table;

  shipments: ShipmentDto[] = [];
  warehouses: WarehouseDto[] = [];
  salesOrders: SalesOrderDto[] = [];
  products: ProductDto[] = [];
  users: UserDto[] = [];
  loading = false;

  sourceTypes = [
    { label: 'Sales Order', value: 'SalesOrder' },
    { label: 'Transfer Order', value: 'TransferOrder' }
  ];

  showDialog = false;
  editing: ShipmentDto | null = null;
  saving = false;

  form: {
    warehouseId: number | null;
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
    shipDate: Date | null;
    carrier: string | null;
    trackingNumber: string | null;
    isBlindShipment: boolean;
    ownerId: number | null;
  } = this.emptyForm();

  lines: LineForm[] = [];
  packages: PackageForm[] = [];

  showRatesDialog = false;
  ratesLoading = false;
  rateQuotes: RateQuoteDto[] = [];
  rateErrors: string[] = [];
  /// Keyed by RateQuoteDto.rateId — which quote rows currently have their surcharge breakdown
  /// expanded (p-table's own [(expandedRowKeys)] binding shape).
  expandedRateRows: { [rateId: string]: boolean } = {};
  ratesShipment: ShipmentDto | null = null;
  showRateErrorsDialog = false;
  groupBy: 'None' | 'Carrier' = 'None';
  groupByOptions = [
    { label: 'None', value: 'None' },
    { label: 'Carrier', value: 'Carrier' }
  ];
  private readonly carrierPalette = ['#2563eb', '#dc2626', '#059669', '#7c3aed', '#d97706', '#0891b2', '#be185d', '#4d7c0f'];

  constructor(
    private api: ShipmentApiService,
    private warehouseApi: WarehouseApiService,
    private salesOrderApi: SalesOrderApiService,
    private productApi: ProductApiService,
    private userApi: UserApiService,
    private stockMovementApi: StockMovementApiService,
    private ratesApi: ShippingConnectorApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.warehouseApi.getAll().subscribe({ next: rows => (this.warehouses = rows), error: () => (this.warehouses = []) });
    this.salesOrderApi.getAll().subscribe({ next: rows => (this.salesOrders = rows), error: () => (this.salesOrders = []) });
    this.productApi.getAll().subscribe({ next: rows => (this.products = rows), error: () => (this.products = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return {
      warehouseId: null, sourceType: 'SalesOrder', salesOrderId: null, destinationWarehouseId: null,
      shipToName: null, shipToContactName: null, shipToEmail: null,
      shipToAddressLine1: null, shipToAddressLine2: null, shipToCity: null,
      shipToState: null, shipToPostalCode: null, shipToCountry: null, shipToPhone: null,
      shipToTaxType: null, shipToTaxCountry: null, shipToTaxId: null,
      shipFromName: null, shipFromContactName: null, shipFromEmail: null,
      shipFromAddressLine1: null, shipFromAddressLine2: null, shipFromCity: null,
      shipFromState: null, shipFromPostalCode: null, shipFromCountry: null, shipFromPhone: null,
      shipDate: new Date(), carrier: null, trackingNumber: null, isBlindShipment: false, ownerId: null
    };
  }

  confirmedSalesOrders(): SalesOrderDto[] {
    return this.salesOrders.filter(o => o.status === 'Confirmed');
  }

  /// Shown as the Ship-From "Company" field's placeholder so it's obvious what address is
  /// actually used when every Ship-From field is left blank (the selected Warehouse's own).
  selectedWarehouseName(): string {
    const warehouse = this.warehouses.find(w => w.id === this.form.warehouseId);
    return warehouse ? warehouse.name : 'Warehouse address';
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.shipments = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load shipments.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.lines = [];
    this.packages = [];
    this.showDialog = true;
  }

  openEdit(shipment: ShipmentDto) {
    this.editing = shipment;
    this.form = {
      warehouseId: shipment.warehouseId, sourceType: shipment.sourceType,
      salesOrderId: shipment.salesOrderId, destinationWarehouseId: shipment.destinationWarehouseId,
      shipToName: shipment.shipToName, shipToContactName: shipment.shipToContactName,
      shipToEmail: shipment.shipToEmail,
      shipToAddressLine1: shipment.shipToAddressLine1,
      shipToAddressLine2: shipment.shipToAddressLine2, shipToCity: shipment.shipToCity,
      shipToState: shipment.shipToState, shipToPostalCode: shipment.shipToPostalCode,
      shipToCountry: shipment.shipToCountry, shipToPhone: shipment.shipToPhone,
      shipToTaxType: shipment.shipToTaxType, shipToTaxCountry: shipment.shipToTaxCountry,
      shipToTaxId: shipment.shipToTaxId,
      shipFromName: shipment.shipFromName, shipFromContactName: shipment.shipFromContactName,
      shipFromEmail: shipment.shipFromEmail, shipFromAddressLine1: shipment.shipFromAddressLine1,
      shipFromAddressLine2: shipment.shipFromAddressLine2, shipFromCity: shipment.shipFromCity,
      shipFromState: shipment.shipFromState, shipFromPostalCode: shipment.shipFromPostalCode,
      shipFromCountry: shipment.shipFromCountry, shipFromPhone: shipment.shipFromPhone,
      shipDate: new Date(shipment.shipDate), carrier: shipment.carrier,
      trackingNumber: shipment.trackingNumber, isBlindShipment: shipment.isBlindShipment,
      ownerId: shipment.ownerId
    };
    this.lines = shipment.lines
      .slice()
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map(l => ({
        productId: l.productId, quantity: l.quantity, salesOrderLineId: l.salesOrderLineId,
        displayOrder: l.displayOrder, availableQty: null, loadingAvailable: false
      }));
    this.packages = shipment.packages.map(p => ({
      packageNumber: p.packageNumber, weightKg: p.weightKg, lengthCm: p.lengthCm,
      widthCm: p.widthCm, heightCm: p.heightCm, trackingNumber: p.trackingNumber,
      items: p.items.map(i => ({ productId: i.productId, quantity: i.quantity }))
    }));
    this.showDialog = true;
    this.lines.forEach(l => this.refreshAvailableQty(l));
  }

  addLine() {
    const nextOrder = this.lines.length ? Math.max(...this.lines.map(l => l.displayOrder)) + 1 : 1;
    this.lines.push({ productId: null, quantity: null, salesOrderLineId: null, displayOrder: nextOrder, availableQty: null, loadingAvailable: false });
  }

  removeLine(index: number) {
    this.lines.splice(index, 1);
  }

  onProductSelect(line: LineForm) {
    line.quantity = null;
    this.refreshAvailableQty(line);
  }

  onWarehouseChange() {
    this.lines.forEach(l => this.refreshAvailableQty(l));
  }

  private refreshAvailableQty(line: LineForm) {
    if (!line.productId || !this.form.warehouseId) {
      line.availableQty = null;
      return;
    }
    line.loadingAvailable = true;
    this.stockMovementApi.getOnHandAtWarehouse(line.productId, this.form.warehouseId).subscribe({
      next: qty => { line.availableQty = qty; line.loadingAvailable = false; },
      error: () => { line.availableQty = null; line.loadingAvailable = false; }
    });
  }

  addPackage() {
    const nextNumber = this.packages.length ? Math.max(...this.packages.map(p => p.packageNumber)) + 1 : 1;
    this.packages.push({
      packageNumber: nextNumber, weightKg: null, lengthCm: null, widthCm: null, heightCm: null,
      trackingNumber: null, items: []
    });
  }

  removePackage(index: number) {
    this.packages.splice(index, 1);
  }

  /// Every line item added to the shipment, with the per-unit and line-total weight computed
  /// from the product's catalog WeightKg — shown next to each line so the user knows what
  /// they're packing before assigning it to a package.
  lineWeightPerUnit(line: LineForm): number | null {
    const product = this.products.find(p => p.id === line.productId);
    return product?.weightKg ?? null;
  }

  lineTotalWeight(line: LineForm): number | null {
    const perUnit = this.lineWeightPerUnit(line);
    if (perUnit == null || line.quantity == null) return null;
    return perUnit * line.quantity;
  }

  /// Dropdown options for "which item goes in this package" — one entry per distinct product
  /// currently on the shipment's Items list, labeled with the product name so the dropdown
  /// doesn't just show a raw id.
  packableLineOptions(): { label: string; value: number }[] {
    const seen = new Set<number>();
    const options: { label: string; value: number }[] = [];
    for (const line of this.lines) {
      if (line.productId == null || seen.has(line.productId)) continue;
      seen.add(line.productId);
      const product = this.products.find(p => p.id === line.productId);
      options.push({ label: product ? `${product.name} (${product.sku})` : `Product #${line.productId}`, value: line.productId });
    }
    return options;
  }

  addPackageItem(pkg: PackageForm) {
    pkg.items.push({ productId: null, quantity: null });
  }

  removePackageItem(pkg: PackageForm, index: number) {
    pkg.items.splice(index, 1);
  }

  /// Sums (product WeightKg * quantity) across every item currently assigned to this package
  /// and fills the Weight field — a one-shot calculation, not a live binding, so the user is
  /// always free to type over it afterward with an actual scale reading.
  recalcPackageWeight(pkg: PackageForm) {
    let total = 0;
    let hasWeight = false;
    for (const item of pkg.items) {
      const product = this.products.find(p => p.id === item.productId);
      if (product?.weightKg != null && item.quantity != null) {
        total += product.weightKg * item.quantity;
        hasWeight = true;
      }
    }
    if (hasWeight) {
      pkg.weightKg = Math.round(total * 1000) / 1000;
      this.notify.success(`Package ${pkg.packageNumber} weight set to ${pkg.weightKg} kg from packed items.`);
    } else {
      this.notify.warn('None of the packed items have a catalog weight set — add one in Products first.');
    }
  }

  save() {
    if (!this.form.warehouseId || !this.form.ownerId) {
      this.notify.warn('Warehouse and owner are required.');
      return;
    }
    if (!this.editing && !this.form.sourceType) {
      this.notify.warn('Source type is required.');
      return;
    }
    if (!this.editing && this.form.sourceType === 'SalesOrder' && (!this.form.salesOrderId || !this.form.shipToAddressLine1 || !this.form.shipToCity)) {
      this.notify.warn('Sales order shipments need a source order and a ship-to address line and city.');
      return;
    }
    if (!this.editing && this.form.sourceType === 'TransferOrder' && !this.form.destinationWarehouseId) {
      this.notify.warn('Transfer order shipments need a destination warehouse.');
      return;
    }
    if (!this.lines.length || this.lines.some(l => !l.productId || l.quantity == null)) {
      this.notify.warn('Every line needs a product and a positive quantity.');
      return;
    }
    const overStockLine = this.lines.find(l => l.availableQty != null && (l.quantity ?? 0) > l.availableQty);
    if (overStockLine) {
      this.notify.warn(`Requested quantity exceeds the ${overStockLine.availableQty} units available for that product at this warehouse.`);
      return;
    }
    this.saving = true;
    const lineDtos: CreateShipmentLineDto[] = this.lines.map(l => ({
      productId: l.productId!, quantity: l.quantity!, salesOrderLineId: l.salesOrderLineId, displayOrder: l.displayOrder
    }));
    const packageDtos: CreateShipmentPackageDto[] = this.packages.map(p => ({
      packageNumber: p.packageNumber, weightKg: p.weightKg, lengthCm: p.lengthCm,
      widthCm: p.widthCm, heightCm: p.heightCm, trackingNumber: p.trackingNumber,
      items: p.items
        .filter(i => i.productId != null && i.quantity != null)
        .map(i => ({ productId: i.productId!, quantity: i.quantity! } as CreateShipmentPackageItemDto))
    }));
    const shipDate = (this.form.shipDate ?? new Date()).toISOString();
    const sharedFields = {
      warehouseId: this.form.warehouseId!,
      shipToName: this.form.shipToName, shipToContactName: this.form.shipToContactName,
      shipToEmail: this.form.shipToEmail,
      shipToAddressLine1: this.form.shipToAddressLine1,
      shipToAddressLine2: this.form.shipToAddressLine2, shipToCity: this.form.shipToCity,
      shipToState: this.form.shipToState, shipToPostalCode: this.form.shipToPostalCode,
      shipToCountry: this.form.shipToCountry, shipToPhone: this.form.shipToPhone,
      shipToTaxType: this.form.shipToTaxType, shipToTaxCountry: this.form.shipToTaxCountry,
      shipToTaxId: this.form.shipToTaxId,
      shipFromName: this.form.shipFromName, shipFromContactName: this.form.shipFromContactName,
      shipFromEmail: this.form.shipFromEmail, shipFromAddressLine1: this.form.shipFromAddressLine1,
      shipFromAddressLine2: this.form.shipFromAddressLine2, shipFromCity: this.form.shipFromCity,
      shipFromState: this.form.shipFromState, shipFromPostalCode: this.form.shipFromPostalCode,
      shipFromCountry: this.form.shipFromCountry, shipFromPhone: this.form.shipFromPhone,
      shipDate, carrier: this.form.carrier, trackingNumber: this.form.trackingNumber,
      isBlindShipment: this.form.isBlindShipment, ownerId: this.form.ownerId!,
      lines: lineDtos, packages: packageDtos
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, sharedFields as UpdateShipmentDto)
      : this.api.create({
          sourceType: this.form.sourceType,
          salesOrderId: this.form.sourceType === 'SalesOrder' ? this.form.salesOrderId : null,
          destinationWarehouseId: this.form.sourceType === 'TransferOrder' ? this.form.destinationWarehouseId : null,
          ...sharedFields
        } as CreateShipmentDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Shipment ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save shipment.');
      }
    });
  }

  delete(shipment: ShipmentDto) {
    this.confirm.confirm({
      message: `Delete shipment "${shipment.shipmentNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(shipment.id).subscribe({
          next: () => {
            this.notify.success('Shipment deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete shipment.')
        });
      }
    });
  }

  pick(shipment: ShipmentDto) {
    this.api.pick(shipment.id).subscribe({
      next: () => {
        this.notify.success('Shipment marked picked.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to mark picked.')
    });
  }

  ship(shipment: ShipmentDto) {
    this.confirm.confirm({
      message: `Ship "${shipment.shipmentNumber}"? This will deduct stock from ${shipment.warehouseName}.`,
      header: 'Confirm Ship',
      icon: 'pi pi-send',
      accept: () => {
        this.api.ship(shipment.id).subscribe({
          next: () => {
            this.notify.success('Shipment marked shipped.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to ship.')
        });
      }
    });
  }

  deliver(shipment: ShipmentDto) {
    this.api.deliver(shipment.id).subscribe({
      next: () => {
        this.notify.success('Shipment marked delivered.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to mark delivered.')
    });
  }

  cancel(shipment: ShipmentDto) {
    this.confirm.confirm({
      message: `Cancel shipment "${shipment.shipmentNumber}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(shipment.id).subscribe({
          next: () => {
            this.notify.success('Shipment cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel shipment.')
        });
      }
    });
  }

  getRates() {
    if (!this.editing) return;
    this.getRatesFor(this.editing);
  }

  getRatesFor(shipment: ShipmentDto) {
    this.ratesShipment = shipment;
    this.groupBy = 'None';
    this.showRatesDialog = true;
    this.showRateErrorsDialog = false;
    this.ratesLoading = true;
    this.rateQuotes = [];
    this.rateErrors = [];
    this.expandedRateRows = {};
    this.ratesApi.getRatesForShipment(shipment.id).subscribe({
      next: result => {
        this.rateQuotes = result.quotes;
        this.rateErrors = result.errors;
        this.ratesLoading = false;
      },
      error: err => {
        this.ratesLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to fetch rate quotes.');
      }
    });
  }

  refreshRates() {
    if (this.ratesShipment) this.getRatesFor(this.ratesShipment);
  }

  /// PrimeNG's Table has no expandedRowKeys two-way binding in this version — only
  /// onRowExpand/onRowCollapse events — so the expanded-state object is maintained by hand here.
  onRateRowExpand(event: { data: RateQuoteDto }) {
    this.expandedRateRows = { ...this.expandedRateRows, [event.data.rateId ?? '']: true };
  }

  onRateRowCollapse(event: { data: RateQuoteDto }) {
    const { [event.data.rateId ?? '']: _, ...rest } = this.expandedRateRows;
    this.expandedRateRows = rest;
  }

  surchargeTotal(quote: RateQuoteDto): number {
    return quote.surcharges.reduce((sum, s) => sum + (s.amount ?? 0), 0);
  }

  onRatesSearch(value: string) {
    this.ratesTable?.filterGlobal(value, 'contains');
  }

  carrierInitial(carrierName: string | null): string {
    return (carrierName ?? '?').trim().charAt(0).toUpperCase() || '?';
  }

  carrierColor(carrierName: string | null): string {
    const key = carrierName ?? '?';
    let hash = 0;
    for (let i = 0; i < key.length; i++) hash = (hash * 31 + key.charCodeAt(i)) >>> 0;
    return this.carrierPalette[hash % this.carrierPalette.length];
  }

  useRate(quote: RateQuoteDto) {
    const carrier = `${quote.carrierName ?? quote.connectorName} — ${quote.serviceLevel ?? 'Standard'}`;

    if (this.showDialog && this.editing && this.ratesShipment?.id === this.editing.id) {
      this.form.carrier = carrier;
      this.showRatesDialog = false;
      this.notify.success('Carrier updated from selected rate quote.');
      return;
    }

    if (!this.ratesShipment) return;
    const shipment = this.ratesShipment;
    const payload: UpdateShipmentDto = {
      warehouseId: shipment.warehouseId,
      shipToName: shipment.shipToName, shipToContactName: shipment.shipToContactName,
      shipToEmail: shipment.shipToEmail, shipToAddressLine1: shipment.shipToAddressLine1,
      shipToAddressLine2: shipment.shipToAddressLine2, shipToCity: shipment.shipToCity,
      shipToState: shipment.shipToState, shipToPostalCode: shipment.shipToPostalCode,
      shipToCountry: shipment.shipToCountry, shipToPhone: shipment.shipToPhone,
      shipToTaxType: shipment.shipToTaxType, shipToTaxCountry: shipment.shipToTaxCountry,
      shipToTaxId: shipment.shipToTaxId,
      shipFromName: shipment.shipFromName, shipFromContactName: shipment.shipFromContactName,
      shipFromEmail: shipment.shipFromEmail, shipFromAddressLine1: shipment.shipFromAddressLine1,
      shipFromAddressLine2: shipment.shipFromAddressLine2, shipFromCity: shipment.shipFromCity,
      shipFromState: shipment.shipFromState, shipFromPostalCode: shipment.shipFromPostalCode,
      shipFromCountry: shipment.shipFromCountry, shipFromPhone: shipment.shipFromPhone,
      shipDate: shipment.shipDate, carrier, trackingNumber: shipment.trackingNumber,
      isBlindShipment: shipment.isBlindShipment, ownerId: shipment.ownerId,
      lines: shipment.lines.map(l => ({
        productId: l.productId, quantity: l.quantity, salesOrderLineId: l.salesOrderLineId, displayOrder: l.displayOrder
      })),
      packages: shipment.packages.map(p => ({
        packageNumber: p.packageNumber, weightKg: p.weightKg, lengthCm: p.lengthCm,
        widthCm: p.widthCm, heightCm: p.heightCm, trackingNumber: p.trackingNumber,
        items: p.items.map(i => ({ productId: i.productId, quantity: i.quantity }))
      }))
    };
    this.api.update(shipment.id, payload).subscribe({
      next: () => {
        this.showRatesDialog = false;
        this.notify.success(`Carrier set to "${carrier}" on ${shipment.shipmentNumber}.`);
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to update carrier.')
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Delivered') return 'success';
    if (status === 'Cancelled') return 'danger';
    if (status === 'Shipped' || status === 'Picked') return 'warn';
    return 'info';
  }
}
