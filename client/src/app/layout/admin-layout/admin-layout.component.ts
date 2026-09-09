import { Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem {
  label: string;
  path: string;
  icon: string;
}

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss',
})
export class AdminLayoutComponent {
  collapsed = signal(false);

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', path: '/admin/dashboard', icon: 'dashboard' },
    { label: 'Apps', path: '/admin/apps', icon: 'rocket_launch' },
    { label: 'Organizations', path: '/admin/organizations', icon: 'corporate_fare' },
    { label: 'Platform Modules', path: '/admin/platform-modules', icon: 'apps' },
    { label: 'Registered Services', path: '/admin/registered-services', icon: 'dns' },
  ];

  constructor(public auth: AuthService) {}

  toggleSidebar(): void {
    this.collapsed.update((v) => !v);
  }

  logout(): void {
    this.auth.logout();
  }
}
