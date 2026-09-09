import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule, CurrencyPipe, DatePipe, TitleCasePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

interface OrgStat {
  id: number; name: string; isActive: boolean; userCount: number;
  enabledModules: string[];
  restaurant?: { todayOrders: number; todayRevenue: number; totalOrders: number; totalRevenue: number; };
  pharmacy?: { totalMedicines: number; expiryAlerts: number; pendingPrescriptions: number; };
}

interface PlatformSummary {
  totalOrgs: number; activeOrgs: number; totalUsers: number;
  restaurant: { todayOrders: number; todayRevenue: number; weekOrders: number; weekRevenue: number; monthRevenue: number; };
  pharmacy: { totalMedicines: number; expiryAlerts: number; pendingRx: number; };
  apis: { name: string; url: string; status: 'up' | 'down' | 'checking'; latency: number; }[];
}

@Component({
  selector: 'app-sa-monitor',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, TitleCasePipe, RouterModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './sa-monitor.component.html',
  styleUrl: './sa-monitor.component.scss'
})
export class SaMonitorComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);

  loading    = signal(true);
  lastUpdate = signal<Date>(new Date());
  orgs       = signal<OrgStat[]>([]);
  summary    = signal<PlatformSummary>({
    totalOrgs: 0, activeOrgs: 0, totalUsers: 0,
    restaurant: { todayOrders: 0, todayRevenue: 0, weekOrders: 0, weekRevenue: 0, monthRevenue: 0 },
    pharmacy: { totalMedicines: 0, expiryAlerts: 0, pendingRx: 0 },
    apis: [
      { name: 'API Gateway',      url: `${environment.apiUrl}/system-health`,          status: 'checking', latency: 0 },
      { name: 'Restaurant API',   url: `${environment.apiUrl}/report/summary`,         status: 'checking', latency: 0 },
      { name: 'Pharmacy API',     url: `${environment.apiUrl}/pharmacy/system-health`, status: 'checking', latency: 0 },
    ]
  });

  private refreshTimer: ReturnType<typeof setInterval> | null = null;

  get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` });
  }

  ngOnInit() {
    this.loadAll();
    this.refreshTimer = setInterval(() => this.loadAll(), 30000); // refresh every 30s
  }

  ngOnDestroy() {
    if (this.refreshTimer) clearInterval(this.refreshTimer);
  }

  loadAll() {
    this.loading.set(true);
    this.checkApiHealth();

    forkJoin({
      orgs:        this.http.get<any[]>(`${environment.apiUrl}/organizations`, { headers: this.headers }).pipe(catchError(() => of([]))),
      reportSum:   this.http.get<any>(`${environment.apiUrl}/report/summary`, { headers: this.headers }).pipe(catchError(() => of(null))),
      orgModules:  this.http.get<any[]>(`${environment.apiUrl}/platform-modules/org-status`, { headers: this.headers }).pipe(catchError(() => of([]))),
    }).subscribe(({ orgs, reportSum, orgModules }) => {
      const moduleMap: Record<number, string[]> = {};
      (orgModules ?? []).forEach((o: any) => {
        const id = o.organizationId ?? o.OrganizationId;
        moduleMap[id] = (o.modules ?? o.Modules ?? [])
          .filter((m: any) => m.isEnabled ?? m.IsEnabled)
          .map((m: any) => m.moduleKey ?? m.ModuleKey ?? '');
      });

      const orgStats: OrgStat[] = (orgs ?? []).map((o: any) => ({
        id: o.id, name: o.name, isActive: o.isActive, userCount: o.userCount,
        enabledModules: moduleMap[o.id] ?? []
      }));

      const pharmacyOrgs = orgStats.filter(o => o.enabledModules.includes('pharmacy'));

      // Load per-org restaurant reports
      const orgReportCalls = orgStats.map(o =>
        this.http.get<any>(`${environment.apiUrl}/organizations/${o.id}/reports`, { headers: this.headers })
          .pipe(catchError(() => of(null)))
      );

      // Load pharmacy stats per pharmacy org
      const pharmacyMedCalls = pharmacyOrgs.map(o =>
        this.http.get<any[]>(`${environment.apiUrl}/pharmacy/medicines/org/${o.id}`, { headers: this.headers })
          .pipe(catchError(() => of([])))
      );
      const pharmacyExpiryCalls = pharmacyOrgs.map(o =>
        this.http.get<any[]>(`${environment.apiUrl}/pharmacy/medicines/expiry-alerts/org/${o.id}?days=90`, { headers: this.headers })
          .pipe(catchError(() => of([])))
      );
      const pharmacyRxCalls = pharmacyOrgs.map(o =>
        this.http.get<any[]>(`${environment.apiUrl}/pharmacy/prescriptions/org/${o.id}`, { headers: this.headers })
          .pipe(catchError(() => of([])))
      );

      forkJoin({
        orgReports:    orgReportCalls.length  ? forkJoin(orgReportCalls)     : of([]),
        pharmMeds:     pharmacyMedCalls.length ? forkJoin(pharmacyMedCalls)  : of([]),
        pharmExpiry:   pharmacyExpiryCalls.length ? forkJoin(pharmacyExpiryCalls) : of([]),
        pharmRx:       pharmacyRxCalls.length  ? forkJoin(pharmacyRxCalls)   : of([]),
      }).subscribe(({ orgReports, pharmMeds, pharmExpiry, pharmRx }) => {
        // Attach restaurant stats to each org
        (orgReports as any[]).forEach((r, i) => {
          if (r) {
            orgStats[i].restaurant = {
              todayOrders: r.todayOrders ?? 0,
              todayRevenue: r.todayRevenue ?? 0,
              totalOrders: r.totalOrders ?? 0,
              totalRevenue: r.totalRevenue ?? 0,
            };
          }
        });

        // Attach pharmacy stats to each pharmacy org
        pharmacyOrgs.forEach((o, i) => {
          const org = orgStats.find(s => s.id === o.id);
          if (org) {
            const meds   = (pharmMeds as any[][])[i] ?? [];
            const expiry = (pharmExpiry as any[][])[i] ?? [];
            const rx     = (pharmRx as any[][])[i] ?? [];
            org.pharmacy = {
              totalMedicines: meds.length,
              expiryAlerts: expiry.filter((e: any) => e.expiryStatus !== 'ok').length,
              pendingPrescriptions: rx.filter((r: any) => r.status === 'Pending').length,
            };
          }
        });

        this.orgs.set(orgStats);

        // Build platform summary
        const allMeds   = (pharmMeds as any[][]).flat();
        const allExpiry = (pharmExpiry as any[][]).flat().filter((e: any) => e.expiryStatus !== 'ok');
        const allRx     = (pharmRx as any[][]).flat().filter((r: any) => r.status === 'Pending');

        this.summary.update(s => ({
          ...s,
          totalOrgs:  orgStats.length,
          activeOrgs: orgStats.filter(o => o.isActive).length,
          totalUsers: orgStats.reduce((sum, o) => sum + (o.userCount ?? 0), 0),
          restaurant: {
            todayOrders:  reportSum?.todayOrders ?? 0,
            todayRevenue: reportSum?.todayRevenue ?? 0,
            weekOrders:   reportSum?.weekOrders ?? 0,
            weekRevenue:  reportSum?.weekRevenue ?? 0,
            monthRevenue: reportSum?.monthRevenue ?? 0,
          },
          pharmacy: {
            totalMedicines: allMeds.length,
            expiryAlerts:   allExpiry.length,
            pendingRx:      allRx.length,
          }
        }));

        this.loading.set(false);
        this.lastUpdate.set(new Date());
      });
    });
  }

  private checkApiHealth() {
    const apis = this.summary().apis;
    apis.forEach((api, idx) => {
      const start = Date.now();
      this.http.get(api.url, { headers: this.headers }).pipe(catchError(() => of(null))).subscribe(res => {
        this.summary.update(s => {
          const updated = [...s.apis];
          updated[idx] = { ...updated[idx], status: res !== null ? 'up' : 'down', latency: Date.now() - start };
          return { ...s, apis: updated };
        });
      });
    });
  }

  moduleColor(key: string): string {
    const colors: Record<string, string> = {
      restaurant: '#e53935', pharmacy: '#1a237e', hr_management: '#2e7d32',
      retail: '#e65100', finance: '#6a1b9a'
    };
    return colors[key] ?? '#757575';
  }

  moduleIcon(key: string): string {
    const icons: Record<string, string> = {
      restaurant: 'restaurant', pharmacy: 'local_pharmacy', hr_management: 'badge',
      retail: 'storefront', finance: 'account_balance'
    };
    return icons[key] ?? 'apps';
  }
}
