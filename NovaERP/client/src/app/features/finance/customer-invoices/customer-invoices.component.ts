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
  CustomerInvoiceApiService, CustomerInvoiceDto, CreateCustomerInvoiceDto, UpdateCustomerInvoiceDto, CreateCustomerInvoiceLineDto
} from '../../../core/customer-invoice-api.service';
import { LedgerAccountApiService, LedgerAccountDto } from '../../../core/ledger-account-api.service';
import { AccountApiService, AccountDto } from '../../../core/account-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

interface LineForm {
  ledgerAccountId: number | null;
  description: string | null;
  amount: number | null;
  displayOrder: number;
}

@Component({
  selector: 'app-customer-invoices',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './customer-invoices.component.html',
  styleUrl: './customer-invoices.component.scss'
})
export class CustomerInvoicesComponent implements OnInit {
  invoices: CustomerInvoiceDto[] = [];
  ledgerAccounts: LedgerAccountDto[] = [];
  accounts: AccountDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: CustomerInvoiceDto | null = null;
  saving = false;

  form: { accountId: number | null; receivableLedgerAccountId: number | null; invoiceDate: Date | null; dueDate: Date | null; ownerId: number | null } = this.emptyForm();
  lines: LineForm[] = [];

  showPaymentDialog = false;
  receiving: CustomerInvoiceDto | null = null;
  paymentLedgerAccountId: number | null = null;

  constructor(
    private api: CustomerInvoiceApiService,
    private ledgerAccountApi: LedgerAccountApiService,
    private accountApi: AccountApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.ledgerAccountApi.getAll().subscribe({ next: rows => (this.ledgerAccounts = rows), error: () => (this.ledgerAccounts = []) });
    this.accountApi.getAll().subscribe({ next: rows => (this.accounts = rows), error: () => (this.accounts = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return { accountId: null, receivableLedgerAccountId: null, invoiceDate: new Date(), dueDate: new Date(), ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.invoices = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load customer invoices.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.lines = [];
    this.showDialog = true;
  }

  openEdit(invoice: CustomerInvoiceDto) {
    this.editing = invoice;
    this.form = {
      accountId: invoice.accountId, receivableLedgerAccountId: invoice.receivableLedgerAccountId,
      invoiceDate: new Date(invoice.invoiceDate), dueDate: new Date(invoice.dueDate), ownerId: invoice.ownerId
    };
    this.lines = invoice.lines
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
    if (!this.form.receivableLedgerAccountId || !this.form.ownerId || (!this.editing && !this.form.accountId)) {
      this.notify.warn('Customer, receivable account, and owner are required.');
      return;
    }
    if (!this.lines.length || this.lines.some(l => !l.ledgerAccountId || !l.amount || l.amount <= 0)) {
      this.notify.warn('Every line needs an account and a positive amount.');
      return;
    }
    this.saving = true;
    const lineDtos: CreateCustomerInvoiceLineDto[] = this.lines.map(l => ({
      ledgerAccountId: l.ledgerAccountId!,
      description: l.description,
      amount: l.amount!,
      displayOrder: l.displayOrder
    }));
    const invoiceDate = (this.form.invoiceDate ?? new Date()).toISOString();
    const dueDate = (this.form.dueDate ?? new Date()).toISOString();
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          receivableLedgerAccountId: this.form.receivableLedgerAccountId,
          invoiceDate, dueDate,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as UpdateCustomerInvoiceDto)
      : this.api.create({
          accountId: this.form.accountId,
          receivableLedgerAccountId: this.form.receivableLedgerAccountId,
          invoiceDate, dueDate,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as CreateCustomerInvoiceDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Customer invoice ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save customer invoice.');
      }
    });
  }

  delete(invoice: CustomerInvoiceDto) {
    this.confirm.confirm({
      message: `Delete customer invoice "${invoice.invoiceNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(invoice.id).subscribe({
          next: () => {
            this.notify.success('Customer invoice deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete customer invoice.')
        });
      }
    });
  }

  send(invoice: CustomerInvoiceDto) {
    this.api.send(invoice.id).subscribe({
      next: () => {
        this.notify.success('Customer invoice sent and posted to the ledger.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to send customer invoice.')
    });
  }

  openReceivePayment(invoice: CustomerInvoiceDto) {
    this.receiving = invoice;
    this.paymentLedgerAccountId = null;
    this.showPaymentDialog = true;
  }

  confirmReceivePayment() {
    if (!this.receiving || !this.paymentLedgerAccountId) {
      this.notify.warn('Select a payment account.');
      return;
    }
    this.api.receivePayment(this.receiving.id, { paymentLedgerAccountId: this.paymentLedgerAccountId }).subscribe({
      next: () => {
        this.showPaymentDialog = false;
        this.notify.success('Payment received.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to record payment.')
    });
  }

  cancel(invoice: CustomerInvoiceDto) {
    this.confirm.confirm({
      message: `Cancel customer invoice "${invoice.invoiceNumber}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(invoice.id).subscribe({
          next: () => {
            this.notify.success('Customer invoice cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel customer invoice.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Paid') return 'success';
    if (status === 'Voided') return 'danger';
    if (status === 'Sent') return 'warn';
    return 'info';
  }
}
