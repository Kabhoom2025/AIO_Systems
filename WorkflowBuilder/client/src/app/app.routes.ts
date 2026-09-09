import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./shell/shell.component').then(m => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'workflows' },
      {
        path: 'workflows',
        loadComponent: () => import('./features/workflows/workflows-list.component').then(m => m.WorkflowsListComponent)
      },
      {
        path: 'workflows/:id',
        loadComponent: () => import('./features/workflows/workflow-builder.component').then(m => m.WorkflowBuilderComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
