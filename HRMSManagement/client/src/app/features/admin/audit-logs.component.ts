import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { PaginatorModule } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { AdminApiService, AuditLogDto, PagedResult } from '../../core/admin-api.service';
import { NotificationService } from '../../core/notification.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    TableModule, ButtonModule, InputTextModule,
    TagModule, TooltipModule, PaginatorModule,
    ProgressSpinnerModule
  ],
  templateUrl: './audit-logs.component.html',
  styleUrl: './audit-logs.component.scss'
})
export class AuditLogsComponent implements OnInit {

  logs: AuditLogDto[] = [];
  totalRecords = 0;
  loading = false;

  page = 1;
  pageSize = 50;
  searchTerm = '';

  private searchDebounce: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private api: AdminApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.loading = true;
    this.api.getAuditLogsPaged(this.page, this.pageSize, this.searchTerm || null).subscribe({
      next: (result: PagedResult<AuditLogDto>) => {
        this.logs = result.items;
        this.totalRecords = result.totalCount;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load audit logs.');
      }
    });
  }

  onSearchChange(): void {
    if (this.searchDebounce) clearTimeout(this.searchDebounce);
    this.searchDebounce = setTimeout(() => {
      this.page = 1;
      this.loadLogs();
    }, 350);
  }

  onPageChange(event: { page?: number; rows?: number }): void {
    this.page = (event.page ?? 0) + 1;
    this.pageSize = event.rows ?? this.pageSize;
    this.loadLogs();
  }

  refresh(): void {
    this.page = 1;
    this.searchTerm = '';
    this.loadLogs();
  }

  statusSeverity(code: number): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    if (code >= 200 && code < 300) return 'success';
    if (code >= 300 && code < 400) return 'info';
    if (code >= 400 && code < 500) return 'warn';
    if (code >= 500)               return 'danger';
    return 'secondary';
  }

  methodSeverity(method: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (method?.toUpperCase()) {
      case 'GET':    return 'info';
      case 'POST':   return 'success';
      case 'PUT':    return 'warn';
      case 'PATCH':  return 'warn';
      case 'DELETE': return 'danger';
      default:       return 'secondary';
    }
  }
}
