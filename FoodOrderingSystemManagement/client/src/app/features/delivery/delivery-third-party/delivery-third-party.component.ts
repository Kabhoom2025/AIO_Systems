import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { DeliveryService } from '../services/delivery.service';
import { ThirdPartyConfigDto, UpsertThirdPartyConfigDto } from '../models/delivery.model';

interface ProviderTemplate {
  name: string;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-delivery-third-party',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule, MatSnackBarModule, RouterModule],
  templateUrl: './delivery-third-party.component.html',
  styleUrl: './delivery-third-party.component.scss',
})
export class DeliveryThirdPartyComponent implements OnInit, OnDestroy {
  private svc     = inject(DeliveryService);
  private snack   = inject(MatSnackBar);
  private destroy = new Subject<void>();

  loading  = signal(true);
  configs  = signal<ThirdPartyConfigDto[]>([]);
  showForm = signal(false);
  saving   = signal(false);
  showKey  = signal<Record<number, boolean>>({});

  form: UpsertThirdPartyConfigDto = this.emptyForm();

  readonly providers: ProviderTemplate[] = [
    { name: 'Dunzo',        icon: 'electric_scooter', description: 'Hyperlocal delivery platform' },
    { name: 'Swiggy Genie', icon: 'delivery_dining',  description: 'On-demand delivery service' },
    { name: 'Porter',       icon: 'local_shipping',   description: 'Mini-truck & bike deliveries' },
    { name: 'Custom',       icon: 'webhook',          description: 'Your own delivery integration' },
  ];

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.destroy.next(); this.destroy.complete(); }

  load(): void {
    this.loading.set(true);
    this.svc.getThirdPartyConfigs().pipe(takeUntil(this.destroy)).subscribe({
      next: list => { this.configs.set(list); this.loading.set(false); },
      error: () => {
        this.loading.set(false);
        this.snack.open('Failed to load integrations.', 'Retry', { duration: 5000, panelClass: 'snack-error' })
          .onAction().subscribe(() => this.load());
      },
    });
  }

  openConfig(provider: ProviderTemplate): void {
    const existing = this.configs().find(c => c.provider === provider.name);
    this.form = existing
      ? { provider: existing.provider, apiKey: existing.apiKey, apiSecret: existing.apiSecret, webhookUrl: existing.webhookUrl, isEnabled: existing.isEnabled }
      : { provider: provider.name, apiKey: '', apiSecret: '', webhookUrl: '', isEnabled: false };
    this.showForm.set(true);
  }

  save(): void {
    this.saving.set(true);
    this.svc.upsertThirdPartyConfig(this.form).pipe(takeUntil(this.destroy)).subscribe({
      next: () => {
        this.load();
        this.showForm.set(false);
        this.saving.set(false);
        this.snack.open(`${this.form.provider} integration saved.`, 'Close', { duration: 3000, panelClass: 'snack-success' });
      },
      error: (err) => {
        this.saving.set(false);
        const msg = err?.error?.message ?? 'Failed to save integration.';
        this.snack.open(msg, 'Close', { duration: 4000, panelClass: 'snack-error' });
      },
    });
  }

  toggleShowKey(id: number): void {
    this.showKey.update(m => ({ ...m, [id]: !m[id] }));
  }

  isConfigured(name: string): ThirdPartyConfigDto | undefined {
    return this.configs().find(c => c.provider === name);
  }

  private emptyForm(): UpsertThirdPartyConfigDto {
    return { provider: '', apiKey: '', apiSecret: '', webhookUrl: '', isEnabled: false };
  }
}
