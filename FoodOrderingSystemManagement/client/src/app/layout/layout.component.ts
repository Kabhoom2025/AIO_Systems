import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { trigger, transition, style, animate, query } from '@angular/animations';
import { HeaderComponent } from './header/header.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { ThemeCustomizerComponent } from './theme-customizer/theme-customizer.component';
import { SettingsService } from '../core/services/settings.service';
import { SignalRService } from '../core/services/signalr.service';
import { NotificationStoreService } from '../core/services/notification-store.service';

const routeAnim = trigger('routeAnimations', [
  transition('* <=> *', [
    query(':enter', [
      style({ opacity: 0, transform: 'translateY(10px)' }),
      animate('220ms ease', style({ opacity: 1, transform: 'translateY(0)' })),
    ], { optional: true }),
  ]),
]);

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, HeaderComponent, SidebarComponent, ThemeCustomizerComponent],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss',
  animations: [routeAnim],
})
export class LayoutComponent implements OnInit, OnDestroy {
  readonly settingsService = inject(SettingsService);
  private signalR          = inject(SignalRService);
  // Force singleton creation at layout mount — starts the hub connection
  // before any child component renders.
  private _notifStore      = inject(NotificationStoreService);

  ngOnInit(): void {
    this.settingsService.load().subscribe();
  }

  ngOnDestroy(): void {
    this.signalR.stopConnection();
  }

  get logoUrl(): string | null {
    return this.settingsService.settings()?.logoUrl ?? null;
  }

  getRouteState(outlet: RouterOutlet): string {
    return outlet.isActivated ? (outlet.activatedRoute.snapshot.url[0]?.path ?? 'home') : '';
  }
}
