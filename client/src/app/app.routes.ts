import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout/admin-layout/admin-layout.component').then((m) => m.AdminLayoutComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'organizations',
        loadComponent: () =>
          import('./features/organizations/organizations-list.component').then(
            (m) => m.OrganizationsListComponent
          ),
      },
      {
        path: 'organizations/:id',
        loadComponent: () =>
          import('./features/organizations/organization-detail.component').then(
            (m) => m.OrganizationDetailComponent
          ),
      },
      {
        path: 'platform-modules',
        loadComponent: () =>
          import('./features/platform-modules/platform-modules.component').then(
            (m) => m.PlatformModulesComponent
          ),
      },
      {
        path: 'registered-services',
        loadComponent: () =>
          import('./features/registered-services/registered-services.component').then(
            (m) => m.RegisteredServicesComponent
          ),
      },
      {
        path: 'apps',
        loadComponent: () =>
          import('./features/apps/apps.component').then((m) => m.AppsComponent),
      },
    ],
  },
  { path: '**', redirectTo: 'login' },
];
