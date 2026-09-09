import { Component, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatRippleModule } from '@angular/material/core';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { AuthService } from '../../core/authentication/auth.service';
import { PermissionStateService } from '../../core/services/permission-state.service';
import { ThemeService } from '../../core/services/theme.service';
import { APP_CONSTANTS } from '../../core/constants/app.constants';

interface NavItem {
  label: string;
  icon: string;
  route?: string;
  queryParams?: Record<string, string>;
  roles?: string[];
  permission?: string;
  modules?: string[];   // If set, only shown when org has ≥1 of these modules enabled
  children?: NavItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterLinkActive, MatIconModule, MatTooltipModule, MatRippleModule, DragDropModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  private authService = inject(AuthService);
  private permState   = inject(PermissionStateService);
  private router      = inject(Router);
  readonly theme      = inject(ThemeService);

  collapsed      = signal(false);
  expandedGroups = signal<Set<string>>(new Set());

  constructor() {
    effect(() => {
      const size = this.theme.sidebarSize();
      this.collapsed.set(size === 'compact');
    });
  }

  private readonly NAV_ORDER_KEY = 'pos_nav_order';

  private readonly COMMERCE      = ['restaurant', 'pharmacy', 'retail'];
  private readonly ALL_VERTICALS = ['restaurant', 'pharmacy', 'retail', 'hr_management', 'finance'];

  private readonly defaultNavItems: NavItem[] = [
    // ── Core (always visible) ─────────────────────────────────
    { label: 'Dashboard',  icon: 'dashboard',        route: '/',           permission: 'dashboard.view' },

    // ── Restaurant / Commerce ─────────────────────────────────
    { label: 'POS',     icon: 'point_of_sale', route: '/pos',    modules: this.COMMERCE,
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER], permission: 'pos.view' },
    { label: 'Waiter',  icon: 'room_service',  route: '/waiter', modules: ['restaurant'],
      roles: [APP_CONSTANTS.ROLES.WAITER], permission: 'waiter.view' },
    { label: 'Orders',  icon: 'receipt_long',  route: '/orders', modules: this.COMMERCE,
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER, APP_CONSTANTS.ROLES.WAITER], permission: 'orders.view' },
    { label: 'Kitchen', icon: 'restaurant',    route: '/kds',    modules: ['restaurant'],
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER], permission: 'kds.view' },
    { label: 'Tables',  icon: 'table_restaurant', route: '/tables', modules: ['restaurant'],
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER], permission: 'tables.view' },
    { label: 'Categories', icon: 'category', route: '/categories', modules: this.COMMERCE,
      roles: [APP_CONSTANTS.ROLES.ADMIN], permission: 'categories.view' },
    {
      label: 'Product Menu', icon: 'restaurant_menu', roles: [APP_CONSTANTS.ROLES.ADMIN], modules: ['restaurant'],
      children: [
        { label: 'Food Items', icon: 'fastfood',  route: '/food-items', roles: [APP_CONSTANTS.ROLES.ADMIN], permission: 'food_items.view' },
        { label: 'Add-Ons',   icon: 'add_circle', route: '/addons',     roles: [APP_CONSTANTS.ROLES.ADMIN], permission: 'addons.view' },
      ],
    },

    // ── Inventory / Procurement ───────────────────────────────
    { label: 'Inventory', icon: 'inventory_2', route: '/inventory',
      modules: ['restaurant', 'pharmacy', 'retail', 'finance'],
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.INVENTORY_MANAGER], permission: 'inventory.view' },
    { label: 'Suppliers', icon: 'storefront', route: '/suppliers',
      modules: ['restaurant', 'pharmacy', 'retail', 'finance'],
      roles: [APP_CONSTANTS.ROLES.ADMIN], permission: 'suppliers.view' },

    // ── Finance ────────────────────────────────────────────────
    {
      label: 'Finance', icon: 'account_balance_wallet',
      modules: ['restaurant', 'retail', 'pharmacy', 'finance'],
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER],
      children: [
        { label: 'Ledger', icon: 'menu_book', route: '/ledger',
          roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER], permission: 'ledger.view' },
      ],
    },

    // ── Analytics ──────────────────────────────────────────────
    { label: 'Reports', icon: 'bar_chart', route: '/reports',
      modules: this.ALL_VERTICALS,
      roles: [APP_CONSTANTS.ROLES.ADMIN], permission: 'reports.view' },

    // ── CRM ────────────────────────────────────────────────────
    {
      label: 'CRM', icon: 'loyalty', modules: this.COMMERCE,
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER],
      children: [
        { label: 'Customers', icon: 'people', route: '/customers',
          roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER], permission: 'customers.view' },
      ],
    },

    // ── Delivery ───────────────────────────────────────────────
    {
      label: 'Delivery', icon: 'local_shipping', modules: ['restaurant', 'retail'],
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER],
      children: [
        { label: 'Dashboard',    icon: 'dashboard',    route: '/delivery',             roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER] },
        { label: 'Drivers',      icon: 'people',       route: '/delivery/drivers',     roles: [APP_CONSTANTS.ROLES.ADMIN] },
        { label: 'Charges',      icon: 'price_change', route: '/delivery/charges',     roles: [APP_CONSTANTS.ROLES.ADMIN] },
        { label: 'Integrations', icon: 'share',        route: '/delivery/third-party', roles: [APP_CONSTANTS.ROLES.ADMIN] },
      ],
    },

    // ── Offers ─────────────────────────────────────────────────
    {
      label: 'Offers & Promos', icon: 'local_offer', modules: ['restaurant', 'retail'],
      roles: [APP_CONSTANTS.ROLES.ADMIN],
      children: [
        { label: 'All Promotions', icon: 'sell',                route: '/promotions',                                      roles: [APP_CONSTANTS.ROLES.ADMIN] },
        { label: 'Coupons',        icon: 'confirmation_number', route: '/promotions', queryParams: { type: 'Coupon' },    roles: [APP_CONSTANTS.ROLES.ADMIN] },
        { label: 'Happy Hours',    icon: 'schedule',            route: '/promotions', queryParams: { type: 'HappyHour' }, roles: [APP_CONSTANTS.ROLES.ADMIN] },
        { label: 'Gift Cards',     icon: 'card_giftcard',       route: '/promotions', queryParams: { type: 'GiftCard' },  roles: [APP_CONSTANTS.ROLES.ADMIN] },
      ],
    },

    // ── QR Menu ────────────────────────────────────────────────
    {
      label: 'QR Menu', icon: 'qr_code_2', modules: ['restaurant'],
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER],
      children: [
        { label: 'Generate QR',  icon: 'qr_code_2',       route: '/qr-menu/generate',     roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER] },
        { label: 'Table QR',     icon: 'table_restaurant', route: '/qr-menu/tables',       roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER] },
        { label: 'Scan QR',      icon: 'qr_code_scanner',  route: '/qr-menu/scan',         roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER] },
        { label: 'Digital Menu', icon: 'restaurant_menu',  route: '/qr-menu/digital-menu', roles: [APP_CONSTANTS.ROLES.ADMIN] },
      ],
    },

    // ── Pharmacy ───────────────────────────────────────────────
    {
      label: 'Pharmacy', icon: 'local_pharmacy', modules: ['pharmacy'],
      roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER],
      children: [
        { label: 'Medicines',      icon: 'medication',     route: '/pharmacy/medicines',      modules: ['pharmacy'], roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER] },
        { label: 'Prescriptions',  icon: 'assignment',     route: '/pharmacy/prescriptions',  modules: ['pharmacy'], roles: [APP_CONSTANTS.ROLES.ADMIN, APP_CONSTANTS.ROLES.CASHIER] },
        { label: 'Expiry Alerts',  icon: 'warning_amber',  route: '/pharmacy/expiry-alerts',  modules: ['pharmacy'], roles: [APP_CONSTANTS.ROLES.ADMIN] },
      ],
    },

    // ── HR ─────────────────────────────────────────────────────
    { label: 'Employees', icon: 'badge', route: '/employees',
      modules: ['restaurant', 'hr_management', 'retail'],
      roles: [APP_CONSTANTS.ROLES.ADMIN], permission: 'employees.view' },

    // ── System Admin (core – always visible to admin) ──────────
    {
      label: 'System Admin', icon: 'admin_panel_settings', roles: [APP_CONSTANTS.ROLES.ADMIN],
      children: [
        { label: 'Feature Toggles',      icon: 'toggle_on',      route: '/feature-toggles',      roles: [APP_CONSTANTS.ROLES.ADMIN] },
        { label: 'Integration Settings', icon: 'cable',           route: '/integration-settings', roles: [APP_CONSTANTS.ROLES.ADMIN] },
        { label: 'Users',                icon: 'manage_accounts', route: '/users',                roles: [APP_CONSTANTS.ROLES.ADMIN], permission: 'users.view' },
        { label: 'Access Control',       icon: 'security',        route: '/access-control',       roles: [APP_CONSTANTS.ROLES.ADMIN], permission: 'roles.permissions' },
      ],
    },
    { label: 'Settings', icon: 'settings', route: '/settings', roles: [APP_CONSTANTS.ROLES.ADMIN], permission: 'settings.view' },
  ];

  navItems = signal<NavItem[]>(this.loadOrderedItems());

  get role(): string { return this.authService.userRole(); }

  isVisible(item: NavItem): boolean {
    const isAdmin      = this.role === APP_CONSTANTS.ROLES.ADMIN;
    const isSuperAdmin = this.authService.isSuperAdmin();

    // Module gate — skip for super admin (they have global access)
    if (item.modules?.length && !isSuperAdmin) {
      const orgModules = this.authService.enabledModules();
      // If org has no modules assigned yet, fall back to showing all (legacy orgs)
      if (orgModules.length > 0 && !item.modules.some(m => orgModules.includes(m))) return false;
    }

    // Role-level gate — item not allowed for this role at all
    if (item.roles && !item.roles.includes(this.role)) return false;

    // Admin bypasses permission checks
    if (isAdmin) return true;

    // Permission gate — if permissions are loaded, enforce them
    if (item.permission) return this.permState.hasPermission(item.permission);

    // Group header: show if any visible child exists
    if (item.children) return item.children.some(c => this.isVisible(c));

    return true;
  }

  toggle(): void { this.collapsed.update(v => !v); }

  toggleGroup(label: string): void {
    this.expandedGroups.update(groups => {
      const next = new Set(groups);
      next.has(label) ? next.delete(label) : next.add(label);
      return next;
    });
  }

  isGroupExpanded(label: string): boolean { return this.expandedGroups().has(label); }

  isGroupActive(item: NavItem): boolean {
    if (!item.children) return false;
    return item.children.some(child =>
      child.route && this.router.isActive(child.route, {
        paths: 'exact', queryParams: 'ignored', fragment: 'ignored', matrixParams: 'ignored',
      })
    );
  }

  drop(event: CdkDragDrop<NavItem[]>): void {
    if (event.previousIndex === event.currentIndex) return;

    // CDK indices are over visible items only (invisible items are not in the DOM)
    const visible   = this.navItems().filter(item => this.isVisible(item));
    const movedItem = visible[event.previousIndex];
    const intoItem  = visible[event.currentIndex];

    const all      = [...this.navItems()];
    const fromIdx  = all.indexOf(movedItem);
    const toIdx    = all.indexOf(intoItem);

    moveItemInArray(all, fromIdx, toIdx);
    this.navItems.set(all);
    this.saveOrder(all);
  }

  private saveOrder(items: NavItem[]): void {
    localStorage.setItem(this.NAV_ORDER_KEY, JSON.stringify(items.map(i => i.label)));
  }

  private loadOrderedItems(): NavItem[] {
    try {
      const raw = localStorage.getItem(this.NAV_ORDER_KEY);
      if (!raw) return [...this.defaultNavItems];

      const order: string[] = JSON.parse(raw);
      const map = new Map(this.defaultNavItems.map(i => [i.label, i]));

      const sorted: NavItem[] = [];
      for (const label of order) {
        const item = map.get(label);
        if (item) sorted.push(item);
      }
      // Append any new items added after the saved order was created
      for (const item of this.defaultNavItems) {
        if (!order.includes(item.label)) sorted.push(item);
      }
      return sorted;
    } catch {
      return [...this.defaultNavItems];
    }
  }
}
