import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  PurchaseOrderApiService, PurchaseOrderDto, CreatePurchaseOrderDto, UpdatePurchaseOrderDto, CreatePurchaseOrderLineDto
} from '../../../core/purchase-order-api.service';
import { VendorApiService, VendorDto } from '../../../core/vendor-api.service';
import { RfqApiService, RfqRequestDto } from '../../../core/rfq-api.service';
import { TaxCodeApiService, TaxCodeDto } from '../../../core/tax-code-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';
import { ProductApiService, ProductDto } from '../../../core/product-api.service';

interface LineForm {
  itemName: string;
  quantity: number | null;
  unitPrice: number | null;
  taxCodeId: number | null;
  productId: number | null;
  displayOrder: number;
}

@Component({
  selector: 'app-purchase-orders',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './purchase-orders.component.html',
  styleUrl: './purchase-orders.component.scss'
})
export class PurchaseOrdersComponent implements OnInit {
  orders: PurchaseOrderDto[] = [];
  vendors: VendorDto[] = [];
  rfqs: RfqRequestDto[] = [];
  taxCodes: TaxCodeDto[] = [];
  users: UserDto[] = [];
  products: ProductDto[] = [];
  loading = false;

  showDialog = false;
  editing: PurchaseOrderDto | null = null;
  saving = false;

  form: { vendorId: number | null; rfqRequestId: number | null; orderDate: Date | null; ownerId: number | null } = this.emptyForm();
  lines: LineForm[] = [];

  constructor(
    private api: PurchaseOrderApiService,
    private vendorApi: VendorApiService,
    private rfqApi: RfqApiService,
    private taxCodeApi: TaxCodeApiService,
    private userApi: UserApiService,
    private productApi: ProductApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.vendorApi.getAll().subscribe({ next: rows => (this.vendors = rows), error: () => (this.vendors = []) });
    this.rfqApi.getAll().subscribe({ next: rows => (this.rfqs = rows), error: () => (this.rfqs = []) });
    this.taxCodeApi.getAll().subscribe({ next: rows => (this.taxCodes = rows), error: () => (this.taxCodes = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
    this.productApi.getAll().subscribe({ next: rows => (this.products = rows), error: () => (this.products = []) });
  }

  private emptyForm() {
    return { vendorId: null, rfqRequestId: null, orderDate: new Date(), ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.orders = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load purchase orders.');
      }
    });
  }

  rfqsForVendor(): RfqRequestDto[] {
    if (!this.form.vendorId) return [];
    return this.rfqs.filter(r => r.status === 'Closed' && r.winningVendorId === this.form.vendorId);
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.lines = [];
    this.showDialog = true;
  }

  openEdit(order: PurchaseOrderDto) {
    this.editing = order;
    this.form = {
      vendorId: order.vendorId, rfqRequestId: order.rfqRequestId,
      orderDate: new Date(order.orderDate), ownerId: order.ownerId
    };
    this.lines = order.lines
      .slice()
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map(l => ({ itemName: l.itemName, quantity: l.quantity, unitPrice: l.unitPrice, taxCodeId: l.taxCodeId, productId: l.productId, displayOrder: l.displayOrder }));
    this.showDialog = true;
  }

  addLine() {
    const nextOrder = this.lines.length ? Math.max(...this.lines.map(l => l.displayOrder)) + 1 : 1;
    this.lines.push({ itemName: '', quantity: null, unitPrice: null, taxCodeId: null, productId: null, displayOrder: nextOrder });
  }

  removeLine(index: number) {
    this.lines.splice(index, 1);
  }

  onProductSelect(line: LineForm) {
    const product = this.products.find(p => p.id === line.productId);
    if (product) {
      line.itemName = product.name;
    }
  }

  taxRateFor(taxCodeId: number | null): number {
    if (!taxCodeId) return 0;
    return this.taxCodes.find(t => t.id === taxCodeId)?.totalRatePercent ?? 0;
  }

  lineSubtotal(line: LineForm): number {
    return (line.quantity ?? 0) * (line.unitPrice ?? 0);
  }

  lineTax(line: LineForm): number {
    return this.lineSubtotal(line) * this.taxRateFor(line.taxCodeId) / 100;
  }

  subtotal(): number {
    return this.lines.reduce((sum, l) => sum + this.lineSubtotal(l), 0);
  }

  taxTotal(): number {
    return this.lines.reduce((sum, l) => sum + this.lineTax(l), 0);
  }

  grandTotal(): number {
    return this.subtotal() + this.taxTotal();
  }

  save() {
    if (!this.form.ownerId || (!this.editing && !this.form.vendorId)) {
      this.notify.warn('Vendor and owner are required.');
      return;
    }
    if (!this.lines.length || this.lines.some(l => !l.itemName || l.quantity == null || l.unitPrice == null)) {
      this.notify.warn('Every line needs an item name, quantity and unit price.');
      return;
    }
    this.saving = true;
    const lineDtos: CreatePurchaseOrderLineDto[] = this.lines.map(l => ({
      itemName: l.itemName,
      quantity: l.quantity!,
      unitPrice: l.unitPrice!,
      taxCodeId: l.taxCodeId,
      productId: l.productId,
      displayOrder: l.displayOrder
    }));
    const orderDate = (this.form.orderDate ?? new Date()).toISOString();
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          rfqRequestId: this.form.rfqRequestId,
          orderDate,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as UpdatePurchaseOrderDto)
      : this.api.create({
          vendorId: this.form.vendorId,
          rfqRequestId: this.form.rfqRequestId,
          orderDate,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as CreatePurchaseOrderDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Purchase order ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save purchase order.');
      }
    });
  }

  delete(order: PurchaseOrderDto) {
    this.confirm.confirm({
      message: `Delete purchase order "${order.poNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(order.id).subscribe({
          next: () => {
            this.notify.success('Purchase order deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete purchase order.')
        });
      }
    });
  }

  confirmOrder(order: PurchaseOrderDto) {
    this.api.confirm(order.id).subscribe({
      next: () => {
        this.notify.success('Purchase order confirmed.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to confirm purchase order.')
    });
  }

  receiveOrder(order: PurchaseOrderDto) {
    this.api.receive(order.id).subscribe({
      next: () => {
        this.notify.success('Purchase order marked received.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to mark purchase order received.')
    });
  }

  cancelOrder(order: PurchaseOrderDto) {
    this.confirm.confirm({
      message: `Cancel purchase order "${order.poNumber}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(order.id).subscribe({
          next: () => {
            this.notify.success('Purchase order cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel purchase order.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Received') return 'success';
    if (status === 'Cancelled') return 'danger';
    if (status === 'Confirmed') return 'warn';
    return 'info';
  }
}
