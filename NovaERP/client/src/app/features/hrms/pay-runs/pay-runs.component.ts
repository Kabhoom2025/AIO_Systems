import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  PayRunApiService, PayRunDto, CreatePayRunDto, UpdatePayRunDto
} from '../../../core/pay-run-api.service';
import { LedgerAccountApiService, LedgerAccountDto } from '../../../core/ledger-account-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

@Component({
  selector: 'app-pay-runs',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputNumberModule, DropdownModule, TagModule, ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './pay-runs.component.html',
  styleUrl: './pay-runs.component.scss'
})
export class PayRunsComponent implements OnInit {
  payRuns: PayRunDto[] = [];
  ledgerAccounts: LedgerAccountDto[] = [];
  users: UserDto[] = [];
  loading = false;

  months = [
    { label: 'January', value: 1 }, { label: 'February', value: 2 }, { label: 'March', value: 3 },
    { label: 'April', value: 4 }, { label: 'May', value: 5 }, { label: 'June', value: 6 },
    { label: 'July', value: 7 }, { label: 'August', value: 8 }, { label: 'September', value: 9 },
    { label: 'October', value: 10 }, { label: 'November', value: 11 }, { label: 'December', value: 12 }
  ];

  showDialog = false;
  editing: PayRunDto | null = null;
  saving = false;

  form: {
    periodMonth: number | null;
    periodYear: number | null;
    expenseLedgerAccountId: number | null;
    deductionsPayableLedgerAccountId: number | null;
    ownerId: number | null;
  } = this.emptyForm();

  showLinesDialog = false;
  viewing: PayRunDto | null = null;

  showPayDialog = false;
  paying: PayRunDto | null = null;
  paymentLedgerAccountId: number | null = null;

  constructor(
    private api: PayRunApiService,
    private ledgerAccountApi: LedgerAccountApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.ledgerAccountApi.getAll().subscribe({ next: rows => (this.ledgerAccounts = rows), error: () => (this.ledgerAccounts = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    const now = new Date();
    return {
      periodMonth: now.getMonth() + 1, periodYear: now.getFullYear(),
      expenseLedgerAccountId: null, deductionsPayableLedgerAccountId: null, ownerId: null
    };
  }

  monthName(month: number): string {
    return this.months.find(m => m.value === month)?.label ?? String(month);
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.payRuns = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load pay runs.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(run: PayRunDto) {
    this.editing = run;
    this.form = {
      periodMonth: run.periodMonth, periodYear: run.periodYear,
      expenseLedgerAccountId: run.expenseLedgerAccountId,
      deductionsPayableLedgerAccountId: run.deductionsPayableLedgerAccountId,
      ownerId: run.ownerId
    };
    this.showDialog = true;
  }

  viewLines(run: PayRunDto) {
    this.viewing = run;
    this.showLinesDialog = true;
  }

  save() {
    if (!this.form.periodMonth || !this.form.periodYear || !this.form.expenseLedgerAccountId
        || !this.form.deductionsPayableLedgerAccountId || !this.form.ownerId) {
      this.notify.warn('Period, both ledger accounts, and owner are required.');
      return;
    }
    this.saving = true;
    const payload = {
      periodMonth: this.form.periodMonth,
      periodYear: this.form.periodYear,
      expenseLedgerAccountId: this.form.expenseLedgerAccountId,
      deductionsPayableLedgerAccountId: this.form.deductionsPayableLedgerAccountId,
      ownerId: this.form.ownerId
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdatePayRunDto)
      : this.api.create(payload as CreatePayRunDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Pay run ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save pay run.');
      }
    });
  }

  delete(run: PayRunDto) {
    this.confirm.confirm({
      message: `Delete pay run "${run.runNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(run.id).subscribe({
          next: () => {
            this.notify.success('Pay run deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete pay run.')
        });
      }
    });
  }

  process(run: PayRunDto) {
    this.api.process(run.id).subscribe({
      next: () => {
        this.notify.success('Pay run processed.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to process pay run.')
    });
  }

  openPay(run: PayRunDto) {
    this.paying = run;
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
        this.notify.success('Pay run paid and posted to the ledger.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to pay pay run.')
    });
  }

  cancel(run: PayRunDto) {
    this.confirm.confirm({
      message: `Cancel pay run "${run.runNumber}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(run.id).subscribe({
          next: () => {
            this.notify.success('Pay run cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel pay run.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Paid') return 'success';
    if (status === 'Cancelled') return 'danger';
    if (status === 'Processed') return 'warn';
    return 'info';
  }
}
