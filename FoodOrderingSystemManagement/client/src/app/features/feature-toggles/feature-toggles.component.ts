import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SettingsService } from '../../core/services/settings.service';
import { Settings } from '../../core/models/settings.model';

interface FeatureToggle {
  key: keyof Settings;
  icon: string;
  name: string;
  description: string;
  category: string;
}

const FEATURES: FeatureToggle[] = [
  { key: 'featOnlineOrdering',    icon: 'shopping_cart',   name: 'Online Ordering',      description: 'Enable customers to place orders from the menu page',     category: 'Ordering' },
  { key: 'featTableReservations', icon: 'event_seat',      name: 'Table Reservations',   description: 'Allow customers to book tables in advance',                category: 'Ordering' },
  { key: 'featDeliveryModule',    icon: 'local_shipping',  name: 'Delivery Module',       description: 'Enable delivery order management and tracking',            category: 'Ordering' },
  { key: 'featKitchenDisplay',    icon: 'restaurant',      name: 'Kitchen Display (KDS)', description: 'Show kitchen display screen for kitchen staff',            category: 'Operations' },
  { key: 'featPromotionsCoupons', icon: 'local_offer',     name: 'Promotions & Coupons',  description: 'Enable promotional campaigns and coupon codes',            category: 'Marketing' },
  { key: 'featLoyaltyProgram',    icon: 'loyalty',         name: 'Loyalty Program',       description: 'Enable points earning and redemption for customers',       category: 'Marketing' },
  { key: 'featSmsNotifications',  icon: 'sms',             name: 'SMS Notifications',    description: 'Send SMS alerts to customers for order updates',           category: 'Notifications' },
  { key: 'featWhatsappAlerts',    icon: 'chat',            name: 'WhatsApp Alerts',       description: 'Send WhatsApp messages via UltraMsg for order updates',    category: 'Notifications' },
];

@Component({
  selector: 'app-feature-toggles',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule, MatSlideToggleModule, MatSnackBarModule],
  templateUrl: './feature-toggles.component.html',
  styleUrl: './feature-toggles.component.scss',
})
export class FeatureTogglesComponent implements OnInit {
  private svc   = inject(SettingsService);
  private snack = inject(MatSnackBar);

  loading  = signal(true);
  saving   = signal(false);
  settings = signal<Settings | null>(null);
  features = FEATURES;

  categories = computed(() => [...new Set(FEATURES.map(f => f.category))]);

  featuresFor(cat: string): FeatureToggle[] {
    return FEATURES.filter(f => f.category === cat);
  }

  isEnabled(key: keyof Settings): boolean {
    return !!(this.settings()?.[key]);
  }

  ngOnInit(): void {
    this.svc.getSettings().subscribe({
      next: s => { this.settings.set(s); this.loading.set(false); },
      error: () => { this.loading.set(false); this.snack.open('Failed to load settings', 'Close', { duration: 3000 }); },
    });
  }

  toggle(key: keyof Settings): void {
    this.settings.update(s => s ? { ...s, [key]: !s[key] } : s);
  }

  save(): void {
    const s = this.settings();
    if (!s) return;
    this.saving.set(true);
    this.svc.updateSettings(s).subscribe({
      next: () => {
        this.saving.set(false);
        this.snack.open('Feature toggles saved', 'OK', { duration: 2500 });
      },
      error: () => {
        this.saving.set(false);
        this.snack.open('Failed to save settings', 'Close', { duration: 3000 });
      },
    });
  }
}
