import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ReservationService } from '../../core/services/reservation.service';
import { NotificationService } from '../../shared/services/notification.service';
import { ReservationDto, ReservationStatus } from '../../core/models/reservation.model';
import { Table } from '../../core/models/table.model';

export interface ReservationDialogData { tables: Table[]; }

type StatusFilter = 'All' | 'Upcoming' | 'Pending' | 'Confirmed' | 'Seated' | 'Cancelled' | 'NoShow';

@Component({
  selector: 'app-reservation-dialog',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatTooltipModule, MatProgressSpinnerModule,
  ],
  template: `
<div class="rd-wrap">

  <!-- Header -->
  <div class="rd-header">
    <div class="rd-header-left">
      <mat-icon>event_available</mat-icon>
      <div>
        <h2>Table Reservations</h2>
        <span class="rd-sub">Manage upcoming guest bookings</span>
      </div>
    </div>
    <button mat-icon-button (click)="close()"><mat-icon>close</mat-icon></button>
  </div>

  <div class="rd-body">

    <!-- ── Left: Create Form ───────────────────────────── -->
    <div class="rd-form-col">
      <div class="rd-form-card">
        <div class="rd-form-title">
          <mat-icon>add_circle_outline</mat-icon>
          <span>New Reservation</span>
        </div>

        <form [formGroup]="form" (ngSubmit)="createReservation()" class="rd-form">

          <mat-form-field appearance="outline" class="rd-field">
            <mat-label>Table</mat-label>
            <mat-select formControlName="tableId">
              @for (t of data.tables; track t.id) {
                <mat-option [value]="t.id">
                  Table #{{ t.tableNumber }} — {{ t.hall }} ({{ t.capacity }} seats)
                </mat-option>
              }
            </mat-select>
            <mat-icon matPrefix>table_restaurant</mat-icon>
          </mat-form-field>

          <mat-form-field appearance="outline" class="rd-field">
            <mat-label>Guest Name</mat-label>
            <input matInput formControlName="guestName" placeholder="John Doe" />
            <mat-icon matPrefix>person</mat-icon>
          </mat-form-field>

          <div class="rd-row">
            <mat-form-field appearance="outline" class="rd-field">
              <mat-label>Phone Number</mat-label>
              <input matInput formControlName="guestPhone" placeholder="+91 98765 43210" />
              <mat-icon matPrefix>phone</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline" class="rd-field rd-field-xs">
              <mat-label>Party Size</mat-label>
              <input matInput type="number" formControlName="partySize" min="1" placeholder="2" />
              <mat-icon matPrefix>group</mat-icon>
            </mat-form-field>
          </div>

          <div class="rd-row">
            <mat-form-field appearance="outline" class="rd-field">
              <mat-label>Date</mat-label>
              <input matInput type="date" formControlName="reservationDate" />
            </mat-form-field>

            <mat-form-field appearance="outline" class="rd-field">
              <mat-label>Time</mat-label>
              <input matInput type="time" formControlName="reservationTime" />
            </mat-form-field>
          </div>

          <mat-form-field appearance="outline" class="rd-field">
            <mat-label>Notes (optional)</mat-label>
            <textarea matInput formControlName="notes" rows="2" placeholder="Special requests…"></textarea>
            <mat-icon matPrefix>notes</mat-icon>
          </mat-form-field>

          <button mat-raised-button color="primary" type="submit"
            [disabled]="form.invalid || saving()" class="rd-submit-btn">
            @if (saving()) { <mat-spinner diameter="18" /> }
            @else { <mat-icon>check</mat-icon> }
            Save Reservation
          </button>
        </form>
      </div>
    </div>

    <!-- ── Right: Reservations List ───────────────────── -->
    <div class="rd-list-col">

      <!-- Filter chips -->
      <div class="rd-filters">
        @for (f of statusFilters; track f) {
          <button class="rd-chip" [class.active]="statusFilter() === f" (click)="statusFilter.set(f)">
            {{ f === 'NoShow' ? 'No-Show' : f }}
            <span class="rd-chip-count">{{ countByFilter(f) }}</span>
          </button>
        }
        <div class="rd-spacer"></div>
        <mat-form-field appearance="outline" class="rd-search">
          <input matInput [ngModel]="searchTerm()" (ngModelChange)="searchTerm.set($event)"
            placeholder="Search guest…" />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
      </div>

      <!-- List -->
      @if (loading()) {
        <div class="rd-center"><mat-spinner diameter="36" /></div>
      } @else if (filtered().length === 0) {
        <div class="rd-empty">
          <mat-icon>event_busy</mat-icon>
          <span>No reservations found</span>
        </div>
      } @else {
        <div class="rd-list">
          @for (r of filtered(); track r.id) {
            <div class="rd-card" [class]="'rdc-' + r.status.toLowerCase()">

              <div class="rdc-top">
                <div class="rdc-table-badge">Table #{{ r.tableNumber }}</div>
                <span class="rdc-status" [class]="'st-' + r.status.toLowerCase()">
                  {{ r.status === 'NoShow' ? 'No-Show' : r.status }}
                </span>
              </div>

              <div class="rdc-guest">
                <mat-icon>person</mat-icon>
                <span>{{ r.guestName }}</span>
              </div>

              <div class="rdc-meta">
                <span class="rdc-chip"><mat-icon>phone</mat-icon>{{ r.guestPhone }}</span>
                <span class="rdc-chip"><mat-icon>group</mat-icon>{{ r.partySize }} guests</span>
                <span class="rdc-chip"><mat-icon>schedule</mat-icon>{{ r.reservationDateTime | date:'dd MMM, h:mm a' }}</span>
                @if (r.hall) {
                  <span class="rdc-chip hall"><mat-icon>meeting_room</mat-icon>{{ r.hall }}</span>
                }
              </div>

              @if (r.notes) {
                <div class="rdc-notes">{{ r.notes }}</div>
              }

              <!-- Actions -->
              <div class="rdc-actions">
                @if (r.status === 'Pending') {
                  <button class="rda-btn confirm" (click)="setStatus(r, 'Confirmed')" matTooltip="Confirm">
                    <mat-icon>check_circle</mat-icon> Confirm
                  </button>
                }
                @if (r.status === 'Confirmed') {
                  <button class="rda-btn seat" (click)="setStatus(r, 'Seated')" matTooltip="Seat Guest">
                    <mat-icon>airline_seat_recline_normal</mat-icon> Seat
                  </button>
                }
                @if (r.status !== 'Cancelled' && r.status !== 'Seated' && r.status !== 'NoShow') {
                  <button class="rda-btn no-show" (click)="setStatus(r, 'NoShow')" matTooltip="No Show">
                    <mat-icon>person_off</mat-icon>
                  </button>
                  <button class="rda-btn cancel" (click)="setStatus(r, 'Cancelled')" matTooltip="Cancel">
                    <mat-icon>cancel</mat-icon>
                  </button>
                }
                <button class="rda-btn delete" (click)="deleteReservation(r)" matTooltip="Delete">
                  <mat-icon>delete_outline</mat-icon>
                </button>
              </div>

            </div>
          }
        </div>
      }
    </div>

  </div>
</div>
  `,
  styles: [`
.rd-wrap { display: flex; flex-direction: column; height: 100%; overflow: hidden; }

.rd-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 20px 24px 16px; border-bottom: 1px solid #f0f0f0;
  flex-shrink: 0;
  .rd-header-left { display: flex; align-items: center; gap: 12px; }
  mat-icon { font-size: 28px; width: 28px; height: 28px; color: var(--primary); }
  h2 { margin: 0; font-size: 1.25rem; font-weight: 700; color: #1a1a1a; }
  .rd-sub { font-size: .8rem; color: #9e9e9e; }
}

.rd-body {
  display: flex; flex: 1; overflow: hidden;
  gap: 0;
}

/* ── Form column ── */
.rd-form-col {
  width: 360px; flex-shrink: 0;
  overflow-y: auto; padding: 20px 20px 20px 24px;
  border-right: 1px solid #f0f0f0;
}

.rd-form-card {
  background: #fafafa; border: 1.5px dashed #e0e0e0;
  border-radius: 14px; padding: 18px 16px;
}

.rd-form-title {
  display: flex; align-items: center; gap: 7px;
  font-size: .92rem; font-weight: 700; color: #424242;
  margin-bottom: 14px;
  mat-icon { font-size: 18px; width: 18px; height: 18px; color: var(--primary); }
}

.rd-form { display: flex; flex-direction: column; gap: 4px; }

.rd-field {
  width: 100%;
  ::ng-deep .mat-mdc-form-field-subscript-wrapper { display: none; }
  ::ng-deep input[type="date"],
  ::ng-deep input[type="time"] {
    min-width: 0;
    box-sizing: border-box;
  }
}

.rd-field-xs {
  flex: 0 0 88px !important;
  min-width: 88px;
}

.rd-row { display: flex; gap: 8px; align-items: flex-start; }

.rd-submit-btn {
  width: 100%; margin-top: 8px;
  display: flex; align-items: center; gap: 6px;
  mat-spinner { --mdc-circular-progress-active-indicator-color: #fff; }
}

/* ── List column ── */
.rd-list-col {
  flex: 1; overflow: hidden; display: flex; flex-direction: column;
  padding: 16px 24px 20px 20px;
}

.rd-filters {
  display: flex; align-items: center; gap: 6px; flex-wrap: wrap;
  margin-bottom: 14px; flex-shrink: 0;
}

.rd-spacer { flex: 1; }

.rd-search {
  width: 200px;
  ::ng-deep .mat-mdc-form-field-subscript-wrapper { display: none; }
  ::ng-deep .mat-mdc-text-field-wrapper { padding: 0 10px; }
  ::ng-deep .mat-mdc-form-field-infix { padding-top: 8px; padding-bottom: 6px; min-height: 38px; }
}

.rd-chip {
  display: flex; align-items: center; gap: 5px;
  padding: 5px 12px; border-radius: 20px; border: 1.5px solid #e0e0e0;
  background: #fff; cursor: pointer; font-size: .78rem; font-weight: 600; color: #757575;
  transition: all .18s;
  .rd-chip-count { background: rgba(0,0,0,.08); border-radius: 10px; padding: 1px 6px; font-size: .7rem; }
  &:hover { border-color: var(--primary); color: var(--primary); }
  &.active { background: var(--primary); border-color: var(--primary); color: #fff;
    .rd-chip-count { background: rgba(255,255,255,.25); } }
}

.rd-list {
  flex: 1; overflow-y: auto; display: flex; flex-direction: column; gap: 10px;
  padding-right: 4px;
}

.rd-center { display: flex; align-items: center; justify-content: center; flex: 1; }

.rd-empty {
  flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 10px; color: #bdbdbd;
  mat-icon { font-size: 48px; width: 48px; height: 48px; }
  span { font-size: .9rem; }
}

/* ── Reservation card ── */
.rd-card {
  background: #fff; border-radius: 12px;
  border: 1.5px solid #f0f0f0; border-left: 4px solid #e0e0e0;
  padding: 12px 14px 10px; display: flex; flex-direction: column; gap: 7px;
  box-shadow: 0 1px 4px rgba(0,0,0,.05);
  transition: box-shadow .18s;
  &:hover { box-shadow: 0 3px 10px rgba(0,0,0,.09); }

  &.rdc-pending    { border-left-color: #f59e0b; }
  &.rdc-confirmed  { border-left-color: #3b82f6; }
  &.rdc-seated     { border-left-color: #22c55e; }
  &.rdc-cancelled  { border-left-color: #ef4444; opacity: .75; }
  &.rdc-noshow     { border-left-color: #9e9e9e; opacity: .75; }
}

.rdc-top {
  display: flex; align-items: center; justify-content: space-between;
}

.rdc-table-badge {
  font-size: .78rem; font-weight: 700; color: #424242;
  background: #f5f5f5; padding: 2px 8px; border-radius: 8px;
}

.rdc-status {
  font-size: .68rem; font-weight: 700; padding: 2px 9px; border-radius: 20px; text-transform: capitalize;
  &.st-pending    { background: #fff3e0; color: #e65100; }
  &.st-confirmed  { background: #e3f2fd; color: #1565c0; }
  &.st-seated     { background: #e8f5e9; color: #1b5e20; }
  &.st-cancelled  { background: #ffebee; color: #b71c1c; }
  &.st-noshow     { background: #f5f5f5; color: #757575; }
}

.rdc-guest {
  display: flex; align-items: center; gap: 5px;
  font-size: .88rem; font-weight: 700; color: #212121;
  mat-icon { font-size: 16px; width: 16px; height: 16px; color: #bdbdbd; }
}

.rdc-meta {
  display: flex; gap: 8px; flex-wrap: wrap;
}

.rdc-chip {
  display: flex; align-items: center; gap: 3px;
  font-size: .72rem; color: #757575;
  mat-icon { font-size: 13px; width: 13px; height: 13px; color: #bdbdbd; }
  &.hall { background: #f3e5f5; color: #6a1b9a; border-radius: 8px; padding: 1px 6px; }
}

.rdc-notes {
  font-size: .75rem; color: #9e9e9e; font-style: italic;
  padding: 4px 8px; background: #fafafa; border-radius: 6px;
}

.rdc-actions {
  display: flex; gap: 5px; flex-wrap: wrap; padding-top: 4px;
  border-top: 1px solid #f5f5f5;
}

.rda-btn {
  display: flex; align-items: center; gap: 3px;
  padding: 3px 9px; border-radius: 6px; border: 1px solid #e0e0e0;
  background: #fafafa; cursor: pointer; font-size: .7rem; font-weight: 600;
  transition: all .15s;
  mat-icon { font-size: 14px; width: 14px; height: 14px; }

  &.confirm  { color: #1565c0; border-color: #bbdefb; &:hover { background: #e3f2fd; } }
  &.seat     { color: #1b5e20; border-color: #c8e6c9; &:hover { background: #e8f5e9; } }
  &.no-show  { color: #757575; &:hover { background: #f5f5f5; } }
  &.cancel   { color: #b71c1c; border-color: #ffcdd2; &:hover { background: #ffebee; } }
  &.delete   { color: #9e9e9e; margin-left: auto; &:hover { color: #d32f2f; background: #ffebee; border-color: #ffcdd2; } }
}
  `],
})
export class ReservationDialogComponent implements OnInit {
  private dialogRef   = inject(MatDialogRef<ReservationDialogComponent>);
  readonly data       = inject<ReservationDialogData>(MAT_DIALOG_DATA);
  private resSvc      = inject(ReservationService);
  private notify      = inject(NotificationService);
  private fb          = inject(FormBuilder);

  reservations = signal<ReservationDto[]>([]);
  loading      = signal(false);
  saving       = signal(false);
  statusFilter = signal<StatusFilter>('Upcoming');
  searchTerm   = signal('');

  readonly statusFilters: StatusFilter[] = ['Upcoming', 'All', 'Pending', 'Confirmed', 'Seated', 'Cancelled', 'NoShow'];

  form = this.fb.group({
    tableId:         [null as number | null, Validators.required],
    guestName:       ['', [Validators.required, Validators.minLength(2)]],
    guestPhone:      ['', Validators.required],
    partySize:       [2,  [Validators.required, Validators.min(1)]],
    reservationDate: ['', Validators.required],
    reservationTime: ['', Validators.required],
    notes:           [''],
  });

  filtered = computed(() => {
    const sf  = this.statusFilter();
    const now = new Date();
    const q   = this.searchTerm().toLowerCase();

    let list = this.reservations();

    if (sf === 'Upcoming') {
      list = list.filter(r =>
        new Date(r.reservationDateTime) >= now &&
        r.status !== 'Cancelled' && r.status !== 'NoShow'
      );
    } else if (sf !== 'All') {
      list = list.filter(r => r.status === sf);
    }

    if (q) list = list.filter(r =>
      r.guestName.toLowerCase().includes(q) ||
      r.guestPhone.includes(q)
    );

    return list;
  });

  countByFilter(f: StatusFilter): number {
    const now  = new Date();
    const all  = this.reservations();
    if (f === 'All')     return all.length;
    if (f === 'Upcoming') return all.filter(r =>
      new Date(r.reservationDateTime) >= now &&
      r.status !== 'Cancelled' && r.status !== 'NoShow'
    ).length;
    return all.filter(r => r.status === f).length;
  }

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.resSvc.getAll().subscribe({
      next: (res) => { this.reservations.set(res.data ?? []); this.loading.set(false); },
      error: () => { this.notify.error('Failed to load reservations.'); this.loading.set(false); },
    });
  }

  createReservation(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const v = this.form.getRawValue();

    const combinedDt = new Date(`${v.reservationDate}T${v.reservationTime}:00`);

    this.resSvc.create({
      tableId:             v.tableId!,
      guestName:           v.guestName!,
      guestPhone:          v.guestPhone!,
      partySize:           v.partySize!,
      reservationDateTime: combinedDt.toISOString(),
      notes:               v.notes || undefined,
    }).subscribe({
      next: (res) => {
        this.notify.success('Reservation created.');
        this.form.reset({ partySize: 2 });
        this.saving.set(false);
        this.load();
      },
      error: (err) => {
        this.notify.error(err?.error?.message || 'Failed to create reservation.');
        this.saving.set(false);
      },
    });
  }

  setStatus(r: ReservationDto, status: string): void {
    this.resSvc.updateStatus(r.id, { status: status as any }).subscribe({
      next: () => { this.notify.success(`Reservation marked as ${status}.`); this.load(); },
      error: (err) => this.notify.error(err?.error?.message || 'Status update failed.'),
    });
  }

  deleteReservation(r: ReservationDto): void {
    this.resSvc.delete(r.id).subscribe({
      next: () => { this.notify.success('Reservation deleted.'); this.load(); },
      error: (err) => this.notify.error(err?.error?.message || 'Delete failed.'),
    });
  }

  close(): void { this.dialogRef.close(); }
}
