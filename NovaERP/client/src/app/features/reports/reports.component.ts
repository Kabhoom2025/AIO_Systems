import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import {
  ReportsApiService, TrialBalanceDto, StatusSummaryReportDto
} from '../../core/reports-api.service';
import { NotificationService } from '../../core/notification.service';

type ReportKey = 'trial-balance' | 'sales-order-summary' | 'purchase-order-summary';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, ProgressSpinnerModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  loading = false;
  selected: ReportKey = 'trial-balance';

  reports: { key: ReportKey; label: string }[] = [
    { key: 'trial-balance', label: 'Trial Balance' },
    { key: 'sales-order-summary', label: 'Sales Order Summary' },
    { key: 'purchase-order-summary', label: 'Purchase Order Summary' }
  ];

  trialBalance: TrialBalanceDto | null = null;
  salesOrderSummary: StatusSummaryReportDto | null = null;
  purchaseOrderSummary: StatusSummaryReportDto | null = null;

  constructor(
    private api: ReportsApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.select('trial-balance');
  }

  select(key: ReportKey) {
    this.selected = key;
    this.loading = true;

    const done = () => (this.loading = false);
    const fail = (err: any) => {
      this.loading = false;
      this.notify.error(err.error?.message ?? 'Failed to load report.');
    };

    if (key === 'trial-balance' && !this.trialBalance) {
      this.api.getTrialBalance().subscribe({ next: r => { this.trialBalance = r; done(); }, error: fail });
    } else if (key === 'sales-order-summary' && !this.salesOrderSummary) {
      this.api.getSalesOrderSummary().subscribe({ next: r => { this.salesOrderSummary = r; done(); }, error: fail });
    } else if (key === 'purchase-order-summary' && !this.purchaseOrderSummary) {
      this.api.getPurchaseOrderSummary().subscribe({ next: r => { this.purchaseOrderSummary = r; done(); }, error: fail });
    } else {
      done();
    }
  }
}
