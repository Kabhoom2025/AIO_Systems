import { DecimalPipe } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import Chart from 'chart.js/auto';

import { DashboardApiService } from '../../core/services/dashboard-api.service';
import { DashboardSummary, DashboardTrends } from '../../core/models/dashboard.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements AfterViewInit, OnDestroy {
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly charts: Chart[] = [];

  @ViewChild('scansOverTimeCanvas') scansOverTimeCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('riskDistributionCanvas') riskDistributionCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('topBrandsCanvas') topBrandsCanvas?: ElementRef<HTMLCanvasElement>;

  readonly summary = signal<DashboardSummary | null>(null);
  readonly trends = signal<DashboardTrends | null>(null);
  readonly loadError = signal<string | null>(null);

  ngAfterViewInit(): void {
    this.dashboardApi.getSummary().subscribe({
      next: (summary) => this.summary.set(summary),
      error: () => this.loadError.set('Could not load dashboard summary — is LinkShield.API running?')
    });

    this.dashboardApi.getTrends(14).subscribe({
      next: (trends) => {
        this.trends.set(trends);
        this.renderCharts(trends);
      },
      error: () => this.loadError.set('Could not load dashboard trends — is LinkShield.API running?')
    });
  }

  ngOnDestroy(): void {
    this.charts.forEach((chart) => chart.destroy());
  }

  private renderCharts(trends: DashboardTrends): void {
    const gridColor = 'rgba(143, 161, 184, 0.15)';
    const textColor = '#8fa1b8';
    Chart.defaults.color = textColor;
    Chart.defaults.font.family = "'Inter', sans-serif";

    if (this.scansOverTimeCanvas) {
      this.charts.push(
        new Chart(this.scansOverTimeCanvas.nativeElement, {
          type: 'bar',
          data: {
            labels: trends.scansOverTime.map((p) => p.date.slice(5)),
            datasets: [
              { label: 'Safe', data: trends.scansOverTime.map((p) => p.safeCount), backgroundColor: '#3fb950', stack: 's' },
              { label: 'Suspicious', data: trends.scansOverTime.map((p) => p.suspiciousCount), backgroundColor: '#d29922', stack: 's' },
              { label: 'Malicious', data: trends.scansOverTime.map((p) => p.maliciousCount), backgroundColor: '#f85149', stack: 's' }
            ]
          },
          options: {
            responsive: true,
            plugins: { legend: { position: 'bottom' } },
            scales: {
              x: { stacked: true, grid: { color: gridColor } },
              y: { stacked: true, beginAtZero: true, grid: { color: gridColor }, ticks: { precision: 0 } }
            }
          }
        })
      );
    }

    if (this.riskDistributionCanvas) {
      const order = ['Low', 'Moderate', 'High', 'Critical'];
      const colors: Record<string, string> = { Low: '#56d364', Moderate: '#d29922', High: '#db6d28', Critical: '#f85149' };
      const byLevel = new Map(trends.riskDistribution.map((p) => [p.riskLevel, p.count]));

      this.charts.push(
        new Chart(this.riskDistributionCanvas.nativeElement, {
          type: 'bar',
          data: {
            labels: order,
            datasets: [{ data: order.map((level) => byLevel.get(level) ?? 0), backgroundColor: order.map((level) => colors[level]) }]
          },
          options: {
            indexAxis: 'y',
            responsive: true,
            plugins: { legend: { display: false } },
            scales: {
              x: { beginAtZero: true, grid: { color: gridColor }, ticks: { precision: 0 } },
              y: { grid: { display: false } }
            }
          }
        })
      );
    }

    if (this.topBrandsCanvas) {
      this.charts.push(
        new Chart(this.topBrandsCanvas.nativeElement, {
          type: 'bar',
          data: {
            labels: trends.topTargetedBrands.map((b) => b.brandName),
            datasets: [{ data: trends.topTargetedBrands.map((b) => b.count), backgroundColor: '#2f81f7' }]
          },
          options: {
            indexAxis: 'y',
            responsive: true,
            plugins: { legend: { display: false } },
            scales: {
              x: { beginAtZero: true, grid: { color: gridColor }, ticks: { precision: 0 } },
              y: { grid: { display: false } }
            }
          }
        })
      );
    }
  }
}
