import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { TooltipModule } from 'primeng/tooltip';
import { catchError, of } from 'rxjs';
import { OrganizationApiService, OrganizationDto } from '../../core/organization-api.service';
import { BranchApiService } from '../../core/branch-api.service';
import { DepartmentApiService } from '../../core/department-api.service';
import { UserApiService } from '../../core/user-api.service';
import { RouterModule } from '@angular/router';
import { DashboardBuilderApiService, WidgetDataDto } from '../../core/dashboard-builder-api.service';

interface StatTile {
  label: string;
  value: string;
  icon: string;
  accent: string;
}

/// Fixed categorical hue order (colorblind-validated — never cycled/reassigned per filter) for
/// the status-breakdown charts below. Reused across all three charts so the same status label
/// (e.g. "Open") always reads as the same hue everywhere on the page.
const CHART_PALETTE = ['#2563eb', '#0ca30c', '#eda100', '#7c3aed', '#1baf7a', '#e34948', '#e87ba4', '#334155'];

type ChartKind = 'bar' | 'doughnut' | 'polarArea';

interface ChartPanel {
  key: string;
  title: string;
  icon: string;
  type: ChartKind;
  options: object;
  fixedHeight?: string;
  data: { labels: string[]; datasets: { data: number[]; backgroundColor: string[]; borderRadius?: number; maxBarThickness?: number; borderWidth?: number; borderColor?: string }[] } | null;
  loading: boolean;
  empty: boolean;
}

interface ProportionSegment {
  label: string;
  count: number;
  percent: number;
  color: string;
}

/// One sensible chart form + icon per known widget type — same "pick the form for the job"
/// reasoning as before, just applied per widgetType instead of hardcoded per panel, since the
/// panel list itself is now whatever the user configured in Dashboard Builder. PosSalesTotal is
/// deliberately absent: it's a single value, not a breakdown, so it's rendered as the hero stat
/// tile above instead of a chart panel if the user has it on their Builder dashboard.
const WIDGET_PRESENTATION: Record<string, { type: ChartKind; icon: string }> = {
  SalesOrderStatusSummary: { type: 'doughnut', icon: 'pi-chart-pie' },
  ServiceTicketStatusSummary: { type: 'polarArea', icon: 'pi-ticket' },
  ProjectTaskStatusSummary: { type: 'bar', icon: 'pi-briefcase' },
  PurchaseOrderStatusSummary: { type: 'bar', icon: 'pi-shopping-bag' },
  ShipmentStatusSummary: { type: 'doughnut', icon: 'pi-truck' },
  RfqStatusSummary: { type: 'polarArea', icon: 'pi-file' }
};
const DEFAULT_PRESENTATION = { type: 'bar' as ChartKind, icon: 'pi-chart-bar' };

const BAR_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  indexAxis: 'y' as const,
  plugins: {
    legend: { display: false },
    tooltip: { padding: 10, cornerRadius: 6 }
  },
  scales: {
    x: {
      beginAtZero: true,
      ticks: { precision: 0, color: '#898781' },
      grid: { color: '#e1e0d9' }
    },
    y: {
      ticks: { color: '#52514e', font: { size: 12 } },
      grid: { display: false }
    }
  }
};

const DOUGHNUT_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '62%',
  plugins: {
    legend: { position: 'bottom' as const, labels: { color: '#52514e', usePointStyle: true, boxWidth: 8, padding: 14 } },
    tooltip: { padding: 10, cornerRadius: 6 }
  }
};

const POLAR_AREA_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'bottom' as const, labels: { color: '#52514e', usePointStyle: true, boxWidth: 8, padding: 14 } },
    tooltip: { padding: 10, cornerRadius: 6 }
  },
  scales: {
    r: {
      ticks: { display: false, backdropColor: 'transparent' },
      grid: { color: '#e1e0d9' },
      angleLines: { color: '#e1e0d9' }
    }
  }
};

/// NovaERP has no DashboardController for aggregate org stats — this page composes KPI tiles
/// client-side from the foundation endpoints (org/branches/departments/users) and reuses the
/// Dashboard Builder feature's own widget-data endpoint (api/my-dashboards/widget-data/{type}),
/// which is deliberately permission-free and restricted to non-sensitive operational data
/// (Sales, Service Desk, POS, Projects) — see WidgetDataService's own doc comment.
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, CardModule, ChartModule, TooltipModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  loading = true;
  organization: OrganizationDto | null = null;

  statTiles: StatTile[] = [];
  posTotal: number | null = null;
  posLoading = true;

  userActivityLoading = true;
  userActivitySegments: ProportionSegment[] = [];

  // Sourced from the user's own Dashboard Builder dashboard (their default one, or their first
  // if none is marked default) — so this page shows exactly the widgets they've chosen to add
  // there, instead of a fixed hardcoded set. See ngOnInit/loadBuilderWidgets.
  charts: ChartPanel[] = [];
  chartsLoading = true;
  builderDashboardName: string | null = null;
  hasBuilderDashboard = false;

  constructor(
    private orgApi: OrganizationApiService,
    private branchApi: BranchApiService,
    private departmentApi: DepartmentApiService,
    private userApi: UserApiService,
    private widgetApi: DashboardBuilderApiService
  ) {}

  ngOnInit(): void {
    let branchCount: number | null = null;
    let departmentCount: number | null = null;

    this.orgApi.getProfile().pipe(catchError(() => of(null))).subscribe(org => {
      this.organization = org;
    });

    this.branchApi.getAll().pipe(catchError(() => of(null))).subscribe(branches => {
      branchCount = branches ? branches.length : null;
      this.rebuildStatTiles(branchCount, departmentCount, null, null);
    });

    this.departmentApi.getAll().pipe(catchError(() => of(null))).subscribe(departments => {
      departmentCount = departments ? departments.length : null;
      this.rebuildStatTiles(branchCount, departmentCount, null, null);
    });

    this.userApi.getAll().pipe(catchError(() => of(null))).subscribe(users => {
      const userCount = users ? users.length : null;
      const activeUserCount = users ? users.filter(u => u.isActive).length : null;
      this.rebuildStatTiles(branchCount, departmentCount, userCount, activeUserCount);
      this.loading = false;

      if (users && users.length) {
        const active = users.filter(u => u.isActive).length;
        const inactive = users.length - active;
        this.userActivitySegments = [
          { label: 'Active', count: active, percent: Math.round((active / users.length) * 100), color: '#0ca30c' },
          { label: 'Inactive', count: inactive, percent: Math.round((inactive / users.length) * 100), color: '#c3c2b7' }
        ];
      }
      this.userActivityLoading = false;
    });

    this.widgetApi.getAll().pipe(catchError(() => of(null))).subscribe(dashboards => {
      const chosen = dashboards?.find(d => d.isDefault) ?? dashboards?.[0] ?? null;
      this.hasBuilderDashboard = !!chosen;
      this.builderDashboardName = chosen?.name ?? null;
      this.chartsLoading = false;

      const widgets = [...(chosen?.widgets ?? [])].sort((a, b) => a.displayOrder - b.displayOrder);

      // PosSalesTotal is a single figure, not a breakdown — shown as the hero stat tile above
      // instead of a chart panel, whether or not the user has it on their Builder dashboard.
      const posWidget = widgets.find(w => w.widgetType === 'PosSalesTotal');
      this.widgetApi.getWidgetData('PosSalesTotal').pipe(catchError(() => of(null))).subscribe(data => {
        this.posTotal = data?.total ?? (data?.values?.[0] ?? null);
        this.posLoading = false;
      });
      if (!posWidget) this.posLoading = false;

      this.charts = widgets
        .filter(w => w.widgetType !== 'PosSalesTotal')
        .map(w => {
          const presentation = WIDGET_PRESENTATION[w.widgetType] ?? DEFAULT_PRESENTATION;
          return {
            key: w.widgetType,
            title: w.title,
            icon: presentation.icon,
            type: presentation.type,
            options: presentation.type === 'bar' ? BAR_OPTIONS : presentation.type === 'doughnut' ? DOUGHNUT_OPTIONS : POLAR_AREA_OPTIONS,
            fixedHeight: presentation.type === 'bar' ? undefined : (presentation.type === 'doughnut' ? '240px' : '260px'),
            data: null,
            loading: true,
            empty: false
          } as ChartPanel;
        });

      for (const panel of this.charts) {
        this.widgetApi.getWidgetData(panel.key).pipe(catchError(() => of(null))).subscribe(data => {
          panel.loading = false;
          panel.empty = !data || !data.labels.length;
          panel.data = data ? this.toChartData(data, panel.type) : null;
        });
      }
    });
  }

  private rebuildStatTiles(branches: number | null, departments: number | null, users: number | null, activeUsers: number | null) {
    this.statTiles = [
      { label: 'Branches', value: branches != null ? String(branches) : '—', icon: 'pi-building', accent: '#2563eb' },
      { label: 'Departments', value: departments != null ? String(departments) : '—', icon: 'pi-sitemap', accent: '#7c3aed' },
      { label: 'Total Users', value: users != null ? String(users) : '—', icon: 'pi-users', accent: '#1baf7a' },
      { label: 'Active Users', value: activeUsers != null ? String(activeUsers) : '—', icon: 'pi-verified', accent: '#0ca30c' }
    ];
  }

  /// Horizontal bar charts read better with a height proportional to category count rather
  /// than a fixed box that squashes many bars or leaves a lone bar floating in empty space.
  chartHeight(categoryCount: number): string {
    return `${Math.max(120, categoryCount * 44 + 40)}px`;
  }

  private toChartData(data: WidgetDataDto, type: ChartKind) {
    const backgroundColor = data.labels.map((_, i) => CHART_PALETTE[i % CHART_PALETTE.length]);
    if (type === 'bar') {
      return { labels: data.labels, datasets: [{ data: data.values, backgroundColor, borderRadius: 4, maxBarThickness: 22 }] };
    }
    // Doughnut/Polar Area read best with a thin surface-colored ring between slices so
    // adjacent same-family hues never visually bleed together.
    return { labels: data.labels, datasets: [{ data: data.values, backgroundColor, borderWidth: 2, borderColor: '#fff' }] };
  }
}
