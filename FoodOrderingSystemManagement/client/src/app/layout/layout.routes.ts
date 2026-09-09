import { Routes } from '@angular/router';
import { loadRemoteModule } from '@angular-architects/native-federation';
import { LayoutComponent } from './layout.component';
import { DashboardComponent } from '../features/dashboard/dashboard.component';
import { CategoriesComponent } from '../features/categories/categories.component';
import { FoodItemsComponent } from '../features/food-items/food-items.component';
import { PosComponent } from '../features/orders/pos/pos.component';
import { OrdersListComponent } from '../features/orders/orders-list/orders-list.component';
import { SettingsComponent } from '../features/settings/settings.component';
import { TablesComponent } from '../features/tables/tables.component';
import { WaiterComponent } from '../features/waiter/waiter.component';
import { UsersComponent } from '../features/users/users.component';
import { AddOnsComponent } from '../features/addons/addons.component';
import { InventoryComponent } from '../features/inventory/inventory.component';
import { LedgerComponent } from '../features/ledger/ledger.component';
import { CustomersComponent } from '../features/customers/customers.component';
import { roleGuard, dashboardGuard } from '../core/guards/role.guard';
import { moduleGuard } from '../core/guards/module.guard';
import { AccessControlComponent } from '../features/access-control/access-control.component';
import { EmployeesComponent } from '../features/employees/employees.component';

const ADMIN                = ['Admin'];
const ADMIN_CASHIER        = ['Admin', 'Cashier'];
const ADMIN_CASHIER_WAITER = ['Admin', 'Cashier', 'Waiter'];

const COMMERCE      = ['restaurant', 'pharmacy', 'retail'];
const ALL_VERTICALS = ['restaurant', 'pharmacy', 'retail', 'hr_management', 'finance'];

export const LAYOUT_ROUTES: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      // ── Core routes (no module gate) ────────────────────────
      { path: '',           component: DashboardComponent,  canActivate: [dashboardGuard] },
      { path: 'users',      component: UsersComponent,      canActivate: [roleGuard(ADMIN)] },
      { path: 'access-control', component: AccessControlComponent, canActivate: [roleGuard(ADMIN)] },
      { path: 'settings',   component: SettingsComponent,   canActivate: [roleGuard(ADMIN)] },

      // ── Commerce module routes ───────────────────────────────
      { path: 'pos',        component: PosComponent,        canActivate: [roleGuard(ADMIN_CASHIER),        moduleGuard(COMMERCE)] },
      { path: 'orders',     component: OrdersListComponent, canActivate: [roleGuard(ADMIN_CASHIER_WAITER), moduleGuard(COMMERCE)] },
      { path: 'categories', component: CategoriesComponent, canActivate: [roleGuard(ADMIN),                moduleGuard(COMMERCE)] },

      // ── Restaurant-only routes ───────────────────────────────
      { path: 'waiter',     component: WaiterComponent,     canActivate: [roleGuard(ADMIN_CASHIER_WAITER), moduleGuard(['restaurant'])] },
      { path: 'tables',     component: TablesComponent,     canActivate: [roleGuard(ADMIN_CASHIER),        moduleGuard(['restaurant'])] },
      { path: 'food-items', component: FoodItemsComponent,  canActivate: [roleGuard(ADMIN),                moduleGuard(['restaurant'])] },
      { path: 'addons',     component: AddOnsComponent,     canActivate: [roleGuard(ADMIN),                moduleGuard(['restaurant'])] },

      // ── Inventory / Procurement ──────────────────────────────
      { path: 'inventory',  component: InventoryComponent,  canActivate: [roleGuard([...ADMIN, 'InventoryManager']), moduleGuard(['restaurant', 'pharmacy', 'retail', 'finance'])] },
      { path: 'suppliers',  loadComponent: () => import('../features/suppliers/suppliers.component').then(m => m.SuppliersComponent),
        canActivate: [roleGuard(ADMIN), moduleGuard(['restaurant', 'pharmacy', 'retail', 'finance'])] },

      // ── Finance ──────────────────────────────────────────────
      { path: 'ledger',     component: LedgerComponent,     canActivate: [roleGuard(ADMIN_CASHIER), moduleGuard(['restaurant', 'retail', 'pharmacy', 'finance'])] },

      // ── CRM ──────────────────────────────────────────────────
      { path: 'customers',  component: CustomersComponent,  canActivate: [roleGuard(ADMIN_CASHIER), moduleGuard(COMMERCE)] },

      // ── HR ────────────────────────────────────────────────────
      { path: 'employees',  component: EmployeesComponent,  canActivate: [roleGuard(ADMIN), moduleGuard(['restaurant', 'hr_management', 'retail'])] },

      // ── Analytics ─────────────────────────────────────────────
      { path: 'reports',    loadComponent: () => import('../features/reports/reports.component').then(m => m.ReportsComponent),
        canActivate: [roleGuard(ADMIN_CASHIER), moduleGuard(ALL_VERTICALS)] },

      // ── Kitchen Display ───────────────────────────────────────
      { path: 'kds',        loadComponent: () => import('../features/kds/kds.component').then(m => m.KdsComponent),
        canActivate: [roleGuard(ADMIN_CASHIER), moduleGuard(['restaurant'])] },

      // ── Delivery ──────────────────────────────────────────────
      { path: 'delivery',             loadComponent: () => import('../features/delivery/delivery-dashboard/delivery-dashboard.component').then(m => m.DeliveryDashboardComponent),
        canActivate: [roleGuard(ADMIN_CASHIER), moduleGuard(['restaurant', 'retail'])] },
      { path: 'delivery/drivers',     loadComponent: () => import('../features/delivery/delivery-drivers/delivery-drivers.component').then(m => m.DeliveryDriversComponent),
        canActivate: [roleGuard(ADMIN), moduleGuard(['restaurant', 'retail'])] },
      { path: 'delivery/charges',     loadComponent: () => import('../features/delivery/delivery-charges/delivery-charges.component').then(m => m.DeliveryChargesComponent),
        canActivate: [roleGuard(ADMIN), moduleGuard(['restaurant', 'retail'])] },
      { path: 'delivery/third-party', loadComponent: () => import('../features/delivery/delivery-third-party/delivery-third-party.component').then(m => m.DeliveryThirdPartyComponent),
        canActivate: [roleGuard(ADMIN), moduleGuard(['restaurant', 'retail'])] },

      // ── Promotions ────────────────────────────────────────────
      { path: 'promotions', loadComponent: () => import('../features/promotions/promotions.component').then(m => m.PromotionsComponent),
        canActivate: [roleGuard(ADMIN), moduleGuard(['restaurant', 'retail'])] },

      // ── QR Menu ───────────────────────────────────────────────
      { path: 'qr-menu/generate',     loadComponent: () => import('../features/qr-menu/generate-qr/generate-qr.component').then(m => m.GenerateQrComponent),
        canActivate: [roleGuard(ADMIN_CASHIER), moduleGuard(['restaurant'])] },
      { path: 'qr-menu/tables',       loadComponent: () => import('../features/qr-menu/table-qr/table-qr.component').then(m => m.TableQrComponent),
        canActivate: [roleGuard(ADMIN_CASHIER), moduleGuard(['restaurant'])] },
      { path: 'qr-menu/scan',         loadComponent: () => import('../features/qr-menu/scan-qr/scan-qr.component').then(m => m.ScanQrComponent),
        canActivate: [roleGuard(ADMIN_CASHIER), moduleGuard(['restaurant'])] },
      { path: 'qr-menu/digital-menu', loadComponent: () => import('../features/qr-menu/digital-menu/digital-menu.component').then(m => m.DigitalMenuComponent),
        canActivate: [roleGuard(ADMIN), moduleGuard(['restaurant'])] },

      // ── Pharmacy (Micro-frontend remote at port 4201) ─────────
      {
        path: 'pharmacy',
        canActivate: [roleGuard(ADMIN_CASHIER), moduleGuard(['pharmacy'])],
        loadChildren: () => loadRemoteModule('pharmacy', './Routes').then(m => m.PHARMACY_ROUTES)
      },

      // ── System Admin (core – no module gate) ──────────────────
      { path: 'feature-toggles',      loadComponent: () => import('../features/feature-toggles/feature-toggles.component').then(m => m.FeatureTogglesComponent),          canActivate: [roleGuard(ADMIN)] },
      { path: 'integration-settings', loadComponent: () => import('../features/integration-settings/integration-settings.component').then(m => m.IntegrationSettingsComponent), canActivate: [roleGuard(ADMIN)] },

      { path: '**', redirectTo: '' },
    ],
  },
];
