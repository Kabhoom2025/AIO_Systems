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
  BankReconciliationApiService, BankReconciliationDto, CreateBankReconciliationDto, UpdateBankReconciliationDto,
  CreateBankStatementLineDto, MatchCandidateDto
} from '../../../core/bank-reconciliation-api.service';
import { LedgerAccountApiService, LedgerAccountDto } from '../../../core/ledger-account-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

interface LineForm {
  transactionDate: Date | null;
  description: string | null;
  amount: number | null;
  displayOrder: number;
}

@Component({
  selector: 'app-bank-reconciliations',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './bank-reconciliations.component.html',
  styleUrl: './bank-reconciliations.component.scss'
})
export class BankReconciliationsComponent implements OnInit {
  reconciliations: BankReconciliationDto[] = [];
  ledgerAccounts: LedgerAccountDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: BankReconciliationDto | null = null;
  saving = false;

  form: { ledgerAccountId: number | null; statementDate: Date | null; statementEndingBalance: number | null; ownerId: number | null } = this.emptyForm();
  lines: LineForm[] = [];

  showMatchDialog = false;
  matching: BankReconciliationDto | null = null;
  matchCandidates: MatchCandidateDto[] = [];
  selectedCandidateByLine: Record<number, number | null> = {};

  constructor(
    private api: BankReconciliationApiService,
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
    return { ledgerAccountId: null, statementDate: new Date(), statementEndingBalance: null, ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.reconciliations = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load bank reconciliations.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.lines = [];
    this.showDialog = true;
  }

  openEdit(reconciliation: BankReconciliationDto) {
    this.editing = reconciliation;
    this.form = {
      ledgerAccountId: reconciliation.ledgerAccountId, statementDate: new Date(reconciliation.statementDate),
      statementEndingBalance: reconciliation.statementEndingBalance, ownerId: reconciliation.ownerId
    };
    this.lines = reconciliation.lines
      .slice()
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map(l => ({ transactionDate: new Date(l.transactionDate), description: l.description, amount: l.amount, displayOrder: l.displayOrder }));
    this.showDialog = true;
  }

  addLine() {
    const nextOrder = this.lines.length ? Math.max(...this.lines.map(l => l.displayOrder)) + 1 : 1;
    this.lines.push({ transactionDate: new Date(), description: null, amount: null, displayOrder: nextOrder });
  }

  removeLine(index: number) {
    this.lines.splice(index, 1);
  }

  save() {
    if (!this.form.ownerId || this.form.statementEndingBalance == null || (!this.editing && !this.form.ledgerAccountId)) {
      this.notify.warn('Ledger account, statement ending balance, and owner are required.');
      return;
    }
    if (!this.lines.length || this.lines.some(l => l.amount == null || l.amount === 0)) {
      this.notify.warn('Every line needs a non-zero amount.');
      return;
    }
    this.saving = true;
    const lineDtos: CreateBankStatementLineDto[] = this.lines.map(l => ({
      transactionDate: (l.transactionDate ?? new Date()).toISOString(),
      description: l.description,
      amount: l.amount!,
      displayOrder: l.displayOrder
    }));
    const statementDate = (this.form.statementDate ?? new Date()).toISOString();
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          statementDate,
          statementEndingBalance: this.form.statementEndingBalance,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as UpdateBankReconciliationDto)
      : this.api.create({
          ledgerAccountId: this.form.ledgerAccountId,
          statementDate,
          statementEndingBalance: this.form.statementEndingBalance,
          ownerId: this.form.ownerId,
          lines: lineDtos
        } as CreateBankReconciliationDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Bank reconciliation ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save bank reconciliation.');
      }
    });
  }

  delete(reconciliation: BankReconciliationDto) {
    this.confirm.confirm({
      message: `Delete this bank reconciliation for "${reconciliation.ledgerAccountName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(reconciliation.id).subscribe({
          next: () => {
            this.notify.success('Bank reconciliation deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete bank reconciliation.')
        });
      }
    });
  }

  openMatch(reconciliation: BankReconciliationDto) {
    this.matching = reconciliation;
    this.selectedCandidateByLine = {};
    this.showMatchDialog = true;
    this.api.getMatchCandidates(reconciliation.ledgerAccountId).subscribe({
      next: rows => (this.matchCandidates = rows),
      error: () => (this.matchCandidates = [])
    });
  }

  matchLine(lineId: number) {
    const candidateId = this.selectedCandidateByLine[lineId];
    if (!this.matching || !candidateId) {
      this.notify.warn('Select a journal entry line to match.');
      return;
    }
    this.api.matchLine(this.matching.id, lineId, { journalEntryLineId: candidateId }).subscribe({
      next: updated => {
        this.matching = updated;
        this.notify.success('Line matched.');
        this.api.getMatchCandidates(updated.ledgerAccountId).subscribe({ next: rows => (this.matchCandidates = rows) });
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to match line.')
    });
  }

  unmatchLine(lineId: number) {
    if (!this.matching) return;
    this.api.unmatchLine(this.matching.id, lineId).subscribe({
      next: updated => {
        this.matching = updated;
        this.notify.success('Line unmatched.');
        this.api.getMatchCandidates(updated.ledgerAccountId).subscribe({ next: rows => (this.matchCandidates = rows) });
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to unmatch line.')
    });
  }

  completeFromMatchDialog() {
    if (!this.matching) return;
    this.api.complete(this.matching.id).subscribe({
      next: () => {
        this.showMatchDialog = false;
        this.notify.success('Bank reconciliation completed.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to complete bank reconciliation.')
    });
  }

  statusSeverity(status: string): 'success' | 'info' {
    return status === 'Completed' ? 'success' : 'info';
  }
}
