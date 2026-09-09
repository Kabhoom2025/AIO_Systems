import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../core/authentication/auth.service';

@Component({
  selector: 'app-sa-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterLinkActive, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './sa-layout.component.html',
  styleUrl: './sa-layout.component.scss',
})
export class SaLayoutComponent {
  private auth = inject(AuthService);

  sidebarCollapsed = signal(false);

  get userName(): string { return this.auth.currentUser()?.name ?? 'Super Admin'; }

  logout(): void { this.auth.logout(); }
  toggleSidebar(): void { this.sidebarCollapsed.update(v => !v); }
}
