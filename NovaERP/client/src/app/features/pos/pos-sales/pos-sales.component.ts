import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
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
  PosSaleApiService, PosSaleDto, CreatePosSaleDto, UpdatePosSaleDto, CreatePosSaleLineDto
} from '../../../core/pos-sale-api.service';
import { WarehouseApiService, WarehouseDto } from '../../../core/warehouse-api.service';
import { AccountApiService, AccountDto } from '../../../core/account-api.service';
import { LedgerAccountApiService, LedgerAccountDto } from '../../../core/ledger-account-api.service';
import { ProductApiService, ProductDto } from '../../../core/product-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';
import { StockMovementApiService } from '../../../core/stock-movement-api.service';

interface LineForm {
  productId: number | null;
  quantity: number | null;
  unitPrice: number | null;
  availableQty: number | null;
  loadingAvailable: boolean;
}

@Component({
  selector: 'app-pos-sales',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './pos-sales.component.html',
  styleUrl: './pos-sales.component.scss'
})
export class PosSalesComponent implements OnInit {
  sales: PosSaleDto[] = [];
  warehouses: WarehouseDto[] = [];
  accounts: AccountDto[] = [];
  ledgerAccounts: LedgerAccountDto[] = [];
  products: ProductDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: PosSaleDto | null = null;
  saving = false;

  form: { warehouseId: number | null; customerAccountId: number | null; saleDate: Date | null; revenueLedgerAccountId: number | null; ownerId: number | null } = this.emptyForm();
  lines: LineForm[] = [];

  showCompleteDialog = false;
  completing: PosSaleDto | null = null;
  paymentLedgerAccountId: number | null = null;

  constructor(
    private api: PosSaleApiService,
    private warehouseApi: WarehouseApiService,
    private accountApi: AccountApiService,
    private ledgerAccountApi: LedgerAccountApiService,
    private productApi: ProductApiService,
    private userApi: UserApiService,
    private stockMovementApi: StockMovementApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.warehouseApi.getAll().subscribe({ next: rows => (this.warehouses = rows), error: () => (this.warehouses = []) });
    this.accountApi.getAll().subscribe({ next: rows => (this.accounts = rows), error: () => (this.accounts = []) });
    this.ledgerAccountApi.getAll().subscribe({ next: rows => (this.ledgerAccounts = rows), error: () => (this.ledgerAccounts = []) });
    this.productApi.getAll().subscribe({ next: rows => (this.products = rows), error: () => (this.products = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return { warehouseId: null, customerAccountId: null, saleDate: new Date(), revenueLedgerAccountId: null, ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.sales = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load POS sales.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.lines = [];
    this.showDialog = true;
  }

  openEdit(sale: PosSaleDto) {
    this.editing = sale;
    this.form = {
      warehouseId: sale.warehouseId, customerAccountId: sale.customerAccountId,
      saleDate: new Date(sale.saleDate), revenueLedgerAccountId: sale.revenueLedgerAccountId, ownerId: sale.ownerId
    };
    this.lines = sale.lines.map(l => ({
      productId: l.productId, quantity: l.quantity, unitPrice: l.unitPrice,
      availableQty: null, loadingAvailable: false
    }));
    this.lines.forEach(l => this.refreshAvailableQty(l));
    this.showDialog = true;
  }

  addLine() {
    this.lines.push({ productId: null, quantity: null, unitPrice: null, availableQty: null, loadingAvailable: false });
  }

  removeLine(index: number) {
    this.lines.splice(index, 1);
  }

  onProductSelect(line: LineForm) {
    line.quantity = null;
    const product = this.products.find(p => p.id === line.productId);
    line.unitPrice = product?.unitCost ?? null;
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

  totalAmount(): number {
    return this.lines.reduce((sum, l) => sum + (l.quantity ?? 0) * (l.unitPrice ?? 0), 0);
  }

  save() {
    if (!this.form.warehouseId || !this.form.revenueLedgerAccountId || !this.form.ownerId) {
      this.notify.warn('Warehouse, revenue account, and owner are required.');
      return;
    }
    if (!this.lines.length || this.lines.some(l => !l.productId || !l.quantity || l.quantity <= 0 || l.unitPrice == null || l.unitPrice < 0)) {
      this.notify.warn('Every line needs a product, a positive quantity, and a unit price.');
      return;
    }
    this.saving = true;
    const lineDtos: CreatePosSaleLineDto[] = this.lines.map(l => ({
      productId: l.productId!, quantity: l.quantity!, unitPrice: l.unitPrice!
    }));
    const saleDate = (this.form.saleDate ?? new Date()).toISOString();
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          warehouseId: this.form.warehouseId, customerAccountId: this.form.customerAccountId,
          saleDate, revenueLedgerAccountId: this.form.revenueLedgerAccountId, ownerId: this.form.ownerId,
          lines: lineDtos
        } as UpdatePosSaleDto)
      : this.api.create({
          warehouseId: this.form.warehouseId, customerAccountId: this.form.customerAccountId,
          saleDate, revenueLedgerAccountId: this.form.revenueLedgerAccountId, ownerId: this.form.ownerId,
          lines: lineDtos
        } as CreatePosSaleDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`POS sale ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save POS sale.');
      }
    });
  }

  delete(sale: PosSaleDto) {
    this.confirm.confirm({
      message: `Delete POS sale "${sale.saleNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(sale.id).subscribe({
          next: () => {
            this.notify.success('POS sale deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete POS sale.')
        });
      }
    });
  }

  openComplete(sale: PosSaleDto) {
    this.completing = sale;
    this.paymentLedgerAccountId = null;
    this.showCompleteDialog = true;
  }

  confirmComplete() {
    if (!this.completing || !this.paymentLedgerAccountId) {
      this.notify.warn('Select a payment account.');
      return;
    }
    this.api.complete(this.completing.id, { paymentLedgerAccountId: this.paymentLedgerAccountId }).subscribe({
      next: () => {
        this.showCompleteDialog = false;
        this.notify.success('Sale completed and posted to the ledger.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to complete sale.')
    });
  }

  refund(sale: PosSaleDto) {
    this.confirm.confirm({
      message: `Refund POS sale "${sale.saleNumber}"? This cannot be undone.`,
      header: 'Confirm Refund',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.refund(sale.id).subscribe({
          next: () => {
            this.notify.success('Sale refunded.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to refund sale.')
        });
      }
    });
  }

  cancel(sale: PosSaleDto) {
    this.confirm.confirm({
      message: `Cancel POS sale "${sale.saleNumber}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(sale.id).subscribe({
          next: () => {
            this.notify.success('POS sale cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel POS sale.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' | 'secondary' {
    if (status === 'Completed') return 'success';
    if (status === 'Refunded') return 'danger';
    if (status === 'Cancelled') return 'secondary';
    return 'info';
  }
}
