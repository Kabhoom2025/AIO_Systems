import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent)
  },
  {
    // No guard: this is the compact embeddable widget (see public/embed/widget-loader.js) meant
    // to be iframed on a third-party site. It handles its own inline login/register, so it must
    // stay reachable whether or not the visitor is already authenticated in this browser.
    path: 'widget',
    loadComponent: () => import('./features/widget/widget.component').then((m) => m.WidgetChatComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell.component').then((m) => m.ShellComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'chat' },
      {
        path: 'chat',
        loadComponent: () => import('./features/chat/chat.component').then((m) => m.ChatComponent)
      },
      {
        path: 'chat/:conversationId',
        loadComponent: () => import('./features/chat/chat.component').then((m) => m.ChatComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then((m) => m.SettingsComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'chat' }
];
