import { Component, inject, OnInit, signal, ElementRef, ViewChild, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../core/models/api-response.model';
import { Settings } from '../../core/models/settings.model';
import { SettingsService } from '../../core/services/settings.service';
import { NotificationService } from '../../shared/services/notification.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { GoogleMapsLoaderService } from '../../core/services/google-maps-loader.service';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatSelectModule,
    MatTooltipModule,
    LoadingSpinnerComponent,
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent implements OnInit {
  private fb              = inject(FormBuilder);
  private http            = inject(HttpClient);
  private settingsService = inject(SettingsService);
  private notify          = inject(NotificationService);
  private mapsLoader      = inject(GoogleMapsLoaderService);
  private zone            = inject(NgZone);
  readonly theme          = inject(ThemeService);

  @ViewChild('fileInput')    fileInput!: ElementRef<HTMLInputElement>;
  @ViewChild('restaurantMapDiv') mapDiv!: ElementRef<HTMLDivElement>;

  form!: FormGroup;
  loading      = signal(true);
  loadError    = signal(false);
  saving       = signal(false);
  logoPreview  = signal<string | null>(null);
  mapLoading   = signal(false);
  mapError     = signal<string | null>(null);
  showApiKey              = signal(false);
  showGoogleSecret        = signal(false);
  showMsSecret            = signal(false);
  showFbSecret            = signal(false);
  showGhSecret            = signal(false);

  private _map: any = null;
  private _marker: any = null;

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.settingsService.load().subscribe({
      next: (res) => {
        const s = res.data;
        this.form = this.fb.group({
          // ── Existing fields ────────────────────────────────────────────────
          restaurantName: [s.restaurantName,   [Validators.required, Validators.maxLength(200)]],
          address:        [s.address        ?? '', Validators.maxLength(500)],
          phone:          [s.phone          ?? '', Validators.maxLength(20)],
          gstNumber:      [s.gstNumber      ?? '', Validators.maxLength(50)],
          taxPercentage:  [s.taxPercentage,   [Validators.required, Validators.min(0), Validators.max(100)]],
          logoUrl:            [s.logoUrl            ?? null],
          pointsPerRupee:     [s.pointsPerRupee  ?? 1.0,  [Validators.required, Validators.min(0)]],
          pointsRedeemRate:   [s.pointsRedeemRate ?? 0.25, [Validators.required, Validators.min(0)]],
          upiId:              [s.upiId ?? ''],
          promoAutoScroll:    [s.promoAutoScroll ?? true],

          // ── POS Settings ──────────────────────────────────────────────────
          posDefaultOrderType:     [s.posDefaultOrderType     ?? 'DineIn'],
          posAutoPrintBill:        [s.posAutoPrintBill        ?? false],
          posEnableTips:           [s.posEnableTips           ?? false],
          posRequireCustomerPhone: [s.posRequireCustomerPhone ?? false],

          // ── Menu Settings ─────────────────────────────────────────────────
          menuShowImages:       [s.menuShowImages       ?? true],
          menuShowDescriptions: [s.menuShowDescriptions ?? true],
          menuShowOutOfStock:   [s.menuShowOutOfStock   ?? true],

          // ── Tax Configuration (extended) ──────────────────────────────────
          taxName:      [s.taxName      ?? 'GST', Validators.maxLength(50)],
          taxInclusive: [s.taxInclusive ?? false],

          // ── Printer Settings ──────────────────────────────────────────────
          printerEnabled:   [s.printerEnabled   ?? false],
          printerType:      [s.printerType      ?? 'Thermal'],
          printerIp:        [s.printerIp        ?? ''],
          printerPort:      [s.printerPort      ?? 9100, [Validators.min(1), Validators.max(65535)]],
          printerAutoPrint: [s.printerAutoPrint ?? false],

          // ── Payment Gateway (extended) ────────────────────────────────────
          paymentCashEnabled:   [s.paymentCashEnabled   ?? true],
          paymentCardEnabled:   [s.paymentCardEnabled   ?? true],
          paymentOnlineEnabled: [s.paymentOnlineEnabled ?? false],

          // ── Email Configuration ───────────────────────────────────────────
          smtpHost:      [s.smtpHost      ?? ''],
          smtpPort:      [s.smtpPort      ?? 587, [Validators.min(1), Validators.max(65535)]],
          smtpUsername:  [s.smtpUsername  ?? ''],
          smtpPassword:  [s.smtpPassword  ?? ''],
          smtpFromEmail: [s.smtpFromEmail ?? ''],
          smtpFromName:  [s.smtpFromName  ?? ''],
          smtpSsl:       [s.smtpSsl       ?? true],

          // ── Backup Settings ───────────────────────────────────────────────
          autoBackupEnabled:    [s.autoBackupEnabled    ?? false],
          backupEmail:          [s.backupEmail          ?? ''],
          backupFrequencyHours: [s.backupFrequencyHours ?? 24, [Validators.min(1)]],

          // ── Theme Settings ────────────────────────────────────────────────
          themePrimaryColor: [s.themePrimaryColor ?? '#bf360c'],
          themeMode:         [s.themeMode         ?? 'light'],

          // ── Google Maps ───────────────────────────────────────────────────
          googleMapsApiKey:    [s.googleMapsApiKey    ?? ''],
          restaurantLatitude:  [s.restaurantLatitude  ?? null, [Validators.min(-90),  Validators.max(90)]],
          restaurantLongitude: [s.restaurantLongitude ?? null, [Validators.min(-180), Validators.max(180)]],

          // ── SSO Provider Credentials ──────────────────────────────────────
          ssoGoogleClientId:        [s.ssoGoogleClientId        ?? ''],
          ssoGoogleClientSecret:    [s.ssoGoogleClientSecret    ?? ''],
          ssoMicrosoftClientId:     [s.ssoMicrosoftClientId     ?? ''],
          ssoMicrosoftClientSecret: [s.ssoMicrosoftClientSecret ?? ''],
          ssoMicrosoftTenantId:     [s.ssoMicrosoftTenantId     ?? 'common'],
          ssoFacebookAppId:         [s.ssoFacebookAppId         ?? ''],
          ssoFacebookAppSecret:     [s.ssoFacebookAppSecret     ?? ''],
          ssoGitHubClientId:        [s.ssoGitHubClientId        ?? ''],
          ssoGitHubClientSecret:    [s.ssoGitHubClientSecret    ?? ''],
        });
        this.logoPreview.set(s.logoUrl ?? null);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load settings.');
        this.loadError.set(true);
        this.loading.set(false);
      },
    });
  }

  get restaurantName() { return this.form.get('restaurantName')!; }
  get taxPercentage()  { return this.form.get('taxPercentage')!; }

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }

  onLogoSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      this.notify.error('Image must be under 2 MB.');
      return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
      const base64 = e.target?.result as string;
      this.logoPreview.set(base64);
      this.form.get('logoUrl')!.setValue(base64);
    };
    reader.readAsDataURL(file);
  }

  removeLogo(): void {
    this.logoPreview.set(null);
    this.form.get('logoUrl')!.setValue(null);
    if (this.fileInput) this.fileInput.nativeElement.value = '';
  }

  // ── Google Maps helpers ──────────────────────────────────────────────────

  onMapPanelOpened(): void {
    const key = this.form.get('googleMapsApiKey')?.value?.trim();
    if (!key) { this.mapError.set('Enter a Google Maps API key above and save, then reopen this panel.'); return; }
    this.mapError.set(null);
    this.mapLoading.set(true);
    this.mapsLoader.load(key).then(() => {
      this.mapLoading.set(false);
      setTimeout(() => this.initMap(), 50);
    }).catch(() => {
      this.mapLoading.set(false);
      this.mapError.set('Failed to load Google Maps. Check that your API key is valid and has Maps JS API enabled.');
    });
  }

  private initMap(): void {
    if (!this.mapDiv?.nativeElement || !window['google']?.maps) return;
    const lat = +(this.form.get('restaurantLatitude')?.value  ?? 20.5937);
    const lng = +(this.form.get('restaurantLongitude')?.value ?? 78.9629);
    const center = { lat, lng };

    this._map = new window['google'].maps.Map(this.mapDiv.nativeElement, {
      center, zoom: lat === 20.5937 ? 5 : 15,
      mapTypeControl: false, streetViewControl: false,
    });

    if (lat !== 20.5937) {
      this._marker = new window['google'].maps.Marker({ position: center, map: this._map, title: 'Restaurant Location', draggable: true });
      this._marker.addListener('dragend', (e: any) => this.zone.run(() => this.setLatLng(e.latLng.lat(), e.latLng.lng())));
    }

    this._map.addListener('click', (e: any) => {
      this.zone.run(() => {
        this.setLatLng(e.latLng.lat(), e.latLng.lng());
        if (this._marker) { this._marker.setPosition(e.latLng); }
        else {
          this._marker = new window['google'].maps.Marker({ position: e.latLng, map: this._map, title: 'Restaurant Location', draggable: true });
          this._marker.addListener('dragend', (ev: any) => this.zone.run(() => this.setLatLng(ev.latLng.lat(), ev.latLng.lng())));
        }
      });
    });
  }

  private setLatLng(lat: number, lng: number): void {
    this.form.get('restaurantLatitude')!.setValue(+lat.toFixed(7));
    this.form.get('restaurantLongitude')!.setValue(+lng.toFixed(7));
  }

  onLatLngInputChange(): void {
    if (!this._map || !this._marker) return;
    const lat = +(this.form.get('restaurantLatitude')?.value ?? 0);
    const lng = +(this.form.get('restaurantLongitude')?.value ?? 0);
    if (lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180) {
      const pos = { lat, lng };
      this._marker.setPosition(pos);
      this._map.panTo(pos);
    }
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    this.http.put<ApiResponse<Settings>>(
      `${environment.apiUrl}/settings`,
      this.form.value
    ).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.notify.success('Settings saved successfully.');
          this.settingsService.load().subscribe();
        } else {
          this.notify.error(res.message || 'Save failed.');
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.notify.error(err?.error?.message || 'Save failed.');
      },
    });
  }
}
