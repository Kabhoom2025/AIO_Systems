import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { TagModule } from 'primeng/tag';
import { DashboardApiService, DashboardSummaryDto, MyDashboardDto } from '../../core/dashboard-api.service';
import { NotificationService } from '../../core/notification.service';
import { getEmployeeIdFromToken } from '../../core/auth.helper';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, CardModule, ChartModule, TagModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  summary: DashboardSummaryDto | null = null;
  myDashboard: MyDashboardDto | null = null;
  loading = false;

  departmentChartData: any;
  departmentChartOptions: any;
  attendanceChartData: any;
  attendanceChartOptions: any;

  constructor(
    private dashboardApi: DashboardApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadSummary();
    if (getEmployeeIdFromToken()) {
      this.loadMyDashboard();
    }
  }

  loadSummary() {
    this.loading = true;
    this.dashboardApi.getSummary().subscribe({
      next: data => {
        this.summary = data;
        this.loading = false;
        this.buildCharts(data);
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load dashboard summary.');
      }
    });
  }

  loadMyDashboard() {
    this.dashboardApi.getMyDashboard().subscribe({
      next: data => (this.myDashboard = data),
      error: err => this.notify.error(err.error?.message ?? 'Failed to load your dashboard.')
    });
  }

  private buildCharts(data: DashboardSummaryDto) {
    this.departmentChartData = {
      labels: data.headcountByDepartment.map(d => d.name),
      datasets: [
        {
          data: data.headcountByDepartment.map(d => d.value),
          backgroundColor: ['#42A5F5', '#66BB6A', '#FFA726', '#AB47BC', '#EC407A', '#26C6DA', '#7E57C2']
        }
      ]
    };
    this.departmentChartOptions = {
      plugins: { legend: { position: 'bottom' } }
    };

    this.attendanceChartData = {
      labels: data.attendanceTrend.map(d => d.date),
      datasets: [
        {
          label: 'Present',
          data: data.attendanceTrend.map(d => d.present),
          borderColor: '#42A5F5',
          tension: 0.3,
          fill: false
        },
        {
          label: 'On Leave',
          data: data.attendanceTrend.map(d => d.onLeave),
          borderColor: '#FFA726',
          tension: 0.3,
          fill: false
        }
      ]
    };
    this.attendanceChartOptions = {
      plugins: { legend: { position: 'bottom' } },
      scales: { y: { beginAtZero: true } }
    };
  }
}
