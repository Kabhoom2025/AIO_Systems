import { Component, OnInit, OnDestroy, inject, signal, ViewChild, ElementRef, NgZone } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { AuthService } from '../../../core/authentication/auth.service';
import { DeliveryService } from '../services/delivery.service';
import { SettingsService } from '../../../core/services/settings.service';
import { GoogleMapsLoaderService } from '../../../core/services/google-maps-loader.service';
import {
  DeliveryDashboardDto, DeliveryOrderDto, DriverDto,
  AssignDriverDto, UpdateDeliveryStatusDto, DELIVERY_STATUSES, DeliveryStatus
} from '../models/delivery.model';

@Component({
  selector: 'app-delivery-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule, MatSnackBarModule, RouterModule, DecimalPipe],
  templateUrl: './delivery-dashboard.component.html',
  styleUrl: './delivery-dashboard.component.scss',
})
export class DeliveryDashboardComponent implements OnInit, OnDestroy {
  private svc         = inject(DeliveryService);
  private auth        = inject(AuthService);
  private snack       = inject(MatSnackBar);
  private settings    = inject(SettingsService);
  private mapsLoader  = inject(GoogleMapsLoaderService);
  private zone        = inject(NgZone);
  private destroy     = new Subject<void>();

  @ViewChild('deliveryMapDiv') mapDiv!: ElementRef<HTMLDivElement>;

  loading   = signal(true);
  mapReady  = signal(false);
  stats    = signal<DeliveryDashboardDto | null>(null);
  orders   = signal<DeliveryOrderDto[]>([]);
  drivers  = signal<DriverDto[]>([]);
  loadError = signal(false);

  filterStatus = signal<string>('all');
  showDriverPanel = signal(false);
  selectedDelivery = signal<DeliveryOrderDto | null>(null);
  selectedDriverId = signal<number | null>(null);
  assignMode = signal<'own' | 'thirdparty'>('own');
  selectedProvider = signal<string | null>(null);
  thirdPartyConfigs = signal<any[]>([]);
  saving = signal(false);

  readonly statuses = DELIVERY_STATUSES;

  get isAdmin(): boolean {
    return this.auth.userRole() === 'Admin';
  }

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.destroy.next(); this.destroy.complete(); }

  load(): void {
    this.loading.set(true);
    this.loadError.set(false);
    forkJoin({
      stats:   this.svc.getDashboard(),
      orders:  this.svc.getOrders(),
      drivers: this.svc.getDrivers(),
    }).pipe(takeUntil(this.destroy)).subscribe({
      next: ({ stats, orders, drivers }) => {
        this.stats.set(stats);
        this.orders.set(orders);
        this.drivers.set(drivers);
        this.loading.set(false);
        this.initMap();
      },
      error: (err) => {
        this.loading.set(false);
        this.loadError.set(true);
        this.snack.open('Failed to load delivery data. Check your connection.', 'Retry', { duration: 5000, panelClass: 'snack-error' })
          .onAction().subscribe(() => this.load());
      },
    });
  }

  get filteredOrders(): DeliveryOrderDto[] {
    const f = this.filterStatus();
    return f === 'all' ? this.orders() : this.orders().filter(o => o.status === f);
  }

  get availableDrivers(): DriverDto[] {
    return this.drivers().filter(d => d.isAvailable && d.isActive);
  }

  openAssign(order: DeliveryOrderDto): void {
    this.selectedDelivery.set(order);
    this.selectedDriverId.set(null);
    this.selectedProvider.set(null);
    this.assignMode.set('own');
    this.showDriverPanel.set(true);
    if (this.thirdPartyConfigs().length === 0) {
      this.svc.getThirdPartyConfigs().pipe(takeUntil(this.destroy)).subscribe({
        next: list => this.thirdPartyConfigs.set(list.filter(c => c.isEnabled)),
      });
    }
  }

  confirmAssign(): void {
    const delivery = this.selectedDelivery();
    if (!delivery) return;
    this.saving.set(true);

    const obs = this.assignMode() === 'thirdparty' && this.selectedProvider()
      ? this.svc.dispatchToThirdParty(delivery.id, this.selectedProvider()!)
      : this.assignMode() === 'own' && this.selectedDriverId()
        ? this.svc.assignDriver(delivery.id, { driverId: this.selectedDriverId()! })
        : null;

    if (!obs) { this.saving.set(false); return; }

    obs.pipe(takeUntil(this.destroy)).subscribe({
      next: updated => {
        this.orders.update(list => list.map(o => o.id === updated.id ? updated : o));
        this.showDriverPanel.set(false);
        this.saving.set(false);
        const msg = this.assignMode() === 'thirdparty'
          ? `Dispatched via ${this.selectedProvider()}`
          : 'Driver assigned successfully';
        this.snack.open(msg, 'Close', { duration: 3000, panelClass: 'snack-success' });
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        const msg = err?.error?.message ?? 'Failed to assign driver. Please try again.';
        this.snack.open(msg, 'Close', { duration: 5000, panelClass: 'snack-error' });
      },
    });
  }

  updateStatus(order: DeliveryOrderDto, status: string): void {
    const dto: UpdateDeliveryStatusDto = { status };
    this.svc.updateStatus(order.id, dto).pipe(takeUntil(this.destroy)).subscribe({
      next: updated => {
        this.orders.update(list => list.map(o => o.id === updated.id ? updated : o));
        this.snack.open(`Order #${order.orderNumber} marked as ${status}`, 'Close', { duration: 2500, panelClass: 'snack-success' });
        this.load();
      },
      error: (err) => {
        const msg = err?.error?.message ?? 'Failed to update status.';
        this.snack.open(msg, 'Close', { duration: 4000, panelClass: 'snack-error' });
      },
    });
  }

  // ── Map ──────────────────────────────────────────────────────────────────

  private initMap(): void {
    const s = this.settings.settings();
    const apiKey = s?.googleMapsApiKey?.trim();
    if (!apiKey) return;

    this.mapsLoader.load(apiKey).then(() => {
      setTimeout(() => this.renderMap(), 100);
    }).catch(() => {});
  }

  private renderMap(): void {
    if (!this.mapDiv?.nativeElement || !window['google']?.maps) return;
    const s = this.settings.settings();
    const restLat = s?.restaurantLatitude  ? +s.restaurantLatitude  : null;
    const restLng = s?.restaurantLongitude ? +s.restaurantLongitude : null;

    const center = restLat && restLng
      ? { lat: restLat, lng: restLng }
      : { lat: 20.5937, lng: 78.9629 };

    const map = new window['google'].maps.Map(this.mapDiv.nativeElement, {
      center, zoom: restLat ? 12 : 5,
      mapTypeControl: false, streetViewControl: false,
    });

    // Restaurant marker
    if (restLat && restLng) {
      new window['google'].maps.Marker({
        position: { lat: restLat, lng: restLng }, map,
        title: s?.restaurantName ?? 'Restaurant',
        icon: { url: 'https://maps.google.com/mapfiles/ms/icons/blue-dot.png' },
      });
    }

    // Delivery order markers (only those with coordinates)
    const active = ['Pending','Assigned','PickedUp','OutForDelivery'];
    this.orders()
      .filter(o => active.includes(o.status) && o.deliveryLatitude && o.deliveryLongitude)
      .forEach(o => {
        const pos = { lat: +o.deliveryLatitude!, lng: +o.deliveryLongitude! };
        const info = new window['google'].maps.InfoWindow({
          content: `<b>#${o.orderNumber}</b><br>${o.deliveryAddress}<br>Status: ${o.status}`,
        });
        const marker = new window['google'].maps.Marker({
          position: pos, map, title: `#${o.orderNumber}`,
          icon: { url: 'https://maps.google.com/mapfiles/ms/icons/orange-dot.png' },
        });
        marker.addListener('click', () => info.open(map, marker));
      });

    // Driver markers
    this.drivers()
      .filter(d => d.isActive && d.currentLatitude && d.currentLongitude)
      .forEach(d => {
        const pos = { lat: +d.currentLatitude!, lng: +d.currentLongitude! };
        const updated = d.lastLocationUpdate
          ? new Date(d.lastLocationUpdate).toLocaleTimeString() : 'unknown';
        const info = new window['google'].maps.InfoWindow({
          content: `<b>${d.name}</b><br>${d.vehicleType ?? ''} ${d.vehicleNo ?? ''}<br>Last update: ${updated}`,
        });
        const marker = new window['google'].maps.Marker({
          position: pos, map, title: d.name,
          icon: { url: 'https://maps.google.com/mapfiles/ms/icons/green-dot.png' },
        });
        marker.addListener('click', () => info.open(map, marker));
      });

    this.zone.run(() => this.mapReady.set(true));
  }

  statusIcon(s: string): string {
    const m: Record<string, string> = {
      Pending: 'hourglass_empty', Assigned: 'person_pin_circle',
      PickedUp: 'directions_bike', OutForDelivery: 'local_shipping',
      Delivered: 'check_circle', Failed: 'cancel', Cancelled: 'block',
    };
    return m[s] ?? 'help';
  }

  nextStatuses(current: string): string[] {
    const flow: Record<string, string[]> = {
      Pending: ['Assigned', 'Cancelled'],
      Assigned: ['PickedUp', 'Cancelled'],
      PickedUp: ['OutForDelivery', 'Failed'],
      OutForDelivery: ['Delivered', 'Failed'],
      Delivered: [], Failed: [], Cancelled: [],
    };
    return flow[current] ?? [];
  }
}
