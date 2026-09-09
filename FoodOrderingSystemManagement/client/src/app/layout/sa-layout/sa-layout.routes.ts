import { Routes } from '@angular/router';
import { SaLayoutComponent } from './sa-layout.component';
import { saAuthGuard } from '../../core/guards/sa-auth.guard';

export const SA_ROUTES: Routes = [
  {
    path: '',
    component: SaLayoutComponent,
    canActivate: [saAuthGuard],
    children: [
      { path: '', redirectTo: 'monitor', pathMatch: 'full' },
      { path: 'monitor', loadComponent: () => import('../../features/super-admin/monitor/sa-monitor.component').then(m => m.SaMonitorComponent) },
      {
        path: 'organizations',
        loadComponent: () =>
          import('../../features/super-admin/super-admin.component').then(m => m.SuperAdminComponent),
      },
      { path: 'branches',      loadComponent: () => import('../../features/super-admin/branches/sa-branches.component').then(m => m.SaBranchesComponent) },
      { path: 'reports',       loadComponent: () => import('../../features/super-admin/reports/sa-reports.component').then(m => m.SaReportsComponent) },
      { path: 'backup',        loadComponent: () => import('../../features/super-admin/backup/sa-backup.component').then(m => m.SaBackupComponent) },
      { path: 'api-config',    loadComponent: () => import('../../features/super-admin/api-config/sa-api-config.component').then(m => m.SaApiConfigComponent) },
      { path: 'licenses',      loadComponent: () => import('../../features/super-admin/licenses/sa-licenses.component').then(m => m.SaLicensesComponent) },
      { path: 'system-health', loadComponent: () => import('../../features/super-admin/system-health/sa-system-health.component').then(m => m.SaSystemHealthComponent) },
      { path: 'platform-modules', loadComponent: () => import('../../features/super-admin/platform-modules/sa-platform-modules.component').then(m => m.SaPlatformModulesComponent) },
    ],
  },
];
