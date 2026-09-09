import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { ToastModule } from 'primeng/toast';
import { environment } from '../../environments/environment';
import { HasPermissionDirective } from '../core/permission.directive';
import { PermissionsService } from '../core/permissions.service';
import { ThemeService } from '../core/theme.service';
import { BreadcrumbComponent } from '../shared/breadcrumb/breadcrumb.component';
import { getOrgIdFromToken } from '../core/auth.helper';
import { AppNotification, NotificationsApiService } from '../core/notifications-api.service';
import { NotificationsHubService } from '../core/notifications-hub.service';
import { NotificationService } from '../core/notification.service';

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
  selector: 'app-pharmacy-shell',
  standalone: true,
  imports: [
    FormsModule, RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule, MatIconModule, MatButtonModule, MatMenuModule, MatBadgeModule,
    ToastModule, HasPermissionDirective, BreadcrumbComponent
  ],
  templateUrl: './pharmacy-shell.component.html',
  styleUrl: './pharmacy-shell.component.scss'
})
export class PharmacyShellComponent implements OnInit, OnDestroy {
  sidenavOpen = signal(true);
  searchTerm = '';
  docsUrl = environment.docsUrl;

  orgId = getOrgIdFromToken();
  unreadCount = signal(0);
  notifications = signal<AppNotification[]>([]);
  private pollHandle?: ReturnType<typeof setInterval>;

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: 'dashboard' },
    { label: 'Point of Sale', icon: 'point_of_sale', route: 'pos', permission: 'sales.create' },
    { label: 'Sales History', icon: 'receipt_long', route: 'sales-history', permission: 'sales.view' },
    { label: 'Online Orders', icon: 'shopping_bag', route: 'online-orders', permission: 'sales.view' },
    { label: 'Deliveries', icon: 'moped', route: 'deliveries', permission: 'deliveries.view' },
    { label: 'Medicines', icon: 'medication', route: 'medicines' },
    { label: 'Prescriptions', icon: 'description', route: 'prescriptions' },
    { label: 'Expiry Alerts', icon: 'warning', route: 'expiry-alerts' },
    { label: 'Suppliers', icon: 'local_shipping', route: 'suppliers' },
    { label: 'Purchase Orders', icon: 'shopping_cart', route: 'purchase-orders' },
    { label: 'Goods Receipts', icon: 'inventory_2', route: 'goods-receipts' },
    { label: 'Stock Adjustments', icon: 'tune', route: 'stock-adjustments' },
    { label: 'Doctors', icon: 'medical_services', route: 'doctors', permission: 'doctors.view' },
    { label: 'Appointments', icon: 'event', route: 'appointments', permission: 'appointments.view' },
    { label: 'Patients', icon: 'personal_injury', route: 'patients', permission: 'patients.view' },
    { label: 'Customers', icon: 'people', route: 'customers', permission: 'customers.view' },
    { label: 'Expenses', icon: 'receipt', route: 'expenses', permission: 'expenses.view' },
    { label: 'Reports', icon: 'bar_chart', route: 'reports', permission: 'reports.view' },
    { label: 'AI Insights', icon: 'auto_awesome', route: 'ai-insights', permission: 'reports.view' },
    { label: 'Settings', icon: 'settings', route: 'settings' },
    { label: 'Audit Log', icon: 'history', route: 'audit-logs', permission: 'audit-logs.view' },
    { label: 'Branches', icon: 'apartment', route: 'branches', permission: 'branches.view' },
    { label: 'Roles & Permissions', icon: 'admin_panel_settings', route: 'roles', permission: 'roles.view' }
  ];

  user: StoredUser | null = this.readStoredUser();

  constructor(
    private router: Router,
    private permissions: PermissionsService,
    public theme: ThemeService,
    private notificationsApi: NotificationsApiService,
    private notificationsHub: NotificationsHubService,
    private notify: NotificationService
  ) {}

  ngOnInit() {
    this.refreshUnreadCount();
    // Fallback safety net in case the SignalR connection drops and hasn't reconnected —
    // the hub push below is the primary path, this just guards against staleness.
    this.pollHandle = setInterval(() => this.refreshUnreadCount(), 5 * 60 * 1000);

    this.notificationsHub.start();
    this.notificationsHub.onNotificationsUpdated(payload => {
      this.unreadCount.set(payload.unreadCount);
      for (const n of payload.notifications) {
        this.notify.info(n.message, n.title);
      }
    });
  }

  ngOnDestroy() {
    if (this.pollHandle) clearInterval(this.pollHandle);
    this.notificationsHub.stop();
  }

  private refreshUnreadCount() {
    if (!this.orgId) return;
    this.notificationsApi.getUnreadCount(this.orgId).subscribe({
      next: res => this.unreadCount.set(res.count),
      error: () => {}
    });
  }

  onNotificationMenuOpened() {
    if (!this.orgId) return;
    this.notificationsApi.getList(this.orgId).subscribe({
      next: list => this.notifications.set(list),
      error: () => {}
    });
  }

  markNotificationRead(n: AppNotification) {
    if (n.isRead) return;
    this.notificationsApi.markRead(n.id).subscribe(() => {
      n.isRead = true;
      this.notifications.set([...this.notifications()]);
      this.refreshUnreadCount();
      if (n.relatedEntityType === 'Medicine' || n.relatedEntityType === 'MedicineBatch') {
        this.router.navigateByUrl('/medicines');
      }
    });
  }

  markAllNotificationsRead() {
    if (!this.orgId) return;
    this.notificationsApi.markAllRead(this.orgId).subscribe(() => {
      this.notifications.set(this.notifications().map(n => ({ ...n, isRead: true })));
      this.refreshUnreadCount();
    });
  }

  relativeTime(iso: string): string {
    const diffMs = Date.now() - new Date(iso).getTime();
    const mins = Math.floor(diffMs / 60000);
    if (mins < 1) return 'just now';
    if (mins < 60) return `${mins}m ago`;
    const hours = Math.floor(mins / 60);
    if (hours < 24) return `${hours}h ago`;
    return `${Math.floor(hours / 24)}d ago`;
  }

  private readStoredUser(): StoredUser | null {
    const raw = localStorage.getItem(environment.userKey);
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return null; }
  }

  toggleSidenav() {
    this.sidenavOpen.set(!this.sidenavOpen());
  }

  onSearch() {
    // Global search across modules is not implemented yet — placeholder for a future
    // cross-module search endpoint. Intentionally a no-op for now.
  }

  logout() {
    localStorage.removeItem(environment.tokenKey);
    localStorage.removeItem(environment.userKey);
    this.permissions.reload();
    this.router.navigateByUrl('/login');
  }
}
