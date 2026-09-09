import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { LedgerService } from '../../core/services/ledger.service';
import { SignalRService } from '../../core/services/signalr.service';
import { NotificationService } from '../../shared/services/notification.service';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { LedgerEntryDialogComponent } from './ledger-entry-dialog.component';
import { LedgerEntry } from '../../core/models/ledger.model';
import { AuthService } from '../../core/authentication/auth.service';
import { APP_CONSTANTS } from '../../core/constants/app.constants';

type Period = 'daily' | 'weekly' | 'monthly' | 'yearly';

@Component({
  selector: 'app-ledger',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatProgressSpinnerModule,
    MatTooltipModule, MatDatepickerModule, MatNativeDateModule, DatePipe,
  ],
  templateUrl: './ledger.component.html',
  styleUrl: './ledger.component.scss',
})
export class LedgerComponent implements OnInit, OnDestroy {
  private ledgerService = inject(LedgerService);
  private signalR       = inject(SignalRService);
  private dialog        = inject(MatDialog);
  private notify        = inject(NotificationService);
  private authService   = inject(AuthService);

  private sub = new Subscription();

  entries = signal<LedgerEntry[]>([]);
  loading = signal(false);
  period  = signal<Period>('daily');
  anchor  = signal(new Date());  // reference date for current window

  isAdmin = computed(() => this.authService.userRole() === APP_CONSTANTS.ROLES.ADMIN);

  // Compute from/to/label from period + anchor
  range = computed(() => this.computeRange(this.period(), this.anchor()));

  // Summary derived from entries — no extra API call needed
  summary = computed(() => {
    const list = this.entries();
    const credit = list.filter(e => e.type === 'Credit').reduce((s, e) => s + e.amount, 0);
    const debit  = list.filter(e => e.type === 'Debit').reduce((s, e) => s + e.amount, 0);
    return { totalCredit: credit, totalDebit: debit, net: credit - debit, entryCount: list.length };
  });

  ngOnInit(): void {
    this.load();
    this.signalR.startConnection().then(() => {
      this.sub.add(
        this.signalR.ledgerEntryCreated$.subscribe(() => this.load())
      );
    });
  }

  ngOnDestroy(): void { this.sub.unsubscribe(); }

  setPeriod(p: Period): void {
    this.period.set(p);
    this.anchor.set(new Date());
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const { from, to } = this.range();
    this.ledgerService.getEntries(from, to).subscribe({
      next: res => { this.entries.set(res.data ?? []); this.loading.set(false); },
      error: () => { this.notify.error('Failed to load ledger.'); this.loading.set(false); },
    });
  }

  prev(): void { this.shift(-1); }
  next(): void { this.shift(1); }

  goToday(): void {
    this.anchor.set(new Date());
    this.load();
  }

  isCurrent(): boolean {
    const now = new Date();
    const a   = this.anchor();
    switch (this.period()) {
      case 'daily':   return this.toISO(a) === this.toISO(now);
      case 'weekly':  return this.weekStart(now).getTime() === this.weekStart(a).getTime();
      case 'monthly': return a.getFullYear() === now.getFullYear() && a.getMonth() === now.getMonth();
      case 'yearly':  return a.getFullYear() === now.getFullYear();
    }
  }

  openAdd(): void {
    const ref = this.dialog.open(LedgerEntryDialogComponent, {
      data: { date: this.range().from }, width: '460px', disableClose: true,
    });
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.ledgerService.create(result).subscribe({
        next: () => { this.notify.success('Entry added.'); this.load(); },
        error: err => this.notify.error(err?.error?.message || 'Failed to add entry.'),
      });
    });
  }

  confirmDelete(entry: LedgerEntry): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: { title: 'Delete Entry', message: `Delete this ${entry.type} entry of ₹${entry.amount}?`, confirmText: 'Delete', danger: true },
      width: '400px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.ledgerService.delete(entry.id).subscribe({
        next: () => { this.notify.success('Entry deleted.'); this.load(); },
        error: err => this.notify.error(err?.error?.message || 'Delete failed.'),
      });
    });
  }

  showTime(): boolean { return this.period() === 'daily'; }

  private shift(dir: number): void {
    const d = new Date(this.anchor());
    switch (this.period()) {
      case 'daily':   d.setDate(d.getDate() + dir); break;
      case 'weekly':  d.setDate(d.getDate() + dir * 7); break;
      case 'monthly': d.setMonth(d.getMonth() + dir); break;
      case 'yearly':  d.setFullYear(d.getFullYear() + dir); break;
    }
    this.anchor.set(d);
    this.load();
  }

  private computeRange(period: Period, anchor: Date): { from: string; to: string; label: string } {
    switch (period) {
      case 'daily': {
        const s = this.toISO(anchor);
        return { from: s, to: s, label: anchor.toLocaleDateString('en-IN', { weekday: 'short', day: 'numeric', month: 'long', year: 'numeric' }) };
      }
      case 'weekly': {
        const mon = this.weekStart(anchor);
        const sun = new Date(mon); sun.setDate(mon.getDate() + 6);
        const lbl = `${mon.getDate()} ${mon.toLocaleString('en', { month: 'short' })} – ${sun.getDate()} ${sun.toLocaleString('en', { month: 'short', year: 'numeric' })}`;
        return { from: this.toISO(mon), to: this.toISO(sun), label: lbl };
      }
      case 'monthly': {
        const first = new Date(anchor.getFullYear(), anchor.getMonth(), 1);
        const last  = new Date(anchor.getFullYear(), anchor.getMonth() + 1, 0);
        return { from: this.toISO(first), to: this.toISO(last), label: anchor.toLocaleString('en-IN', { month: 'long', year: 'numeric' }) };
      }
      case 'yearly': {
        const first = new Date(anchor.getFullYear(), 0, 1);
        const last  = new Date(anchor.getFullYear(), 11, 31);
        return { from: this.toISO(first), to: this.toISO(last), label: anchor.getFullYear().toString() };
      }
    }
  }

  private weekStart(d: Date): Date {
    const day = d.getDay(); // 0=Sun
    const mon = new Date(d);
    mon.setDate(d.getDate() - (day === 0 ? 6 : day - 1));
    mon.setHours(0, 0, 0, 0);
    return mon;
  }

  private toISO(d: Date): string {
    return d.toISOString().split('T')[0];
  }
}
