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
  SalesOrderApiService, SalesOrderDto, CreateSalesOrderDto, UpdateSalesOrderDto, CreateSalesOrderLineDto
} from '../../../core/sales-order-api.service';
import { AccountApiService, AccountDto } from '../../../core/account-api.service';
import { OpportunityApiService, OpportunityDto } from '../../../core/opportunity-api.service';
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
  selector: 'app-sales-orders',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './sales-orders.component.html',
  styleUrl: './sales-orders.component.scss'
})
export class SalesOrdersComponent implements OnInit {
  orders: SalesOrderDto[] = [];
  accounts: AccountDto[] = [];
  opportunities: OpportunityDto[] = [];
  taxCodes: TaxCodeDto[] = [];
  users: UserDto[] = [];
  products: ProductDto[] = [];
  loading = false;

  showDialog = false;
  editing: SalesOrderDto | null = null;
  saving = false;

  form: { accountId: number | null; opportunityId: number | null; orderDate: Date | null; ownerId: number | null } = this.emptyForm();
  lines: LineForm[] = [];

  constructor(
    private api: SalesOrderApiService,
    private accountApi: AccountApiService,
    private opportunityApi: OpportunityApiService,
    private taxCodeApi: TaxCodeApiService,
    private userApi: UserApiService,
    private productApi: ProductApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.accountApi.getAll().subscribe({ next: rows => (this.accounts = rows), error: () => (this.accounts = []) });
    this.opportunityApi.getAll().subscribe({ next: rows => (this.opportunities = rows), error: () => (this.opportunities = []) });
    this.taxCodeApi.getAll().subscribe({ next: rows => (this.taxCodes = rows), error: () => (this.taxCodes = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
    this.productApi.getAll().subscribe({ next: rows => (this.products = rows), error: () => (this.products = []) });
  }

  private emptyForm() {
    return { accountId: null, opportunityId: null, orderDate: new Date(), ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.orders = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load sales orders.');
      }
    });
  }

  opportunitiesForAccount(): OpportunityDto[] {
    if (!this.form.accountId) return [];
    return this.opportunities.filter(o => o.accountId === this.form.accountId);
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.lines = [];
    this.showDialog = true;
  }

  openEdit(order: SalesOrderDto) {
    this.editing = order;
    this.form = {
      accountId: order.accountId, opportunityId: order.opportunityId,
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
    if (!this.form.ownerId || (!this.editing && !this.form.accountId)) {
      this.notify.warn('Account and owner are required.');
      return;
    }
    if (!this.lines.length || this.lines.some(l => !l.itemName || l.quantity == null || l.unitPrice == null)) {
      this.notify.warn('Every line needs an item name, quantity and unit price.');
      return;
    }
    this.saving = true;
    const lineDtos: CreateSalesOrderLineDto[] = this.lines.map(l => ({
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
          opportunityId: this.form.opportunityId,
          orderDate,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as UpdateSalesOrderDto)
      : this.api.create({
          accountId: this.form.accountId,
          opportunityId: this.form.opportunityId,
          orderDate,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as CreateSalesOrderDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Sales order ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save sales order.');
      }
    });
  }

  delete(order: SalesOrderDto) {
    this.confirm.confirm({
      message: `Delete sales order "${order.orderNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(order.id).subscribe({
          next: () => {
            this.notify.success('Sales order deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete sales order.')
        });
      }
    });
  }

  confirmOrder(order: SalesOrderDto) {
    this.api.confirm(order.id).subscribe({
      next: () => {
        this.notify.success('Sales order confirmed.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to confirm sales order.')
    });
  }

  cancelOrder(order: SalesOrderDto) {
    this.confirm.confirm({
      message: `Cancel sales order "${order.orderNumber}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(order.id).subscribe({
          next: () => {
            this.notify.success('Sales order cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel sales order.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Confirmed') return 'success';
    if (status === 'Cancelled') return 'danger';
    return 'info';
  }
}
