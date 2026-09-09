import { Routes } from '@angular/router';
import { authGuard, patientAuthGuard, permissionGuard } from './core/auth.guard';

export const PHARMACY_ROUTES: Routes = [
  {
    path: 'welcome',
    loadComponent: () => import('./features/landing/landing.component')
      .then(m => m.LandingComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component')
      .then(m => m.LoginComponent)
  },
  {
    path: 'patient/login',
    loadComponent: () => import('./features/patient-portal/patient-login/patient-login.component')
      .then(m => m.PatientLoginComponent)
  },
  {
    path: 'patient/verify',
    loadComponent: () => import('./features/patient-portal/patient-verify/patient-verify.component')
      .then(m => m.PatientVerifyComponent)
  },
  {
    path: 'patient',
    canActivate: [patientAuthGuard],
    loadComponent: () => import('./features/patient-portal/patient-shell/patient-shell.component')
      .then(m => m.PatientShellComponent),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      {
        path: 'home',
        loadComponent: () => import('./features/patient-portal/patient-home/patient-home.component')
          .then(m => m.PatientHomeComponent)
      },
      {
        path: 'locations',
        loadComponent: () => import('./features/patient-portal/patient-locations/patient-locations.component')
          .then(m => m.PatientLocationsComponent)
      },
      {
        path: 'doctors',
        loadComponent: () => import('./features/patient-portal/patient-doctors/patient-doctors.component')
          .then(m => m.PatientDoctorsComponent)
      },
      {
        path: 'appointments',
        loadComponent: () => import('./features/patient-portal/patient-appointments/patient-appointments.component')
          .then(m => m.PatientAppointmentsComponent)
      },
      {
        path: 'store',
        loadComponent: () => import('./features/patient-portal/patient-store/patient-store.component')
          .then(m => m.PatientStoreComponent)
      },
      {
        path: 'cart',
        loadComponent: () => import('./features/patient-portal/patient-cart/patient-cart.component')
          .then(m => m.PatientCartComponent)
      },
      {
        path: 'orders',
        loadComponent: () => import('./features/patient-portal/patient-orders/patient-orders.component')
          .then(m => m.PatientOrdersComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/patient-portal/patient-profile/patient-profile.component')
          .then(m => m.PatientProfileComponent)
      }
    ]
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./pharmacy-shell/pharmacy-shell.component')
      .then(m => m.PharmacyShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        data: { breadcrumb: 'Dashboard' },
        loadComponent: () => import('./features/dashboard/dashboard.component')
          .then(m => m.DashboardComponent)
      },
      {
        path: 'medicines',
        data: { breadcrumb: 'Medicines' },
        loadComponent: () => import('./features/medicines/medicines.component')
          .then(m => m.MedicinesComponent)
      },
      {
        path: 'prescriptions',
        data: { breadcrumb: 'Prescriptions' },
        loadComponent: () => import('./features/prescriptions/prescriptions.component')
          .then(m => m.PrescriptionsComponent)
      },
      {
        path: 'expiry-alerts',
        data: { breadcrumb: 'Expiry Alerts' },
        loadComponent: () => import('./features/expiry-alerts/expiry-alerts.component')
          .then(m => m.ExpiryAlertsComponent)
      },
      {
        path: 'suppliers',
        data: { breadcrumb: 'Suppliers' },
        loadComponent: () => import('./features/suppliers/suppliers.component')
          .then(m => m.SuppliersComponent)
      },
      {
        path: 'purchase-orders',
        data: { breadcrumb: 'Purchase Orders' },
        loadComponent: () => import('./features/purchase-orders/purchase-orders.component')
          .then(m => m.PurchaseOrdersComponent)
      },
      {
        path: 'goods-receipts',
        data: { breadcrumb: 'Goods Receipts' },
        loadComponent: () => import('./features/goods-receipts/goods-receipts.component')
          .then(m => m.GoodsReceiptsComponent)
      },
      {
        path: 'stock-adjustments',
        data: { breadcrumb: 'Stock Adjustments' },
        loadComponent: () => import('./features/stock-adjustments/stock-adjustments.component')
          .then(m => m.StockAdjustmentsComponent)
      },
      {
        path: 'branches',
        canActivate: [permissionGuard('branches.view')],
        data: { breadcrumb: 'Branches' },
        loadComponent: () => import('./features/branches/branches.component')
          .then(m => m.BranchesComponent)
      },
      {
        path: 'roles',
        canActivate: [permissionGuard('roles.view')],
        data: { breadcrumb: 'Roles & Permissions' },
        loadComponent: () => import('./features/roles/roles.component')
          .then(m => m.RolesComponent)
      },
      {
        path: 'doctors',
        canActivate: [permissionGuard('doctors.view')],
        data: { breadcrumb: 'Doctors' },
        loadComponent: () => import('./features/doctors/doctors.component')
          .then(m => m.DoctorsComponent)
      },
      {
        path: 'patients',
        canActivate: [permissionGuard('patients.view')],
        data: { breadcrumb: 'Patients' },
        loadComponent: () => import('./features/patients/patients.component')
          .then(m => m.PatientsComponent)
      },
      {
        path: 'appointments',
        canActivate: [permissionGuard('appointments.view')],
        data: { breadcrumb: 'Appointments' },
        loadComponent: () => import('./features/appointments/appointments.component')
          .then(m => m.AppointmentsComponent)
      },
      {
        path: 'customers',
        canActivate: [permissionGuard('customers.view')],
        data: { breadcrumb: 'Customers' },
        loadComponent: () => import('./features/customers/customers.component')
          .then(m => m.CustomersComponent)
      },
      {
        path: 'pos',
        canActivate: [permissionGuard('sales.create')],
        data: { breadcrumb: 'Point of Sale' },
        loadComponent: () => import('./features/pos/pos.component')
          .then(m => m.PosComponent)
      },
      {
        path: 'sales-history',
        canActivate: [permissionGuard('sales.view')],
        data: { breadcrumb: 'Sales History' },
        loadComponent: () => import('./features/sales-history/sales-history.component')
          .then(m => m.SalesHistoryComponent)
      },
      {
        path: 'online-orders',
        canActivate: [permissionGuard('sales.view')],
        data: { breadcrumb: 'Online Orders' },
        loadComponent: () => import('./features/online-orders/online-orders.component')
          .then(m => m.OnlineOrdersComponent)
      },
      {
        path: 'expenses',
        canActivate: [permissionGuard('expenses.view')],
        data: { breadcrumb: 'Expenses' },
        loadComponent: () => import('./features/expenses/expenses.component')
          .then(m => m.ExpensesComponent)
      },
      {
        path: 'reports',
        canActivate: [permissionGuard('reports.view')],
        data: { breadcrumb: 'Reports' },
        loadComponent: () => import('./features/reports/reports.component')
          .then(m => m.ReportsComponent)
      },
      {
        path: 'deliveries',
        canActivate: [permissionGuard('deliveries.view')],
        data: { breadcrumb: 'Deliveries' },
        loadComponent: () => import('./features/deliveries/deliveries.component')
          .then(m => m.DeliveriesComponent)
      },
      {
        path: 'ai-insights',
        canActivate: [permissionGuard('reports.view')],
        data: { breadcrumb: 'AI Insights' },
        loadComponent: () => import('./features/ai-insights/ai-insights.component')
          .then(m => m.AiInsightsComponent)
      },
      {
        path: 'settings',
        data: { breadcrumb: 'Settings' },
        loadComponent: () => import('./features/settings/settings.component')
          .then(m => m.SettingsComponent)
      },
      {
        path: 'audit-logs',
        canActivate: [permissionGuard('audit-logs.view')],
        data: { breadcrumb: 'Audit Log' },
        loadComponent: () => import('./features/audit-logs/audit-logs.component')
          .then(m => m.AuditLogsComponent)
      }
    ]
  }
];
