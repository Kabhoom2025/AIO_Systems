import { Component, OnInit, HostListener, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { AuthService } from '../../core/authentication/auth.service';
import { SettingsService } from '../../core/services/settings.service';
import { NotificationStoreService } from '../../core/services/notification-store.service';
import { ThemeService, AppTheme } from '../../core/services/theme.service';
import { PrinterService } from '../../core/services/printer.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatDividerModule,
    MatTooltipModule,
    MatBadgeModule,
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent implements OnInit {
  private authService     = inject(AuthService);
  private settingsService = inject(SettingsService);
  readonly notifStore     = inject(NotificationStoreService);
  readonly themeService   = inject(ThemeService);
  readonly printerService = inject(PrinterService);

  currentUser    = this.authService.currentUser;
  restaurantName = this.settingsService.settings;
  now            = signal(new Date());
  showNotifPanel   = signal(false);
  showThemePanel   = signal(false);
  showPrinterPanel = signal(false);

  ngOnInit(): void {
    setInterval(() => this.now.set(new Date()), 60_000);
    this.printerService.connect();
  }

  logout(): void {
    this.authService.logout();
  }

  get initials(): string {
    const name = this.currentUser()?.name ?? '';
    return name.split(' ').map((n) => n[0]).join('').toUpperCase().slice(0, 2);
  }

  toggleNotifPanel(e: Event): void {
    e.stopPropagation();
    this.showThemePanel.set(false);
    this.showNotifPanel.update(v => !v);
  }

  toggleThemePanel(e: Event): void {
    e.stopPropagation();
    this.showNotifPanel.set(false);
    this.showThemePanel.update(v => !v);
  }

  togglePrinterPanel(e: Event): void {
    e.stopPropagation();
    this.showNotifPanel.set(false);
    this.showThemePanel.set(false);
    this.showPrinterPanel.update(v => !v);
  }

  selectTheme(theme: AppTheme): void {
    this.themeService.apply(theme);
  }

  setReceiptPrinter(name: string): void { this.printerService.setReceiptPrinter(name); }
  setLabelPrinter(name: string): void { this.printerService.setLabelPrinter(name); }

  openDocs(): void {
    window.open(environment.docsUrl, '_blank', 'noopener');
  }

  @HostListener('document:click')
  closePanels(): void {
    this.showNotifPanel.set(false);
    this.showThemePanel.set(false);
    this.showPrinterPanel.set(false);
  }
}
