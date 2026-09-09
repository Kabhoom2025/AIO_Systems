import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken, getAuthHeaders } from '../../core/auth.helper';

interface AuditLogEntry {
  id: number;
  timestamp: string;
  userName: string;
  action: string;
  entityType: string;
  entityId: number | null;
}

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './audit-logs.component.html',
  styleUrl: './audit-logs.component.scss'
})
export class AuditLogsComponent implements OnInit {
  orgId = getOrgIdFromToken();
  logs = signal<AuditLogEntry[]>([]);
  loading = signal(false);

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loading.set(true);
    this.http.get<AuditLogEntry[]>(`${environment.apiUrl}/audit-logs/org/${this.orgId}`, {
      headers: getAuthHeaders()
    }).subscribe({
      next: data => { this.logs.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  actionColor(action: string) {
    return ({ Create: '#43a047', Update: '#1e88e5', Delete: '#e53935' } as any)[action] ?? '#757575';
  }
}
