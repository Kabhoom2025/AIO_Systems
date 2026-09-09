import { Component, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe, TitleCasePipe } from '@angular/common';
import { environment } from '../../../environments/environment';

interface ExpiryAlert {
  medicineId: number; medicineName: string; genericName: string;
  batchId: number; batchNumber: string; expiryDate: string;
  daysToExpiry: number; quantity: number; expiryStatus: string;
  rackLocation?: string;
}

@Component({
  selector: 'app-expiry-alerts',
  standalone: true,
  imports: [DatePipe, TitleCasePipe],
  template: `
    <div class="expiry-page">
      <div class="page-header">
        <h2>Expiry Alerts</h2>
        <div class="filter-buttons">
          <button [class.active]="filterDays() === 30"  (click)="filterDays.set(30)">30 Days</button>
          <button [class.active]="filterDays() === 60"  (click)="filterDays.set(60)">60 Days</button>
          <button [class.active]="filterDays() === 90"  (click)="filterDays.set(90)">90 Days</button>
        </div>
      </div>

      <div class="summary-strip">
        <div class="summary-item expired">
          <span class="count">{{ countByStatus('expired') }}</span>
          <span class="label">Expired</span>
        </div>
        <div class="summary-item critical">
          <span class="count">{{ countByStatus('critical') }}</span>
          <span class="label">Critical (&lt;30d)</span>
        </div>
        <div class="summary-item warning">
          <span class="count">{{ countByStatus('warning') }}</span>
          <span class="label">Warning (&lt;90d)</span>
        </div>
      </div>

      <table class="alerts-table">
        <thead>
          <tr>
            <th>Medicine</th>
            <th>Batch</th>
            <th>Expiry Date</th>
            <th>Days</th>
            <th>Qty</th>
            <th>Rack</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          @for (a of alerts(); track a.batchId) {
            <tr [class]="'row-' + a.expiryStatus">
              <td>
                <b>{{ a.medicineName }}</b>
                <div class="generic">{{ a.genericName }}</div>
              </td>
              <td>{{ a.batchNumber }}</td>
              <td>{{ a.expiryDate | date:'dd MMM yyyy' }}</td>
              <td>
                <span class="days-badge" [class]="a.expiryStatus">
                  {{ a.daysToExpiry < 0 ? 'Expired ' + (-a.daysToExpiry) + 'd ago' : a.daysToExpiry + 'd' }}
                </span>
              </td>
              <td>{{ a.quantity }}</td>
              <td>{{ a.rackLocation ?? '—' }}</td>
              <td><span class="status-badge" [class]="a.expiryStatus">{{ a.expiryStatus | titlecase }}</span></td>
            </tr>
          }
          @empty {
            <tr><td colspan="7" class="empty">No expiry alerts in this range.</td></tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .expiry-page { padding: 1.5rem; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;
      h2 { margin: 0; color: #1a237e; } }
    .filter-buttons { display: flex; gap: 0.5rem;
      button { padding: 6px 16px; border: 1px solid #1a237e; border-radius: 20px; background: #fff; color: #1a237e; cursor: pointer;
        &.active { background: #1a237e; color: #fff; } }
    }
    .summary-strip { display: flex; gap: 1rem; margin-bottom: 1.5rem;
      .summary-item { flex: 1; border-radius: 10px; padding: 1rem; text-align: center;
        .count { display: block; font-size: 2rem; font-weight: 700; }
        .label { font-size: 0.8rem; }
        &.expired { background: #ffebee; color: #b71c1c; }
        &.critical { background: #fff3e0; color: #e65100; }
        &.warning  { background: #fffde7; color: #f57f17; }
      }
    }
    .alerts-table { width: 100%; border-collapse: collapse;
      th { background: #f5f5f5; padding: 10px 12px; text-align: left; font-size: 0.85rem; color: #666; }
      td { padding: 10px 12px; border-bottom: 1px solid #f0f0f0; font-size: 0.85rem; }
      .generic { font-size: 0.75rem; color: #999; }
      tr.row-expired { background: #ffebee; }
      tr.row-critical { background: #fff3e0; }
    }
    .days-badge, .status-badge { padding: 3px 10px; border-radius: 12px; font-size: 0.75rem; font-weight: 700;
      &.expired  { background: #b71c1c; color: #fff; }
      &.critical { background: #e65100; color: #fff; }
      &.warning  { background: #f57f17; color: #fff; }
      &.ok       { background: #43a047; color: #fff; }
    }
    .empty { text-align: center; padding: 2rem; color: #999; }
  `]
})
export class ExpiryAlertsComponent implements OnInit {
  alerts    = signal<ExpiryAlert[]>([]);
  filterDays = signal(90);
  orgId     = 1;

  constructor(private http: HttpClient) {}

  ngOnInit() { this.load(); }

  load() {
    this.http.get<ExpiryAlert[]>(`${environment.apiUrl}/medicines/expiry-alerts/org/${this.orgId}?days=${this.filterDays()}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
    }).subscribe(data => this.alerts.set(data));
  }

  countByStatus(s: string) { return this.alerts().filter(a => a.expiryStatus === s).length; }
}
