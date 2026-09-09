import { Routes } from '@angular/router';
import { authGuard, permissionGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./novaerp-shell/novaerp-shell.component').then(m => m.NovaErpShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'my-portal',
        loadComponent: () => import('./features/portal/my-portal/my-portal.component').then(m => m.MyPortalComponent)
      },
      {
        path: 'my-customer-portal',
        loadComponent: () => import('./features/portal/my-customer-portal/my-customer-portal.component').then(m => m.MyCustomerPortalComponent)
      },
      {
        path: 'my-vendor-portal',
        loadComponent: () => import('./features/portal/my-vendor-portal/my-vendor-portal.component').then(m => m.MyVendorPortalComponent)
      },
      {
        path: 'dashboard-builder',
        loadComponent: () => import('./features/dashboard-builder/dashboard-builder.component').then(m => m.DashboardBuilderComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent),
        canActivate: [permissionGuard('reports.view')]
      },
      {
        path: 'ai-assistant',
        loadComponent: () => import('./features/ai-assistant/ai-assistant.component').then(m => m.AiAssistantComponent)
      },
      {
        path: 'organization',
        loadComponent: () => import('./features/organization/organization.component').then(m => m.OrganizationComponent),
        canActivate: [permissionGuard('organization.view')]
      },
      {
        path: 'branches',
        loadComponent: () => import('./features/branches/branches.component').then(m => m.BranchesComponent),
        canActivate: [permissionGuard('branches.view')]
      },
      {
        path: 'departments',
        loadComponent: () => import('./features/departments/departments.component').then(m => m.DepartmentsComponent),
        canActivate: [permissionGuard('departments.view')]
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent),
        canActivate: [permissionGuard('users.view')]
      },
      {
        path: 'users/:id',
        loadComponent: () => import('./features/users/user-detail/user-detail.component').then(m => m.UserDetailComponent),
        canActivate: [permissionGuard('users.view')]
      },
      {
        path: 'roles',
        loadComponent: () => import('./features/roles/roles.component').then(m => m.RolesComponent),
        canActivate: [permissionGuard('roles.view')]
      },
      {
        path: 'audit-logs',
        loadComponent: () => import('./features/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent),
        canActivate: [permissionGuard('audit-logs.view')]
      },
      {
        path: 'notifications',
        loadComponent: () => import('./features/notifications/notifications.component').then(m => m.NotificationsComponent),
        canActivate: [permissionGuard('notifications.view')]
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
        canActivate: [permissionGuard('settings.view')]
      },
      {
        path: 'feature-toggles',
        loadComponent: () => import('./features/feature-toggles/feature-toggles.component').then(m => m.FeatureTogglesComponent),
        canActivate: [permissionGuard('settings.view')]
      },
      {
        path: 'currencies',
        loadComponent: () => import('./features/currencies/currencies.component').then(m => m.CurrenciesComponent),
        canActivate: [permissionGuard('settings.view')]
      },
      {
        path: 'exchange-rates',
        loadComponent: () => import('./features/exchange-rates/exchange-rates.component').then(m => m.ExchangeRatesComponent),
        canActivate: [permissionGuard('settings.view')]
      },
      {
        path: 'languages',
        loadComponent: () => import('./features/languages/languages.component').then(m => m.LanguagesComponent),
        canActivate: [permissionGuard('settings.view')]
      },
      {
        path: 'documents',
        loadComponent: () => import('./features/documents/documents.component').then(m => m.DocumentsComponent),
        canActivate: [permissionGuard('documents.view')]
      },
      {
        path: 'workflow-definitions',
        loadComponent: () => import('./features/workflow-definitions/workflow-definitions.component').then(m => m.WorkflowDefinitionsComponent),
        canActivate: [permissionGuard('workflows.view')]
      },
      {
        path: 'workflow-definitions/new',
        loadComponent: () => import('./features/workflow-definitions/workflow-canvas/workflow-canvas.component').then(m => m.WorkflowCanvasComponent),
        canActivate: [permissionGuard('workflows.create')]
      },
      {
        path: 'workflow-definitions/:id',
        loadComponent: () => import('./features/workflow-definitions/workflow-canvas/workflow-canvas.component').then(m => m.WorkflowCanvasComponent),
        canActivate: [permissionGuard('workflows.view')]
      },
      {
        path: 'workflow-instances',
        loadComponent: () => import('./features/workflow-instances/workflow-instances.component').then(m => m.WorkflowInstancesComponent),
        canActivate: [permissionGuard('workflows.view')]
      },
      {
        path: 'automation-rules',
        loadComponent: () => import('./features/automation-rules/automation-rules.component').then(m => m.AutomationRulesComponent),
        canActivate: [permissionGuard('automation.view')]
      },
      {
        path: 'scheduled-jobs',
        loadComponent: () => import('./features/scheduled-jobs/scheduled-jobs.component').then(m => m.ScheduledJobsComponent),
        canActivate: [permissionGuard('scheduler.view')]
      },
      {
        path: 'notification-channel-settings',
        loadComponent: () => import('./features/notification-channel-settings/notification-channel-settings.component').then(m => m.NotificationChannelSettingsComponent),
        canActivate: [permissionGuard('settings.view')]
      },
      {
        path: 'ai-assistant-settings',
        loadComponent: () => import('./features/ai-assistant-settings/ai-assistant-settings.component').then(m => m.AiAssistantSettingsComponent),
        canActivate: [permissionGuard('settings.view')]
      },
      {
        path: 'shipping-connectors',
        loadComponent: () => import('./features/shipping-connectors/shipping-connectors-list.component').then(m => m.ShippingConnectorsListComponent),
        canActivate: [permissionGuard('carrier-connector.view')]
      },
      {
        path: 'shipping-connectors/:id',
        loadComponent: () => import('./features/shipping-connectors/shipping-connector-edit.component').then(m => m.ShippingConnectorEditComponent),
        canActivate: [permissionGuard('carrier-connector.view')]
      },
      {
        path: 'tax-codes',
        loadComponent: () => import('./features/tax-codes/tax-codes.component').then(m => m.TaxCodesComponent),
        canActivate: [permissionGuard('tax.view')]
      },
      {
        path: 'leads',
        loadComponent: () => import('./features/crm/leads/leads.component').then(m => m.LeadsComponent),
        canActivate: [permissionGuard('crm.view')]
      },
      {
        path: 'accounts',
        loadComponent: () => import('./features/crm/accounts/accounts.component').then(m => m.AccountsComponent),
        canActivate: [permissionGuard('crm.view')]
      },
      {
        path: 'contacts',
        loadComponent: () => import('./features/crm/contacts/contacts.component').then(m => m.ContactsComponent),
        canActivate: [permissionGuard('crm.view')]
      },
      {
        path: 'opportunities',
        loadComponent: () => import('./features/crm/opportunities/opportunities.component').then(m => m.OpportunitiesComponent),
        canActivate: [permissionGuard('crm.view')]
      },
      {
        path: 'sales-orders',
        loadComponent: () => import('./features/sales/sales-orders/sales-orders.component').then(m => m.SalesOrdersComponent),
        canActivate: [permissionGuard('sales.view')]
      },
      {
        path: 'vendors',
        loadComponent: () => import('./features/procurement/vendors/vendors.component').then(m => m.VendorsComponent),
        canActivate: [permissionGuard('procurement.view')]
      },
      {
        path: 'rfqs',
        loadComponent: () => import('./features/procurement/rfqs/rfqs.component').then(m => m.RfqsComponent),
        canActivate: [permissionGuard('procurement.view')]
      },
      {
        path: 'purchase-orders',
        loadComponent: () => import('./features/purchase/purchase-orders/purchase-orders.component').then(m => m.PurchaseOrdersComponent),
        canActivate: [permissionGuard('purchase.view')]
      },
      {
        path: 'products',
        loadComponent: () => import('./features/inventory/products/products.component').then(m => m.ProductsComponent),
        canActivate: [permissionGuard('inventory.view')]
      },
      {
        path: 'stock-movements',
        loadComponent: () => import('./features/inventory/stock-movements/stock-movements.component').then(m => m.StockMovementsComponent),
        canActivate: [permissionGuard('inventory.view')]
      },
      {
        path: 'warehouses',
        loadComponent: () => import('./features/inventory/warehouses/warehouses.component').then(m => m.WarehousesComponent),
        canActivate: [permissionGuard('warehouse.view')]
      },
      {
        path: 'stock-transfers',
        loadComponent: () => import('./features/inventory/stock-transfers/stock-transfers.component').then(m => m.StockTransfersComponent),
        canActivate: [permissionGuard('warehouse.view')]
      },
      {
        path: 'shipments',
        loadComponent: () => import('./features/inventory/shipments/shipments.component').then(m => m.ShipmentsComponent),
        canActivate: [permissionGuard('warehouse.view')]
      },
      {
        path: 'bill-of-materials',
        loadComponent: () => import('./features/manufacturing/bill-of-materials/bill-of-materials.component').then(m => m.BillOfMaterialsComponent),
        canActivate: [permissionGuard('manufacturing.view')]
      },
      {
        path: 'production-orders',
        loadComponent: () => import('./features/manufacturing/production-orders/production-orders.component').then(m => m.ProductionOrdersComponent),
        canActivate: [permissionGuard('manufacturing.view')]
      },
      {
        path: 'ledger-accounts',
        loadComponent: () => import('./features/finance/ledger-accounts/ledger-accounts.component').then(m => m.LedgerAccountsComponent),
        canActivate: [permissionGuard('finance.view')]
      },
      {
        path: 'journal-entries',
        loadComponent: () => import('./features/finance/journal-entries/journal-entries.component').then(m => m.JournalEntriesComponent),
        canActivate: [permissionGuard('finance.view')]
      },
      {
        path: 'vendor-bills',
        loadComponent: () => import('./features/finance/vendor-bills/vendor-bills.component').then(m => m.VendorBillsComponent),
        canActivate: [permissionGuard('finance.view')]
      },
      {
        path: 'customer-invoices',
        loadComponent: () => import('./features/finance/customer-invoices/customer-invoices.component').then(m => m.CustomerInvoicesComponent),
        canActivate: [permissionGuard('finance.view')]
      },
      {
        path: 'bank-reconciliations',
        loadComponent: () => import('./features/finance/bank-reconciliations/bank-reconciliations.component').then(m => m.BankReconciliationsComponent),
        canActivate: [permissionGuard('finance.view')]
      },
      {
        path: 'employees',
        loadComponent: () => import('./features/hrms/employees/employees.component').then(m => m.EmployeesComponent),
        canActivate: [permissionGuard('hrms.view')]
      },
      {
        path: 'leave-types',
        loadComponent: () => import('./features/hrms/leave-types/leave-types.component').then(m => m.LeaveTypesComponent),
        canActivate: [permissionGuard('hrms.view')]
      },
      {
        path: 'leave-requests',
        loadComponent: () => import('./features/hrms/leave-requests/leave-requests.component').then(m => m.LeaveRequestsComponent),
        canActivate: [permissionGuard('hrms.view')]
      },
      {
        path: 'employee-compensation',
        loadComponent: () => import('./features/hrms/employee-compensation/employee-compensation.component').then(m => m.EmployeeCompensationComponent),
        canActivate: [permissionGuard('payroll.view')]
      },
      {
        path: 'pay-runs',
        loadComponent: () => import('./features/hrms/pay-runs/pay-runs.component').then(m => m.PayRunsComponent),
        canActivate: [permissionGuard('payroll.view')]
      },
      {
        path: 'projects',
        loadComponent: () => import('./features/projects/projects/projects.component').then(m => m.ProjectsComponent),
        canActivate: [permissionGuard('projects.view')]
      },
      {
        path: 'project-tasks',
        loadComponent: () => import('./features/projects/project-tasks/project-tasks.component').then(m => m.ProjectTasksComponent),
        canActivate: [permissionGuard('projects.view')]
      },
      {
        path: 'asset-categories',
        loadComponent: () => import('./features/assets/asset-categories/asset-categories.component').then(m => m.AssetCategoriesComponent),
        canActivate: [permissionGuard('assets.view')]
      },
      {
        path: 'assets',
        loadComponent: () => import('./features/assets/assets/assets.component').then(m => m.AssetsComponent),
        canActivate: [permissionGuard('assets.view')]
      },
      {
        path: 'ticket-categories',
        loadComponent: () => import('./features/service-desk/ticket-categories/ticket-categories.component').then(m => m.TicketCategoriesComponent),
        canActivate: [permissionGuard('service-desk.view')]
      },
      {
        path: 'tickets',
        loadComponent: () => import('./features/service-desk/tickets/tickets.component').then(m => m.TicketsComponent),
        canActivate: [permissionGuard('service-desk.view')]
      },
      {
        path: 'pos-sales',
        loadComponent: () => import('./features/pos/pos-sales/pos-sales.component').then(m => m.PosSalesComponent),
        canActivate: [permissionGuard('pos.view')]
      },
      {
        path: 'stores',
        loadComponent: () => import('./features/retail/stores/stores.component').then(m => m.StoresComponent),
        canActivate: [permissionGuard('retail.view')]
      },
      {
        path: 'vehicles',
        loadComponent: () => import('./features/logistics/vehicles/vehicles.component').then(m => m.VehiclesComponent),
        canActivate: [permissionGuard('logistics.view')]
      },
      {
        path: 'delivery',
        loadComponent: () => import('./features/logistics/delivery/delivery.component').then(m => m.DeliveryComponent),
        canActivate: [permissionGuard('logistics.view')]
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
