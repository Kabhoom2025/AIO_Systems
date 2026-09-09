import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  LedgerAccountApiService, LedgerAccountDto, CreateLedgerAccountDto, UpdateLedgerAccountDto
} from '../../../core/ledger-account-api.service';

const ACCOUNT_TYPES = ['Asset', 'Liability', 'Equity', 'Revenue', 'Expense'];

@Component({
  selector: 'app-ledger-accounts',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, DropdownModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './ledger-accounts.component.html',
  styleUrl: './ledger-accounts.component.scss'
})
export class LedgerAccountsComponent implements OnInit {
  accounts: LedgerAccountDto[] = [];
  accountTypes = ACCOUNT_TYPES;
  loading = false;

  showDialog = false;
  editing: LedgerAccountDto | null = null;
  saving = false;

  form: { code: string; name: string; type: string; isActive: boolean } = this.emptyForm();

  constructor(
    private api: LedgerAccountApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm() {
    return { code: '', name: '', type: 'Asset', isActive: true };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.accounts = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load ledger accounts.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(account: LedgerAccountDto) {
    this.editing = account;
    this.form = { code: account.code, name: account.name, type: account.type, isActive: account.isActive };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || (!this.editing && !this.form.code)) {
      this.notify.warn('Code and name are required.');
      return;
    }
    this.saving = true;
    const payload = { name: this.form.name, type: this.form.type, isActive: this.form.isActive };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateLedgerAccountDto)
      : this.api.create({ ...payload, code: this.form.code } as CreateLedgerAccountDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Ledger account ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save ledger account.');
      }
    });
  }

  delete(account: LedgerAccountDto) {
    this.confirm.confirm({
      message: `Delete ledger account "${account.code}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(account.id).subscribe({
          next: () => {
            this.notify.success('Ledger account deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete ledger account.')
        });
      }
    });
  }

  typeSeverity(type: string): 'success' | 'danger' | 'info' | 'warn' {
    if (type === 'Asset') return 'success';
    if (type === 'Liability') return 'danger';
    if (type === 'Revenue') return 'info';
    return 'warn';
  }
}
