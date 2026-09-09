import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { ChartModule } from 'primeng/chart';
import { NotificationService } from '../../core/notification.service';
import {
  DashboardBuilderApiService, DashboardDto, DashboardWidgetDto, WidgetDataDto
} from '../../core/dashboard-builder-api.service';

const WIDGET_TYPE_LABELS: Record<string, string> = {
  SalesOrderStatusSummary: 'Sales Orders by Status',
  ServiceTicketStatusSummary: 'Tickets by Status',
  PosSalesTotal: 'POS Sales Total',
  ProjectTaskStatusSummary: 'Project Tasks by Status',
  PurchaseOrderStatusSummary: 'Purchase Orders by Status',
  ShipmentStatusSummary: 'Shipments by Status',
  RfqStatusSummary: 'RFQs by Status'
};

const ALL_WIDGET_TYPES = Object.keys(WIDGET_TYPE_LABELS);
const SIZE_OPTIONS = ['Small', 'Medium', 'Large'];

@Component({
  selector: 'app-dashboard-builder',
  standalone: true,
  imports: [
    CommonModule, FormsModule, DragDropModule, ButtonModule, DropdownModule,
    DialogModule, InputTextModule, ToastModule, ConfirmDialogModule, ChartModule
  ],
  providers: [ConfirmationService],
  templateUrl: './dashboard-builder.component.html',
  styleUrl: './dashboard-builder.component.scss'
})
export class DashboardBuilderComponent implements OnInit {
  loading = false;
  dashboards: DashboardDto[] = [];
  current: DashboardDto | null = null;

  sizeOptions = SIZE_OPTIONS;
  widgetTypeLabel(type: string): string {
    return WIDGET_TYPE_LABELS[type] ?? type;
  }

  get availableWidgetTypes(): string[] {
    const used = new Set(this.current?.widgets.map(w => w.widgetType) ?? []);
    return ALL_WIDGET_TYPES.filter(t => !used.has(t));
  }

  chartData: Record<number, any> = {};
  chartOptions = { responsive: true, plugins: { legend: { display: false } } };

  showNewDialog = false;
  newDashboardName = '';
  saving = false;

  constructor(
    private api: DashboardBuilderApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => {
        this.dashboards = rows;
        this.loading = false;
        if (rows.length) {
          this.selectDashboard(rows[0].id);
        }
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load dashboards.');
      }
    });
  }

  selectDashboard(id: number) {
    this.api.getById(id).subscribe({
      next: dashboard => {
        this.current = dashboard;
        this.loadWidgetData();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to load dashboard.')
    });
  }

  private loadWidgetData() {
    this.chartData = {};
    for (const widget of this.current?.widgets ?? []) {
      this.api.getWidgetData(widget.widgetType).subscribe({
        next: data => (this.chartData[widget.id] = this.toChartData(data)),
        error: () => (this.chartData[widget.id] = null)
      });
    }
  }

  private toChartData(data: WidgetDataDto) {
    return {
      labels: data.labels,
      datasets: [{ data: data.values, backgroundColor: '#6366f1' }]
    };
  }

  openNewDashboard() {
    this.newDashboardName = '';
    this.showNewDialog = true;
  }

  createDashboard() {
    if (!this.newDashboardName) {
      this.notify.warn('Name is required.');
      return;
    }
    this.saving = true;
    this.api.create({ name: this.newDashboardName, isDefault: false }).subscribe({
      next: created => {
        this.saving = false;
        this.showNewDialog = false;
        this.notify.success('Dashboard created.');
        this.dashboards = [...this.dashboards, created];
        this.selectDashboard(created.id);
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to create dashboard.');
      }
    });
  }

  deleteDashboard(d: DashboardDto) {
    this.confirm.confirm({
      message: `Delete dashboard "${d.name}"? This will remove all its widgets too.`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(d.id).subscribe({
          next: () => {
            this.dashboards = this.dashboards.filter(x => x.id !== d.id);
            this.notify.success('Dashboard deleted.');
            if (this.current?.id === d.id) {
              this.current = null;
              this.chartData = {};
              if (this.dashboards.length) this.selectDashboard(this.dashboards[0].id);
            }
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete dashboard.')
        });
      }
    });
  }

  addWidget(widgetType: string) {
    if (!this.current) return;
    this.api.addWidget(this.current.id, {
      widgetType, title: this.widgetTypeLabel(widgetType), sizeOption: 'Medium'
    }).subscribe({
      next: updated => {
        this.current = updated;
        this.loadWidgetData();
        this.notify.success('Widget added.');
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to add widget.')
    });
  }

  removeWidget(widget: DashboardWidgetDto) {
    if (!this.current) return;
    this.confirm.confirm({
      message: `Remove widget "${widget.title}"?`,
      header: 'Confirm Remove',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.removeWidget(this.current!.id, widget.id).subscribe({
          next: updated => {
            this.current = updated;
            this.loadWidgetData();
            this.notify.success('Widget removed.');
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to remove widget.')
        });
      }
    });
  }

  onDrop(event: CdkDragDrop<DashboardWidgetDto[]>) {
    if (!this.current) return;
    moveItemInArray(this.current.widgets, event.previousIndex, event.currentIndex);
    this.persistLayout();
  }

  onSizeChange(_widget: DashboardWidgetDto) {
    this.persistLayout();
  }

  private persistLayout() {
    if (!this.current) return;
    const widgets = this.current.widgets.map((w, index) => ({
      widgetId: w.id,
      displayOrder: index + 1,
      sizeOption: w.sizeOption
    }));
    this.api.reorderWidgets(this.current.id, { widgets }).subscribe({
      next: updated => (this.current = updated),
      error: err => this.notify.error(err.error?.message ?? 'Failed to save layout.')
    });
  }

  sizeColSpan(size: string): string {
    if (size === 'Small') return 'span 1';
    if (size === 'Large') return 'span 3';
    return 'span 2';
  }
}
