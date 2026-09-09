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
  JournalEntryApiService, JournalEntryDto, CreateJournalEntryDto, UpdateJournalEntryDto, CreateJournalEntryLineDto
} from '../../../core/journal-entry-api.service';
import { LedgerAccountApiService, LedgerAccountDto } from '../../../core/ledger-account-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

interface LineForm {
  ledgerAccountId: number | null;
  debit: number | null;
  credit: number | null;
  displayOrder: number;
}

@Component({
  selector: 'app-journal-entries',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './journal-entries.component.html',
  styleUrl: './journal-entries.component.scss'
})
export class JournalEntriesComponent implements OnInit {
  entries: JournalEntryDto[] = [];
  ledgerAccounts: LedgerAccountDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: JournalEntryDto | null = null;
  saving = false;

  form: { entryDate: Date | null; description: string | null; ownerId: number | null } = this.emptyForm();
  lines: LineForm[] = [];

  constructor(
    private api: JournalEntryApiService,
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
    return { entryDate: new Date(), description: null, ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.entries = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load journal entries.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.lines = [];
    this.showDialog = true;
  }

  openEdit(entry: JournalEntryDto) {
    this.editing = entry;
    this.form = { entryDate: new Date(entry.entryDate), description: entry.description, ownerId: entry.ownerId };
    this.lines = entry.lines
      .slice()
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map(l => ({ ledgerAccountId: l.ledgerAccountId, debit: l.debit || null, credit: l.credit || null, displayOrder: l.displayOrder }));
    this.showDialog = true;
  }

  addLine() {
    const nextOrder = this.lines.length ? Math.max(...this.lines.map(l => l.displayOrder)) + 1 : 1;
    this.lines.push({ ledgerAccountId: null, debit: null, credit: null, displayOrder: nextOrder });
  }

  removeLine(index: number) {
    this.lines.splice(index, 1);
  }

  totalDebit(): number {
    return this.lines.reduce((sum, l) => sum + (l.debit ?? 0), 0);
  }

  totalCredit(): number {
    return this.lines.reduce((sum, l) => sum + (l.credit ?? 0), 0);
  }

  isBalanced(): boolean {
    return this.lines.length > 0 && this.totalDebit() === this.totalCredit() && this.totalDebit() > 0;
  }

  save() {
    if (!this.form.ownerId) {
      this.notify.warn('Owner is required.');
      return;
    }
    if (!this.lines.length || this.lines.some(l => !l.ledgerAccountId)) {
      this.notify.warn('Every line needs a ledger account.');
      return;
    }
    if (!this.isBalanced()) {
      this.notify.warn('Total debits must equal total credits.');
      return;
    }
    this.saving = true;
    const lineDtos: CreateJournalEntryLineDto[] = this.lines.map(l => ({
      ledgerAccountId: l.ledgerAccountId!,
      debit: l.debit ?? 0,
      credit: l.credit ?? 0,
      description: null,
      displayOrder: l.displayOrder
    }));
    const entryDate = (this.form.entryDate ?? new Date()).toISOString();
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          entryDate,
          description: this.form.description,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as UpdateJournalEntryDto)
      : this.api.create({
          entryDate,
          description: this.form.description,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as CreateJournalEntryDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Journal entry ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save journal entry.');
      }
    });
  }

  delete(entry: JournalEntryDto) {
    this.confirm.confirm({
      message: `Delete journal entry "${entry.entryNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(entry.id).subscribe({
          next: () => {
            this.notify.success('Journal entry deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete journal entry.')
        });
      }
    });
  }

  post(entry: JournalEntryDto) {
    this.api.post(entry.id).subscribe({
      next: () => {
        this.notify.success('Journal entry posted.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to post journal entry.')
    });
  }

  void(entry: JournalEntryDto) {
    this.confirm.confirm({
      message: `Void journal entry "${entry.entryNumber}"?`,
      header: 'Confirm Void',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.void(entry.id).subscribe({
          next: () => {
            this.notify.success('Journal entry voided.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to void journal entry.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Posted') return 'success';
    if (status === 'Voided') return 'danger';
    return 'info';
  }
}
