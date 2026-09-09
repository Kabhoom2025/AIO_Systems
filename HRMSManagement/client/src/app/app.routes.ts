import { Routes } from '@angular/router';
import { authGuard, permissionGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  // Public candidate portal — no auth, order matters: literal 'track/:token' before ':orgCode'.
  {
    path: 'careers/track/:token',
    loadComponent: () => import('./features/careers/careers-track.component').then(m => m.CareersTrackComponent)
  },
  {
    path: 'careers/:orgCode/jobs/:id',
    loadComponent: () => import('./features/careers/careers-apply.component').then(m => m.CareersApplyComponent)
  },
  {
    path: 'careers/:orgCode',
    loadComponent: () => import('./features/careers/careers-list.component').then(m => m.CareersListComponent)
  },
  {
    path: '',
    loadComponent: () => import('./hrms-shell/hrms-shell.component').then(m => m.HrmsShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'employees',
        loadComponent: () => import('./features/employees/employees.component').then(m => m.EmployeesComponent),
        canActivate: [permissionGuard('employees.view')]
      },
      {
        path: 'employees/:id',
        loadComponent: () => import('./features/employees/employee-detail.component').then(m => m.EmployeeDetailComponent),
        canActivate: [permissionGuard('employees.view')]
      },
      {
        path: 'attendance',
        loadComponent: () => import('./features/attendance/attendance.component').then(m => m.AttendanceComponent)
      },
      {
        path: 'leaves',
        loadComponent: () => import('./features/leaves/leaves.component').then(m => m.LeavesComponent)
      },
      {
        path: 'payroll',
        loadComponent: () => import('./features/payroll/payroll.component').then(m => m.PayrollComponent),
        canActivate: [permissionGuard('payroll.view')]
      },
      {
        path: 'recruitment',
        loadComponent: () => import('./features/recruitment/recruitment.component').then(m => m.RecruitmentComponent),
        canActivate: [permissionGuard('recruitment.view')]
      },
      {
        path: 'performance',
        loadComponent: () => import('./features/performance/performance.component').then(m => m.PerformanceComponent),
        canActivate: [permissionGuard('performance.view')]
      },
      {
        path: 'assets',
        loadComponent: () => import('./features/assets/assets.component').then(m => m.AssetsComponent),
        canActivate: [permissionGuard('assets.view')]
      },
      {
        path: 'expenses',
        loadComponent: () => import('./features/expenses/expenses.component').then(m => m.ExpensesComponent)
      },
      {
        path: 'helpdesk',
        loadComponent: () => import('./features/helpdesk/helpdesk.component').then(m => m.HelpdeskComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent),
        canActivate: [permissionGuard('reports.view')]
      },
      {
        path: 'organization',
        loadComponent: () => import('./features/organization/organization.component').then(m => m.OrganizationComponent),
        canActivate: [permissionGuard('organization.view')]
      },
      {
        path: 'users',
        loadComponent: () => import('./features/admin/users.component').then(m => m.UsersComponent),
        canActivate: [permissionGuard('users.view')]
      },
      {
        path: 'roles',
        loadComponent: () => import('./features/admin/roles.component').then(m => m.RolesComponent),
        canActivate: [permissionGuard('roles.view')]
      },
      {
        path: 'audit-logs',
        loadComponent: () => import('./features/admin/audit-logs.component').then(m => m.AuditLogsComponent),
        canActivate: [permissionGuard('audit-logs.view')]
      },
      {
        path: 'workflows',
        loadComponent: () => import('./features/workflows/workflows-list.component').then(m => m.WorkflowsListComponent),
        canActivate: [permissionGuard('workflows.view')]
      },
      {
        path: 'workflows/:id',
        loadComponent: () => import('./features/workflows/workflow-builder.component').then(m => m.WorkflowBuilderComponent),
        canActivate: [permissionGuard('workflows.view')]
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
