import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { ToastModule } from 'primeng/toast';
import { environment } from '../../environments/environment';
import { HasPermissionDirective } from '../core/permission.directive';
import { AuthService } from '../core/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  permission?: string;
}

interface StoredUser {
  name: string;
  email: string;
  role: string;
  organizationName: string;
}

@Component({
  selector: 'app-hrms-shell',
  standalone: true,
  imports: [
    CommonModule, RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule, MatIconModule, MatButtonModule, MatMenuModule,
    ToastModule, HasPermissionDirective
  ],
  templateUrl: './hrms-shell.component.html',
  styleUrl: './hrms-shell.component.scss'
})
export class HrmsShellComponent {
  sidenavOpen = signal(true);

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: 'dashboard' },
    { label: 'Employees', icon: 'badge', route: 'employees', permission: 'employees.view' },
    { label: 'Attendance', icon: 'schedule', route: 'attendance' },
    { label: 'Leaves', icon: 'event_busy', route: 'leaves' },
    { label: 'Payroll', icon: 'payments', route: 'payroll', permission: 'payroll.view' },
    { label: 'Recruitment', icon: 'work', route: 'recruitment', permission: 'recruitment.view' },
    { label: 'Performance', icon: 'insights', route: 'performance', permission: 'performance.view' },
    { label: 'Assets', icon: 'devices', route: 'assets', permission: 'assets.view' },
    { label: 'Expenses', icon: 'receipt_long', route: 'expenses' },
    { label: 'Help Desk', icon: 'support_agent', route: 'helpdesk' },
    { label: 'Reports', icon: 'bar_chart', route: 'reports', permission: 'reports.view' },
    { label: 'Organization', icon: 'apartment', route: 'organization', permission: 'organization.view' },
    { label: 'Workflows', icon: 'account_tree', route: 'workflows', permission: 'workflows.view' },
    { label: 'Users', icon: 'group', route: 'users', permission: 'users.view' },
    { label: 'Roles & Permissions', icon: 'admin_panel_settings', route: 'roles', permission: 'roles.view' },
    { label: 'Audit Log', icon: 'history', route: 'audit-logs', permission: 'audit-logs.view' }
  ];

  user: StoredUser | null = this.readStoredUser();

  constructor(private router: Router, private auth: AuthService) {}

  toggleSidenav() {
    this.sidenavOpen.set(!this.sidenavOpen());
  }

  logout() {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  private readStoredUser(): StoredUser | null {
    const raw = localStorage.getItem(environment.userKey);
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return null; }
  }
}
