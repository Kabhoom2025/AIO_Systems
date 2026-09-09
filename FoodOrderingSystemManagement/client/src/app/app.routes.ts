import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'sa/login',
    loadComponent: () =>
      import('./features/sa-auth/sa-login.component').then((m) => m.SaLoginComponent),
  },
  {
    path: 'sa',
    loadChildren: () =>
      import('./layout/sa-layout/sa-layout.routes').then((m) => m.SA_ROUTES),
  },
  {
    // Public customer menu — no auth guard
    path: 'menu',
    loadComponent: () =>
      import('./features/menu/menu.component').then((m) => m.MenuComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./layout/layout.routes').then((m) => m.LAYOUT_ROUTES),
  },
  { path: '**', redirectTo: '' },
];
