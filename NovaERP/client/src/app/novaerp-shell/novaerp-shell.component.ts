import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { trigger, transition, style, animate, query, group } from '@angular/animations';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { ToastModule } from 'primeng/toast';
import { SidebarModule } from 'primeng/sidebar';
import { AuthService } from '../core/auth.service';
import { HasPermissionDirective } from '../core/permission.directive';
import { PermissionsService } from '../core/permissions.service';
import { NotificationHubService } from '../core/notification-hub.service';
import { NotificationApiService } from '../core/notification-api.service';
import { AiAssistantComponent } from '../features/ai-assistant/ai-assistant.component';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  permission?: string;
}

interface NavGroup {
  label: string;
  items: NavItem[];
}

// A single, centralized page-transition animation — every routed page slides in from the side
// on load without any individual feature component needing its own animation code.
const routeFadeSlide = trigger('routeAnimations', [
  transition('* <=> *', [
    query(':enter', [style({ opacity: 0, transform: 'translateX(60px)' })], { optional: true }),
    query(':leave', [style({ position: 'absolute', width: '100%' })], { optional: true }),
    group([
      query(':leave', [animate('250ms ease-out', style({ opacity: 0, transform: 'translateX(-30px)' }))], { optional: true }),
      query(':enter', [animate('500ms 80ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))], { optional: true })
    ])
  ])
]);

@Component({
  selector: 'app-novaerp-shell',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule, MatIconModule, MatButtonModule, MatMenuModule, MatBadgeModule,
    ToastModule, SidebarModule, HasPermissionDirective, AiAssistantComponent
  ],
  templateUrl: './novaerp-shell.component.html',
  styleUrl: './novaerp-shell.component.scss',
  animations: [routeFadeSlide]
})
export class NovaErpShellComponent implements OnInit, OnDestroy {
  sidenavOpen = signal(true);
  unreadCount = signal(0);
  showAiPanel = signal(false);

  // Grouped and ordered to tell one continuous story of how the ERP is actually used:
  // set up the org and its people, then find/sell to customers (CRM/Sales), buy from
  // vendors (Procurement/Purchase), define what you stock (Inventory), store and move it
  // (Warehouse & Shipping), optionally build it (Manufacturing), and finally post the
  // financial side of all of it (Finance). Platform/automation plumbing brackets the flow
  // since every module above depends on it but it isn't itself a step in the business flow.
  navGroups: NavGroup[] = [
    {
      label: 'Overview',
      items: [
        { label: 'Dashboard', icon: 'dashboard', route: 'dashboard' },
        // No permission field — the Employee Portal must be reachable by every logged-in
        // user regardless of role, since hrms.view/payroll.view stay Admin-only.
        { label: 'My Portal', icon: 'person', route: 'my-portal' },
        // Same reasoning as My Portal — crm.view stays Admin/Manager-facing, so this must be
        // permission-free to be reachable by an external customer login.
        { label: 'My Customer Portal', icon: 'storefront', route: 'my-customer-portal' },
        // Same reasoning again — procurement.view/purchase.view stay Admin/Manager-facing, so
        // this must be permission-free to be reachable by an external vendor login.
        { label: 'My Vendor Portal', icon: 'local_shipping', route: 'my-vendor-portal' },
        // A Dashboard belongs to a User, not a Role — no permission field, every authenticated
        // user manages their own.
        { label: 'Dashboard Builder', icon: 'dashboard_customize', route: 'dashboard-builder' },
        // A Conversation belongs to a User too — same reasoning, no permission field.
        { label: 'AI Assistant', icon: 'smart_toy', route: 'ai-assistant' }
      ]
    },
    {
      label: '1. Organization Setup',
      items: [
        { label: 'Organization', icon: 'apartment', route: 'organization', permission: 'organization.view' },
        { label: 'Branches', icon: 'store', route: 'branches', permission: 'branches.view' },
        { label: 'Departments', icon: 'account_tree', route: 'departments', permission: 'departments.view' },
        { label: 'Users', icon: 'group', route: 'users', permission: 'users.view' },
        { label: 'Roles', icon: 'admin_panel_settings', route: 'roles', permission: 'roles.view' }
      ]
    },
    {
      label: '2. CRM — Find Customers',
      items: [
        { label: 'Leads', icon: 'person_search', route: 'leads', permission: 'crm.view' },
        { label: 'Accounts', icon: 'business', route: 'accounts', permission: 'crm.view' },
        { label: 'Contacts', icon: 'contacts', route: 'contacts', permission: 'crm.view' },
        { label: 'Opportunities', icon: 'trending_up', route: 'opportunities', permission: 'crm.view' }
      ]
    },
    {
      label: '3. Sales — Sell to Customers',
      items: [
        { label: 'Sales Orders', icon: 'point_of_sale', route: 'sales-orders', permission: 'sales.view' }
      ]
    },
    {
      label: '4. Procurement & Purchase — Buy from Vendors',
      items: [
        { label: 'Vendors', icon: 'storefront', route: 'vendors', permission: 'procurement.view' },
        { label: 'RFQs', icon: 'request_quote', route: 'rfqs', permission: 'procurement.view' },
        { label: 'Purchase Orders', icon: 'shopping_cart', route: 'purchase-orders', permission: 'purchase.view' }
      ]
    },
    {
      label: '5. Inventory — Define What You Stock',
      items: [
        { label: 'Products', icon: 'inventory_2', route: 'products', permission: 'inventory.view' },
        { label: 'Stock Movements', icon: 'swap_vert', route: 'stock-movements', permission: 'inventory.view' }
      ]
    },
    {
      label: '6. Warehouse & Shipping — Store & Fulfill',
      items: [
        { label: 'Warehouses', icon: 'warehouse', route: 'warehouses', permission: 'warehouse.view' },
        { label: 'Stock Transfers', icon: 'compare_arrows', route: 'stock-transfers', permission: 'warehouse.view' },
        { label: 'Shipments', icon: 'local_shipping', route: 'shipments', permission: 'warehouse.view' }
      ]
    },
    {
      label: '7. Manufacturing — Build What You Sell',
      items: [
        { label: 'Bill of Materials', icon: 'schema', route: 'bill-of-materials', permission: 'manufacturing.view' },
        { label: 'Production Orders', icon: 'precision_manufacturing', route: 'production-orders', permission: 'manufacturing.view' }
      ]
    },
    {
      label: '8. Finance — Post the Money Side',
      items: [
        { label: 'Tax Codes', icon: 'receipt_long', route: 'tax-codes', permission: 'tax.view' },
        { label: 'Ledger Accounts', icon: 'account_balance', route: 'ledger-accounts', permission: 'finance.view' },
        { label: 'Journal Entries', icon: 'receipt_long', route: 'journal-entries', permission: 'finance.view' },
        { label: 'Vendor Bills', icon: 'request_quote', route: 'vendor-bills', permission: 'finance.view' },
        { label: 'Customer Invoices', icon: 'description', route: 'customer-invoices', permission: 'finance.view' },
        { label: 'Bank Reconciliations', icon: 'account_balance_wallet', route: 'bank-reconciliations', permission: 'finance.view' }
      ]
    },
    {
      label: '9. HRMS — Manage People',
      items: [
        { label: 'Employees', icon: 'badge', route: 'employees', permission: 'hrms.view' },
        { label: 'Leave Types', icon: 'event_available', route: 'leave-types', permission: 'hrms.view' },
        { label: 'Leave Requests', icon: 'beach_access', route: 'leave-requests', permission: 'hrms.view' },
        { label: 'Employee Compensation', icon: 'payments', route: 'employee-compensation', permission: 'payroll.view' },
        { label: 'Pay Runs', icon: 'request_quote', route: 'pay-runs', permission: 'payroll.view' }
      ]
    },
    {
      label: '10. Projects — Deliver Work',
      items: [
        { label: 'Projects', icon: 'work', route: 'projects', permission: 'projects.view' },
        { label: 'Project Tasks', icon: 'checklist', route: 'project-tasks', permission: 'projects.view' }
      ]
    },
    {
      label: '11. Assets — Track Equipment',
      items: [
        { label: 'Asset Categories', icon: 'category', route: 'asset-categories', permission: 'assets.view' },
        { label: 'Assets', icon: 'inventory', route: 'assets', permission: 'assets.view' }
      ]
    },
    {
      label: '12. Service Desk — Support Requests',
      items: [
        { label: 'Ticket Categories', icon: 'label', route: 'ticket-categories', permission: 'service-desk.view' },
        { label: 'Tickets', icon: 'confirmation_number', route: 'tickets', permission: 'service-desk.view' }
      ]
    },
    {
      label: '13. POS — Sell at the Register',
      items: [
        { label: 'POS Sales', icon: 'point_of_sale', route: 'pos-sales', permission: 'pos.view' }
      ]
    },
    {
      label: '14. Retail — Manage Stores',
      items: [
        { label: 'Stores', icon: 'storefront', route: 'stores', permission: 'retail.view' }
      ]
    },
    {
      label: '15. Logistics — Load & Dispatch Vehicles',
      items: [
        { label: 'Vehicles', icon: 'local_shipping', route: 'vehicles', permission: 'logistics.view' },
        { label: 'Delivery', icon: 'departure_board', route: 'delivery', permission: 'logistics.view' }
      ]
    },
    {
      label: '15a. Carrier Connectors — Carrier/TMS Rate-Shop Integrations',
      items: [
        { label: 'Carrier Connectors', icon: 'cable', route: 'shipping-connectors', permission: 'carrier-connector.view' }
      ]
    },
    {
      label: '16. Reports — View Aggregate Data',
      items: [
        { label: 'Reports', icon: 'summarize', route: 'reports', permission: 'reports.view' }
      ]
    },
    {
      label: 'Platform & Automation',
      items: [
        { label: 'Audit Logs', icon: 'history', route: 'audit-logs', permission: 'audit-logs.view' },
        { label: 'Notifications', icon: 'notifications', route: 'notifications', permission: 'notifications.view' },
        { label: 'Settings', icon: 'settings', route: 'settings', permission: 'settings.view' },
        { label: 'Feature Toggles', icon: 'toggle_on', route: 'feature-toggles', permission: 'settings.view' },
        { label: 'Currencies', icon: 'payments', route: 'currencies', permission: 'settings.view' },
        { label: 'Exchange Rates', icon: 'currency_exchange', route: 'exchange-rates', permission: 'settings.view' },
        { label: 'Languages', icon: 'translate', route: 'languages', permission: 'settings.view' },
        { label: 'Documents', icon: 'description', route: 'documents', permission: 'documents.view' },
        { label: 'Workflow Definitions', icon: 'schema', route: 'workflow-definitions', permission: 'workflows.view' },
        { label: 'My Approvals', icon: 'fact_check', route: 'workflow-instances', permission: 'workflows.view' },
        { label: 'Automation Rules', icon: 'bolt', route: 'automation-rules', permission: 'automation.view' },
        { label: 'Scheduled Jobs', icon: 'schedule', route: 'scheduled-jobs', permission: 'scheduler.view' },
        { label: 'Notification Channels', icon: 'forward_to_inbox', route: 'notification-channel-settings', permission: 'settings.view' },
        { label: 'AI Assistant Settings', icon: 'smart_toy', route: 'ai-assistant-settings', permission: 'settings.view' }
      ]
    }
  ];

  get navItems(): NavItem[] {
    return this.navGroups.flatMap(g => g.items);
  }

  navSearchTerm = '';

  // A group's own label has no permission field — without this pass a group with zero
  // permitted items (e.g. Reports for the Employee role) would still render its empty header,
  // the same bug *appHasPermission fixed for individual items but never applied to groups.
  private get permittedNavGroups(): NavGroup[] {
    return this.navGroups
      .map(group => ({
        ...group,
        items: group.items.filter(item => !item.permission || this.permissions.has(item.permission))
      }))
      .filter(group => group.items.length > 0);
  }

  get filteredNavGroups(): NavGroup[] {
    const query = this.navSearchTerm.trim().toLowerCase();
    if (!query) return this.permittedNavGroups;

    return this.permittedNavGroups
      .map(group => ({
        ...group,
        items: group.items.filter(item => item.label.toLowerCase().includes(query))
      }))
      .filter(group => group.items.length > 0);
  }

  clearNavSearch() {
    this.navSearchTerm = '';
  }

  private readonly quickAccessRoutes = ['workflow-instances', 'workflow-definitions'];

  get quickAccessItems(): NavItem[] {
    return this.navItems.filter(item => this.quickAccessRoutes.includes(item.route));
  }

  user: ReturnType<AuthService['getStoredUser']>;

  constructor(
    private router: Router,
    private auth: AuthService,
    private permissions: PermissionsService,
    private notificationHub: NotificationHubService,
    private notificationApi: NotificationApiService
  ) {
    this.user = this.auth.getStoredUser();
  }

  ngOnInit() {
    this.notificationHub.connect();
    this.notificationHub.notification$.subscribe(() => this.unreadCount.update(v => v + 1));
    this.notificationApi.getUnreadCount().subscribe({
      next: res => this.unreadCount.set(res.count),
      error: () => {}
    });
  }

  ngOnDestroy() {
    this.notificationHub.disconnect();
  }

  toggleSidenav() {
    this.sidenavOpen.set(!this.sidenavOpen());
  }

  toggleAiPanel() {
    this.showAiPanel.set(!this.showAiPanel());
  }

  logout() {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  prepareRouteAnimation(outlet: RouterOutlet): string {
    return outlet?.isActivated ? outlet.activatedRoute.snapshot.url.join('/') : '';
  }
}
