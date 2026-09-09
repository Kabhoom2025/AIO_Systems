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
  VendorBillApiService, VendorBillDto, CreateVendorBillDto, UpdateVendorBillDto, CreateVendorBillLineDto
} from '../../../core/vendor-bill-api.service';
import { LedgerAccountApiService, LedgerAccountDto } from '../../../core/ledger-account-api.service';
import { VendorApiService, VendorDto } from '../../../core/vendor-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

interface LineForm {
  ledgerAccountId: number | null;
  description: string | null;
  amount: number | null;
  displayOrder: number;
}

@Component({
  selector: 'app-vendor-bills',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './vendor-bills.component.html',
  styleUrl: './vendor-bills.component.scss'
})
export class VendorBillsComponent implements OnInit {
  bills: VendorBillDto[] = [];
  ledgerAccounts: LedgerAccountDto[] = [];
  vendors: VendorDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: VendorBillDto | null = null;
  saving = false;

  form: { vendorId: number | null; payableLedgerAccountId: number | null; billDate: Date | null; dueDate: Date | null; ownerId: number | null } = this.emptyForm();
  lines: LineForm[] = [];

  showPayDialog = false;
  paying: VendorBillDto | null = null;
  paymentLedgerAccountId: number | null = null;

  constructor(
    private api: VendorBillApiService,
    private ledgerAccountApi: LedgerAccountApiService,
    private vendorApi: VendorApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.ledgerAccountApi.getAll().subscribe({ next: rows => (this.ledgerAccounts = rows), error: () => (this.ledgerAccounts = []) });
    this.vendorApi.getAll().subscribe({ next: rows => (this.vendors = rows), error: () => (this.vendors = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return { vendorId: null, payableLedgerAccountId: null, billDate: new Date(), dueDate: new Date(), ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.bills = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load vendor bills.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.lines = [];
    this.showDialog = true;
  }

  openEdit(bill: VendorBillDto) {
    this.editing = bill;
    this.form = {
      vendorId: bill.vendorId, payableLedgerAccountId: bill.payableLedgerAccountId,
      billDate: new Date(bill.billDate), dueDate: new Date(bill.dueDate), ownerId: bill.ownerId
    };
    this.lines = bill.lines
      .slice()
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map(l => ({ ledgerAccountId: l.ledgerAccountId, description: l.description, amount: l.amount, displayOrder: l.displayOrder }));
    this.showDialog = true;
  }

  addLine() {
    const nextOrder = this.lines.length ? Math.max(...this.lines.map(l => l.displayOrder)) + 1 : 1;
    this.lines.push({ ledgerAccountId: null, description: null, amount: null, displayOrder: nextOrder });
  }

  removeLine(index: number) {
    this.lines.splice(index, 1);
  }

  totalAmount(): number {
    return this.lines.reduce((sum, l) => sum + (l.amount ?? 0), 0);
  }

  save() {
    if (!this.form.payableLedgerAccountId || !this.form.ownerId || (!this.editing && !this.form.vendorId)) {
      this.notify.warn('Vendor, payable account, and owner are required.');
      return;
    }
    if (!this.lines.length || this.lines.some(l => !l.ledgerAccountId || !l.amount || l.amount <= 0)) {
      this.notify.warn('Every line needs an account and a positive amount.');
      return;
    }
    this.saving = true;
    const lineDtos: CreateVendorBillLineDto[] = this.lines.map(l => ({
      ledgerAccountId: l.ledgerAccountId!,
      description: l.description,
      amount: l.amount!,
      displayOrder: l.displayOrder
    }));
    const billDate = (this.form.billDate ?? new Date()).toISOString();
    const dueDate = (this.form.dueDate ?? new Date()).toISOString();
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          payableLedgerAccountId: this.form.payableLedgerAccountId,
          billDate, dueDate,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as UpdateVendorBillDto)
      : this.api.create({
          vendorId: this.form.vendorId,
          payableLedgerAccountId: this.form.payableLedgerAccountId,
          billDate, dueDate,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as CreateVendorBillDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Vendor bill ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save vendor bill.');
      }
    });
  }

  delete(bill: VendorBillDto) {
    this.confirm.confirm({
      message: `Delete vendor bill "${bill.billNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(bill.id).subscribe({
          next: () => {
            this.notify.success('Vendor bill deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete vendor bill.')
        });
      }
    });
  }

  approve(bill: VendorBillDto) {
    this.api.approve(bill.id).subscribe({
      next: () => {
        this.notify.success('Vendor bill approved and posted to the ledger.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to approve vendor bill.')
    });
  }

  openPay(bill: VendorBillDto) {
    this.paying = bill;
    this.paymentLedgerAccountId = null;
    this.showPayDialog = true;
  }

  confirmPay() {
    if (!this.paying || !this.paymentLedgerAccountId) {
      this.notify.warn('Select a payment account.');
      return;
    }
    this.api.pay(this.paying.id, { paymentLedgerAccountId: this.paymentLedgerAccountId }).subscribe({
      next: () => {
        this.showPayDialog = false;
        this.notify.success('Vendor bill paid.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to pay vendor bill.')
    });
  }

  cancel(bill: VendorBillDto) {
    this.confirm.confirm({
      message: `Cancel vendor bill "${bill.billNumber}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(bill.id).subscribe({
          next: () => {
            this.notify.success('Vendor bill cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel vendor bill.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Paid') return 'success';
    if (status === 'Voided') return 'danger';
    if (status === 'Approved') return 'warn';
    return 'info';
  }
}
